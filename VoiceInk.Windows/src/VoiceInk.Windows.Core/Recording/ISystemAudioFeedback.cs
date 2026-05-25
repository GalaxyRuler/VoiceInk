namespace VoiceInk.Windows.Core.Recording;

public interface ISystemAudioFeedback
{
    Task MuteAsync(CancellationToken cancellationToken);
    Task RestoreAsync(TimeSpan delay, CancellationToken cancellationToken);
}
