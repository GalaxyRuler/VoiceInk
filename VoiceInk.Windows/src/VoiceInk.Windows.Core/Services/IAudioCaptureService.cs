using VoiceInk.Windows.Core.Audio;

namespace VoiceInk.Windows.Core.Services;

public interface IAudioCaptureService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task<AudioCaptureResult> StopAsync(CancellationToken cancellationToken);
}
