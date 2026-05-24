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
    public DictationState State { get; private set; } = DictationState.Idle;
    public string? LastError { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        LastError = null;
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

    public async Task StopAsync(CancellationToken cancellationToken)
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

        await historyStore.SaveAsync(
            new TranscriptionHistoryItem(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                finalText,
                transcription.ProviderName,
                audio.Duration,
                transcription.Duration),
            cancellationToken);

        State = DictationState.Idle;
    }
}
