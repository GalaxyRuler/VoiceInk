namespace VoiceInk.Windows.Core.Models;

public enum WhisperModelWarmupStatus
{
    Idle,
    Skipped,
    Warming,
    Succeeded,
    Failed,
    Canceled
}

public sealed record WhisperModelWarmupState(
    WhisperModelWarmupStatus Status,
    string ModelPath,
    string DisplayName,
    string Trigger,
    string Message,
    DateTimeOffset? StartedAt = null,
    DateTimeOffset? CompletedAt = null,
    TimeSpan? Duration = null)
{
    public static WhisperModelWarmupState Idle { get; } = new(
        WhisperModelWarmupStatus.Idle,
        string.Empty,
        string.Empty,
        string.Empty,
        "Model warmup idle");
}

public sealed record WhisperModelWarmupStartResult(
    bool Started,
    WhisperModelWarmupStatus Status,
    string Message,
    Task? WarmupTask = null);
