using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Metrics;
using VoiceInk.Windows.Core.PowerMode;
using VoiceInk.Windows.Core.Recording;
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
    IPowerModeTargetProvider? powerModeTargetProvider = null,
    ISessionMetricStore? sessionMetricStore = null,
    IRecordingCaptureStopFeedback? recordingCaptureStopFeedback = null,
    ILiveTranscriptionPreviewService? liveTranscriptionPreviewService = null,
    IPowerModeAutoSendService? powerModeAutoSendService = null,
    IVoiceActivityDetector? voiceActivityDetector = null)
{
    private readonly SemaphoreSlim lifecycleGate = new(1, 1);
    private readonly IDictionaryStore dictionaryStore = dictionaryStore ?? EmptyDictionaryStore.Instance;
    private readonly IAudioChunkPublisher? audioChunkPublisher = audioCapture as IAudioChunkPublisher;
    private PowerModeResolution? activePowerModeResolution;
    private ILiveTranscriptionPreviewSession? liveTranscriptionPreviewSession;
    private bool isLivePreviewSubscribed;
    private bool isAcceptingPartialTranscript;

    public DictationState State { get; private set; } = DictationState.Idle;
    public string? LastError { get; private set; }
    public string? LastWarning { get; private set; }
    public bool LastStopInsertedText { get; private set; }
    public string PartialTranscript { get; private set; } = string.Empty;

    public void UpdatePartialTranscript(string? transcript)
    {
        if (State != DictationState.Recording || !isAcceptingPartialTranscript)
        {
            return;
        }

        PartialTranscript = transcript?.Trim() ?? string.Empty;
    }

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
            LastStopInsertedText = false;
            ResetPartialTranscript();

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
                isAcceptingPartialTranscript = true;
                activePowerModeResolution = powerModeResolution;
                await StartLiveTranscriptionPreviewAsync(powerModeResolution.EffectiveSettings, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                activePowerModeResolution = null;
                await StopLiveTranscriptionPreviewAsync(CancellationToken.None);
                State = DictationState.Idle;
                ResetPartialTranscript();
                throw;
            }
            catch (Exception ex)
            {
                activePowerModeResolution = null;
                await StopLiveTranscriptionPreviewAsync(CancellationToken.None);
                State = DictationState.Error;
                LastError = ex.Message;
                ResetPartialTranscript();
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
            LastStopInsertedText = false;
            ResetPartialTranscript();
            await StopLiveTranscriptionPreviewAsync(CancellationToken.None);

            try
            {
                // Always let capture stop release recorder resources before honoring caller cancellation.
                var audio = await audioCapture.StopAsync(CancellationToken.None);
                await NotifyCaptureStoppedAsync(CancellationToken.None);
                cancellationToken.ThrowIfCancellationRequested();
                var hasCompletedHistory = false;
                var hasInsertedText = false;

                var powerModeResolution = await ResolveCurrentPowerModeForCompletionAsync(cancellationToken);
                var settings = powerModeResolution.EffectiveSettings;

                try
                {
                    var vocabulary = await this.dictionaryStore.ListVocabularyAsync(cancellationToken);
                    var replacements = await this.dictionaryStore.ListReplacementsAsync(cancellationToken);
                    var vocabularyPrompt = DictionaryService.RenderVocabularyPrompt(vocabulary);

                    if (settings.IsVadEnabled && voiceActivityDetector is not null)
                    {
                        var voiceActivity = await voiceActivityDetector.AnalyzeAsync(audio, cancellationToken);
                        if (!voiceActivity.HasSpeech)
                        {
                            State = DictationState.Idle;
                            LastWarning = "No speech detected";
                            ResetPartialTranscript();
                            return;
                        }
                    }

                    State = DictationState.Transcribing;
                    var transcription = await transcriptionService.TranscribeAsync(
                        audio,
                        TranscriptionConfiguration.BuildOptions(settings, vocabularyPrompt),
                        cancellationToken);

                    var textOptions = new TextPostProcessingOptions(
                            AppendTrailingSpace: false,
                            RemoveFillerWords: settings.RemoveFillerWords,
                            FillerWords: FillerWordSettings.EffectiveList(settings.FillerWords),
                            WordReplacements: replacements,
                            PunctuationCleanupMode: settings.PunctuationCleanupMode,
                            LowercaseTranscription: settings.LowercaseTranscription,
                            ApplyTextFormatting: settings.IsTextFormattingEnabled);
                    var enhancementInputText = TextPostProcessor.ProcessForEnhancementInput(
                        transcription.Text,
                        textOptions);
                    var finalText = TextPostProcessor.Process(transcription.Text, textOptions);

                    if (finalText.Length == 0)
                    {
                        State = DictationState.Idle;
                        ResetPartialTranscript();
                        return;
                    }

                    var enhancement = enhancementPipeline is null
                        ? null
                        : await enhancementPipeline.EnhanceAsync(
                            enhancementInputText,
                            settings,
                            vocabulary,
                            cancellationToken);
                    if (enhancement?.WarningMessage is not null)
                    {
                        LastWarning = enhancement.WarningMessage;
                    }

                    State = DictationState.Inserting;
                    var insertedText = TextPostProcessor.ApplyTrailingSpace(
                        enhancement?.FinalText ?? finalText,
                        settings.AppendTrailingSpace);
                    await textInjection.InsertAsync(insertedText, cancellationToken);
                    LastStopInsertedText = true;
                    hasInsertedText = true;
                    await AutoSendPowerModeKeyAsync(powerModeResolution, cancellationToken);

                    try
                    {
                        var historyItem = new TranscriptionHistoryItem(
                            Guid.NewGuid(),
                            DateTimeOffset.UtcNow,
                            finalText,
                            TranscriptionConfiguration.ProviderName(settings),
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
                            powerModeEmoji: powerModeResolution.PowerModeEmoji);
                        await historyStore.SaveAsync(
                            historyItem,
                            cancellationToken);
                        hasCompletedHistory = true;
                        var metricsResult = await RecordMetricAsync(historyItem, cancellationToken);
                        LastWarning ??= metricsResult.WarningMessage;
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
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    if (!hasCompletedHistory && !hasInsertedText)
                    {
                        await SaveCanceledHistoryBestEffortAsync(audio, powerModeResolution, CancellationToken.None);
                    }

                    State = DictationState.Idle;
                    throw;
                }
                catch (Exception ex)
                {
                    if (!hasCompletedHistory)
                    {
                        await SaveFailedHistoryBestEffortAsync(
                            audio,
                            powerModeResolution,
                            ex.Message,
                            CancellationToken.None);
                    }

                    State = DictationState.Error;
                    LastError = ex.Message;
                    ResetPartialTranscript();
                    return;
                }

                State = DictationState.Idle;
                ResetPartialTranscript();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                State = DictationState.Idle;
                ResetPartialTranscript();
                throw;
            }
            catch (Exception ex)
            {
                State = DictationState.Error;
                LastError = ex.Message;
                ResetPartialTranscript();
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
            LastStopInsertedText = false;
            ResetPartialTranscript();
            await StopLiveTranscriptionPreviewAsync(CancellationToken.None);

            try
            {
                // Always release recorder resources before honoring caller cancellation.
                var audio = await audioCapture.StopAsync(CancellationToken.None);
                await NotifyCaptureStoppedAsync(CancellationToken.None);
                cancellationToken.ThrowIfCancellationRequested();

                var powerModeResolution = await ResolveCurrentPowerModeForCompletionAsync(cancellationToken);
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
                ResetPartialTranscript();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                State = DictationState.Idle;
                ResetPartialTranscript();
                throw;
            }
            catch (Exception ex)
            {
                State = DictationState.Error;
                LastError = ex.Message;
                ResetPartialTranscript();
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

    private async Task<PowerModeResolution> ResolveCurrentPowerModeForCompletionAsync(
        CancellationToken cancellationToken)
    {
        var settings = await settingsStore.LoadAsync(cancellationToken);
        return PowerModeMatcher.Resolve(settings, activePowerModeResolution?.Target);
    }

    private async Task AutoSendPowerModeKeyAsync(
        PowerModeResolution powerModeResolution,
        CancellationToken cancellationToken)
    {
        if (powerModeAutoSendService is null || powerModeResolution.AutoSendKey == PowerModeAutoSendKey.None)
        {
            return;
        }

        try
        {
            await powerModeAutoSendService.SendAsync(powerModeResolution.AutoSendKey, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LastWarning = $"Power Mode auto-send failed: {ex.Message}";
        }
    }

    private async Task SaveCanceledHistoryBestEffortAsync(
        AudioCaptureResult audio,
        PowerModeResolution powerModeResolution,
        CancellationToken cancellationToken)
    {
        try
        {
            var settings = powerModeResolution.EffectiveSettings;
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
            throw;
        }
        catch (Exception ex)
        {
            LastWarning = $"History save failed: {ex.Message}";
        }
    }

    private async Task SaveFailedHistoryBestEffortAsync(
        AudioCaptureResult audio,
        PowerModeResolution powerModeResolution,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var settings = powerModeResolution.EffectiveSettings;
            var safeError = string.IsNullOrWhiteSpace(errorMessage) ? "Transcription failed" : errorMessage;
            await historyStore.SaveAsync(
                new TranscriptionHistoryItem(
                    Guid.NewGuid(),
                    DateTimeOffset.UtcNow,
                    $"{TranscriptionHistoryItem.FailedTranscriptionPrefix} {safeError}",
                    TranscriptionConfiguration.ProviderName(settings),
                    audio.Duration,
                    TimeSpan.Zero,
                    originalText: $"{TranscriptionHistoryItem.FailedTranscriptionPrefix} {safeError}",
                    status: TranscriptionHistoryStatus.Failed,
                    language: settings.Language,
                    modelPath: TranscriptionConfiguration.ModelMetadata(settings),
                    errorMessage: safeError,
                    audioFilePath: audio.FilePath,
                    powerModeName: powerModeResolution.PowerModeName,
                    powerModeEmoji: powerModeResolution.PowerModeEmoji),
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LastWarning = $"History save failed: {ex.Message}";
        }
    }

    private Task<SessionMetricRecorderResult> RecordMetricAsync(
        TranscriptionHistoryItem item,
        CancellationToken cancellationToken) =>
        sessionMetricStore is null
            ? Task.FromResult(new SessionMetricRecorderResult(false))
            : SessionMetricRecorder.RecordAsync(
                item,
                sessionMetricStore,
                SessionMetricRecorder.DefaultSource,
                cancellationToken);

    private async Task StartLiveTranscriptionPreviewAsync(
        AppSettings settings,
        CancellationToken cancellationToken)
    {
        if (!settings.ShowLiveTranscriptPreview
            || liveTranscriptionPreviewService is null
            || audioChunkPublisher is null)
        {
            return;
        }

        try
        {
            liveTranscriptionPreviewSession = await liveTranscriptionPreviewService.TryStartAsync(
                settings,
                UpdatePartialTranscript,
                cancellationToken);
            if (liveTranscriptionPreviewSession is not null)
            {
                audioChunkPublisher.AudioChunkAvailable += AudioChunkPublisher_AudioChunkAvailable;
                isLivePreviewSubscribed = true;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LastWarning = $"Live transcript preview unavailable: {ex.Message}";
            await StopLiveTranscriptionPreviewAsync(CancellationToken.None);
        }
    }

    private async Task StopLiveTranscriptionPreviewAsync(CancellationToken cancellationToken)
    {
        if (isLivePreviewSubscribed && audioChunkPublisher is not null)
        {
            audioChunkPublisher.AudioChunkAvailable -= AudioChunkPublisher_AudioChunkAvailable;
            isLivePreviewSubscribed = false;
        }

        var session = liveTranscriptionPreviewSession;
        liveTranscriptionPreviewSession = null;
        if (session is null)
        {
            return;
        }

        try
        {
            await session.CompleteAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LastWarning ??= $"Live transcript preview cleanup failed: {ex.Message}";
        }

        try
        {
            await session.DisposeAsync();
        }
        catch (Exception ex)
        {
            LastWarning ??= $"Live transcript preview cleanup failed: {ex.Message}";
        }
    }

    private void AudioChunkPublisher_AudioChunkAvailable(object? sender, AudioChunk chunk)
    {
        try
        {
            liveTranscriptionPreviewSession?.EnqueueAudio(chunk);
        }
        catch
        {
            // Streaming preview is best-effort and must not disrupt recording.
        }
    }

    private async Task NotifyCaptureStoppedAsync(CancellationToken cancellationToken)
    {
        if (recordingCaptureStopFeedback is null)
        {
            return;
        }

        try
        {
            await recordingCaptureStopFeedback.CaptureStoppedAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
        }
    }

    private void ResetPartialTranscript()
    {
        isAcceptingPartialTranscript = false;
        PartialTranscript = string.Empty;
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
