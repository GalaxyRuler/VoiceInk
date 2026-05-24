using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Text;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Core.Dictation;

public sealed class DictationController(
    IAudioCaptureService audioCapture,
    ITranscriptionService transcriptionService,
    ITextInjectionService textInjection,
    IHistoryStore historyStore,
    ISettingsStore settingsStore,
    IDictionaryStore? dictionaryStore = null,
    TextEnhancementPipeline? enhancementPipeline = null)
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

                var enhancement = enhancementPipeline is null
                    ? null
                    : await enhancementPipeline.EnhanceAsync(finalText, settings, vocabulary, cancellationToken);
                if (enhancement?.WarningMessage is not null)
                {
                    LastWarning = enhancement.WarningMessage;
                }

                State = DictationState.Inserting;
                await textInjection.InsertAsync(enhancement?.FinalText ?? finalText, cancellationToken);

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
                            modelPath: settings.ModelPath,
                            promptName: enhancement?.PromptName,
                            enhancementDuration: enhancement?.EnhancementDuration,
                            errorMessage: enhancement?.WarningMessage,
                            audioFilePath: audio.FilePath,
                            enhancedText: enhancement?.EnhancedText,
                            enhancementProviderName: enhancement?.EnhancementProviderName,
                            enhancementModelName: enhancement?.EnhancementModelName,
                            aiRequestSystemMessage: enhancement?.SystemMessage,
                            aiRequestUserMessage: enhancement?.UserMessage),
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

    public async Task CancelAsync(CancellationToken cancellationToken)
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
                // Always release recorder resources before honoring caller cancellation.
                var audio = await audioCapture.StopAsync(CancellationToken.None);
                cancellationToken.ThrowIfCancellationRequested();

                var settings = await settingsStore.LoadAsync(cancellationToken);
                try
                {
                    await historyStore.SaveAsync(
                        new TranscriptionHistoryItem(
                            Guid.NewGuid(),
                            DateTimeOffset.UtcNow,
                            TranscriptionHistoryItem.CanceledTranscriptionText,
                            ProviderName(settings),
                            audio.Duration,
                            TimeSpan.Zero,
                            originalText: TranscriptionHistoryItem.CanceledTranscriptionText,
                            status: TranscriptionHistoryStatus.Canceled,
                            language: settings.Language,
                            modelPath: settings.ModelPath,
                            audioFilePath: audio.FilePath),
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

    private static string ProviderName(AppSettings settings) =>
        settings.TranscriptionProvider switch
        {
            TranscriptionProviderKind.LocalWhisper => "local-whisper",
            _ => settings.TranscriptionProvider.ToString()
        };

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
