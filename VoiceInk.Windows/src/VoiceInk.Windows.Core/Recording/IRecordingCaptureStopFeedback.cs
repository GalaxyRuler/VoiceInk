namespace VoiceInk.Windows.Core.Recording;

public interface IRecordingCaptureStopFeedback
{
    Task CaptureStoppedAsync(CancellationToken cancellationToken);
}
