namespace VoiceInk.Windows.Core.Metrics;

public sealed record SessionMetric(
    Guid Id,
    Guid TranscriptionId,
    DateTimeOffset Timestamp,
    string Source,
    int WordCount,
    TimeSpan AudioDuration,
    string? TranscriptionModelName,
    TimeSpan? TranscriptionDuration,
    double? SpeedFactor,
    string? PowerModeName,
    string? AiEnhancementModelName,
    TimeSpan? EnhancementDuration);
