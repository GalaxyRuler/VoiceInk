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
    ISettingsStore settingsStore)
{
    private readonly SemaphoreSlim lifecycleGate = new(1, 1);

    public DictationState State { get; private set; } = DictationState.Idle;
    public string? LastError { get; private set; }
    public string? LastWarning { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await lifecycleGate.WaitAsync(cancellationToken);
        try
        {
            if (State is DictationState.Recording or DictationState.Transcribing or DictationState.Inserting)
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
                    LastError = "Select a local whisper model before dictating.";
                    return;
                }

                State = DictationState.Recording;
                await audioCapture.StartAsync(cancellationToken);
            }
            catch (OperationCanceledException)
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
        await lifecycleGate.WaitAsync(cancellationToken);
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
                var audio = await audioCapture.StopAsync(cancellationToken);
                var settings = await settingsStore.LoadAsync(cancellationToken);

                State = DictationState.Transcribing;
                var transcription = await transcriptionService.TranscribeAsync(
                    audio,
                    new TranscriptionOptions(settings.ModelPath, settings.Language),
                    cancellationToken);

                var finalText = TextPostProcessor.Process(
                    transcription.Text,
                    new TextPostProcessingOptions(settings.AppendTrailingSpace));

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
                            transcription.Duration),
                        cancellationToken);
                }
                catch (OperationCanceledException)
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
            catch (OperationCanceledException)
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
}
