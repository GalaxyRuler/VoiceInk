namespace VoiceInk.Windows.Core.Metrics;

public sealed record SessionMetricsSummary(
    int TotalSessions,
    int TotalWords,
    TimeSpan TotalAudioDuration,
    double WordsPerMinute,
    int KeystrokesSaved,
    TimeSpan TimeSaved)
{
    public static SessionMetricsSummary Empty { get; } =
        new(0, 0, TimeSpan.Zero, 0, 0, TimeSpan.Zero);
}
