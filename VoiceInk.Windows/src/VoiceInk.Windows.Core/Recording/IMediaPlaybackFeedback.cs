namespace VoiceInk.Windows.Core.Recording;

public interface IMediaPlaybackFeedback
{
    Task PauseAsync(CancellationToken cancellationToken);
    Task ResumeAsync(TimeSpan delay, CancellationToken cancellationToken);
}
