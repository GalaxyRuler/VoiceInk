using VoiceInk.Windows.Core.Audio;

namespace VoiceInk.Windows.Core.Services;

public interface IVoiceActivityDetector
{
    Task<VoiceActivityResult> AnalyzeAsync(
        AudioCaptureResult audio,
        CancellationToken cancellationToken);
}
