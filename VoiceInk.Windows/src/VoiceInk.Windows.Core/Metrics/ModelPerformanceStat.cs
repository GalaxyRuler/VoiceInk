namespace VoiceInk.Windows.Core.Metrics;

public sealed record ModelPerformanceStat(
    string Name,
    int SessionCount,
    TimeSpan TotalProcessingDuration,
    TimeSpan AverageProcessingDuration,
    TimeSpan AverageAudioDuration,
    double SpeedFactor);
