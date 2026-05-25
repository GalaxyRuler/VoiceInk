using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed class CompositeLiveTranscriptionPreviewService(
    IReadOnlyList<ILiveTranscriptionPreviewService> services) : ILiveTranscriptionPreviewService
{
    public async Task<ILiveTranscriptionPreviewSession?> TryStartAsync(
        AppSettings settings,
        Action<string> partialTranscriptUpdated,
        CancellationToken cancellationToken)
    {
        foreach (var service in services)
        {
            var session = await service.TryStartAsync(
                    settings,
                    partialTranscriptUpdated,
                    cancellationToken)
                .ConfigureAwait(false);
            if (session is not null)
            {
                return session;
            }
        }

        return null;
    }
}
