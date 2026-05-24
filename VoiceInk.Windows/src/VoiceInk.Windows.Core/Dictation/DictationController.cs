using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Text;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Core.Dictation;

public sealed class DictationController(
    IAudioCaptureService audioCapture,
    ITranscriptionService transcriptionService,
    ITextInjectionService textInjection,
    IHistoryStore historyStore,
    ISettingsStore settingsStore,
    IDictionaryStore? dictionaryStore = null)
{
    private readonly SemaphoreSlim lifecycleGate = new(1, 1);
    private readonly IDictionaryStore dictionaryStore = dictionaryStore ?? EmptyDictionaryStore.Instance;

    public DictationState State { get; private set; } = DictationState.Idle;
    public string? LastError { get; private set; }
    public string? LastWarning { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!await lifecycleGate.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            if (State != DictationState.Idle)
            {
                return;
            }

            LastError = null;
            LastWarning = null;

            try
            {
                var settings = await settingsStore.LoadAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(settings.ModelPath))
                {
                    State = DictationState.Error;
                    LastError = "Local whisper model path is required.";
                    return;
                }

                State = DictationState.Recording;
                await audioCapture.StartAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                State = DictationState.Idle;
                throw;
            }
            catch (Exception ex)
            {
                State = DictationState.Error;
                LastError = ex.Message;
            }
        }
        finally
        {
            lifecycleGate.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!await lifecycleGate.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            if (State != DictationState.Recording)
            {
                return;
            }

            LastError = null;
            LastWarning = null;

            try
            {
                // Always let capture stop release recorder resources before honoring caller cancellation.
                var audio = await audioCapture.StopAsync(CancellationToken.None);
                cancellationToken.ThrowIfCancellationRequested();

                var settings = await settingsStore.LoadAsync(cancellationToken);
                var vocabulary = await this.dictionaryStore.ListVocabularyAsync(cancellationToken);
                var replacements = await this.dictionaryStore.ListReplacementsAsync(cancellationToken);
                var vocabularyPrompt = DictionaryService.RenderVocabularyPrompt(vocabulary);

                State = DictationState.Transcribing;
                var transcription = await transcriptionService.TranscribeAsync(
                    audio,
                    new TranscriptionOptions(settings.ModelPath, settings.Language, vocabularyPrompt),
                    cancellationToken);

                var finalText = TextPostProcessor.Process(
                    transcription.Text,
                    new TextPostProcessingOptions(
                        AppendTrailingSpace: settings.AppendTrailingSpace,
                        RemoveFillerWords: settings.RemoveFillerWords,
                        WordReplacements: replacements,
                        PunctuationCleanupMode: settings.PunctuationCleanupMode,
                        LowercaseTranscription: settings.LowercaseTranscription));

                if (finalText.Length == 0)
                {
                    State = DictationState.Idle;
                    return;
                }

                State = DictationState.Inserting;
                await textInjection.InsertAsync(finalText, cancellationToken);

                try
                {
                    await historyStore.SaveAsync(
                        new TranscriptionHistoryItem(
                            Guid.NewGuid(),
                            DateTimeOffset.UtcNow,
                            finalText,
                            transcription.ProviderName,
                            audio.Duration,
                            transcription.Duration,
                            originalText: transcription.Text,
                            status: TranscriptionHistoryStatus.Completed,
                            language: settings.Language,
                            modelPath: settings.ModelPath),
                        cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    State = DictationState.Idle;
                    throw;
                }
                catch (Exception ex)
                {
                    LastWarning = $"History save failed: {ex.Message}";
                }

                State = DictationState.Idle;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                State = DictationState.Idle;
                throw;
            }
            catch (Exception ex)
            {
                State = DictationState.Error;
                LastError = ex.Message;
            }
        }
        finally
        {
            lifecycleGate.Release();
        }
    }

    private sealed class EmptyDictionaryStore : IDictionaryStore
    {
        public static EmptyDictionaryStore Instance { get; } = new();

        public Task<IReadOnlyList<VocabularyWord>> ListVocabularyAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<VocabularyWord>>([]);
        }

        public Task<IReadOnlyList<WordReplacement>> ListReplacementsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<WordReplacement>>([]);
        }
    }
}
