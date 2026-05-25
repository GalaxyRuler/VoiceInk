namespace VoiceInk.Windows.Core.Metrics;

public sealed record SessionMetricRecorderResult(
    bool Recorded,
    string? WarningMessage = null);
