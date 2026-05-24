using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.PowerMode;
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
    TextEnhancementPipeline? enhancementPipeline = null,
    IPowerModeTargetProvider? powerModeTargetProvider = null)
{
    private readonly SemaphoreSlim lifecycleGate = new(1, 1);
    private readonly IDictionaryStore dictionaryStore = dictionaryStore ?? EmptyDictionaryStore.Instance;
    private PowerModeResolution? activePowerModeResolution;

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
                var powerModeResolution = await ResolvePowerModeAsync(settings, cancellationToken);
                var configurationError = TranscriptionConfiguration.ValidateRequiredSettings(
                    powerModeResolution.EffectiveSettings);
                if (configurationError is not null)
                {
                    State = DictationState.Error;
                    LastError = configurationError;
                    return;
                }

                State = DictationState.Recording;
                await audioCapture.StartAsync(cancellationToken);
                activePowerModeResolution = powerModeResolution;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                activePowerModeResolution = null;
                State = DictationState.Idle;
                throw;
            }
            catch (Exception ex)
            {
                activePowerModeResolution = null;
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

                var powerModeResolution = activePowerModeResolution
                    ?? PowerModeMatcher.Resolve(await settingsStore.LoadAsync(cancellationToken), target: null);
                var settings = powerModeResolution.EffectiveSettings;
                var vocabulary = await this.dictionaryStore.ListVocabularyAsync(cancellationToken);
                var replacements = await this.dictionaryStore.ListReplacementsAsync(cancellationToken);
                var vocabularyPrompt = DictionaryService.RenderVocabularyPrompt(vocabulary);

                State = DictationState.Transcribing;
                var transcription = await transcriptionService.TranscribeAsync(
                    audio,
                    TranscriptionConfiguration.BuildOptions(settings, vocabularyPrompt),
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
                            modelPath: TranscriptionConfiguration.ModelMetadata(settings),
                            promptName: enhancement?.PromptName,
                            enhancementDuration: enhancement?.EnhancementDuration,
                            errorMessage: enhancement?.WarningMessage,
                            audioFilePath: audio.FilePath,
                            enhancedText: enhancement?.EnhancedText,
                            enhancementProviderName: enhancement?.EnhancementProviderName,
                            enhancementModelName: enhancement?.EnhancementModelName,
                            aiRequestSystemMessage: enhancement?.SystemMessage,
                            aiRequestUserMessage: enhancement?.UserMessage,
                            powerModeName: powerModeResolution.PowerModeName,
                            powerModeEmoji: powerModeResolution.PowerModeEmoji),
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
            finally
            {
                activePowerModeResolution = null;
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

                var powerModeResolution = activePowerModeResolution
                    ?? PowerModeMatcher.Resolve(await settingsStore.LoadAsync(cancellationToken), target: null);
                var settings = powerModeResolution.EffectiveSettings;
                try
                {
                    await historyStore.SaveAsync(
                        new TranscriptionHistoryItem(
                            Guid.NewGuid(),
                            DateTimeOffset.UtcNow,
                            TranscriptionHistoryItem.CanceledTranscriptionText,
                            TranscriptionConfiguration.ProviderName(settings),
                            audio.Duration,
                            TimeSpan.Zero,
                            originalText: TranscriptionHistoryItem.CanceledTranscriptionText,
                            status: TranscriptionHistoryStatus.Canceled,
                            language: settings.Language,
                            modelPath: TranscriptionConfiguration.ModelMetadata(settings),
                            audioFilePath: audio.FilePath,
                            powerModeName: powerModeResolution.PowerModeName,
                            powerModeEmoji: powerModeResolution.PowerModeEmoji),
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
            finally
            {
                activePowerModeResolution = null;
            }
        }
        finally
        {
            lifecycleGate.Release();
        }
    }

    private async Task<PowerModeResolution> ResolvePowerModeAsync(
        AppSettings settings,
        CancellationToken cancellationToken)
    {
        if (powerModeTargetProvider is null)
        {
            return PowerModeMatcher.Resolve(settings, target: null);
        }

        try
        {
            var target = await powerModeTargetProvider.GetCurrentTargetAsync(cancellationToken);
            return PowerModeMatcher.Resolve(settings, target);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LastWarning = $"Power Mode detection failed: {ex.Message}";
            return PowerModeMatcher.Resolve(settings, target: null);
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
