namespace VoiceInk.Windows.Core.Recording;

public interface IRecordingSoundFileProbe
{
    RecordingSoundFileProbeResult Probe(string filePath);
}

public sealed record RecordingSoundFileProbeResult
{
    private RecordingSoundFileProbeResult(bool isSuccess, TimeSpan duration, string errorMessage)
    {
        IsSuccess = isSuccess;
        Duration = duration;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }
    public TimeSpan Duration { get; }
    public string ErrorMessage { get; }

    public static RecordingSoundFileProbeResult Success(TimeSpan duration) =>
        new(true, duration, string.Empty);

    public static RecordingSoundFileProbeResult Failure(string errorMessage) =>
        new(false, TimeSpan.Zero, errorMessage);
}
