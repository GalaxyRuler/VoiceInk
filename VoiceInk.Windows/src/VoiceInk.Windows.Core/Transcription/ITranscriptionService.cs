using VoiceInk.Windows.Core.Audio;

namespace VoiceInk.Windows.Core.Transcription;

public interface ITranscriptionService
{
    Task<TranscriptionResult> TranscribeAsync(
        AudioCaptureResult audio,
        TranscriptionOptions options,
        CancellationToken cancellationToken);
}
