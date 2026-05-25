using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Services;

public interface ILiveTranscriptionPreviewService
{
    Task<ILiveTranscriptionPreviewSession?> TryStartAsync(
        AppSettings settings,
        Action<string> partialTranscriptUpdated,
        CancellationToken cancellationToken);
}
