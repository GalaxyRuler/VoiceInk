namespace VoiceInk.Windows.Core.Metrics;

public static class SessionMetricsAggregator
{
    private const double AverageTypingWordsPerMinute = 35;
    private const int AverageKeystrokesPerWord = 5;

    public static SessionMetricsSummary Summarize(IEnumerable<SessionMetric> metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        var totalSessions = 0;
        var totalWords = 0;
        var totalAudioDuration = TimeSpan.Zero;

        foreach (var metric in metrics)
        {
            totalSessions++;
            totalWords += Math.Max(0, metric.WordCount);
            totalAudioDuration += ClampDuration(metric.AudioDuration);
        }

        var wordsPerMinute = totalAudioDuration > TimeSpan.Zero
            ? totalWords / totalAudioDuration.TotalMinutes
            : 0;
        var estimatedTypingTime = TimeSpan.FromMinutes(totalWords / AverageTypingWordsPerMinute);
        var timeSaved = estimatedTypingTime > totalAudioDuration
            ? estimatedTypingTime - totalAudioDuration
            : TimeSpan.Zero;

        return new SessionMetricsSummary(
            totalSessions,
            totalWords,
            totalAudioDuration,
            wordsPerMinute,
            totalWords * AverageKeystrokesPerWord,
            timeSaved);
    }

    public static IReadOnlyList<ModelPerformanceStat> TranscriptionModelPerformance(
        IEnumerable<SessionMetric> metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        return metrics
            .Where(metric =>
                !string.IsNullOrWhiteSpace(metric.TranscriptionModelName)
                && metric.TranscriptionDuration is not null
                && metric.TranscriptionDuration.Value > TimeSpan.Zero)
            .GroupBy(metric => metric.TranscriptionModelName!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var sessionCount = group.Count();
                var totalProcessing = TimeSpan.FromTicks(group.Sum(metric => metric.TranscriptionDuration!.Value.Ticks));
                var totalAudio = TimeSpan.FromTicks(group.Sum(metric => ClampDuration(metric.AudioDuration).Ticks));
                return new ModelPerformanceStat(
                    group.First().TranscriptionModelName!.Trim(),
                    sessionCount,
                    totalProcessing,
                    TimeSpan.FromTicks(totalProcessing.Ticks / Math.Max(1, sessionCount)),
                    TimeSpan.FromTicks(totalAudio.Ticks / Math.Max(1, sessionCount)),
                    totalProcessing > TimeSpan.Zero ? totalAudio.TotalSeconds / totalProcessing.TotalSeconds : 0);
            })
            .OrderBy(stat => stat.AverageProcessingDuration)
            .ThenBy(stat => stat.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static IReadOnlyList<ModelPerformanceStat> EnhancementModelPerformance(
        IEnumerable<SessionMetric> metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        return metrics
            .Where(metric =>
                !string.IsNullOrWhiteSpace(metric.AiEnhancementModelName)
                && metric.EnhancementDuration is not null
                && metric.EnhancementDuration.Value > TimeSpan.Zero)
            .GroupBy(metric => metric.AiEnhancementModelName!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var sessionCount = group.Count();
                var totalProcessing = TimeSpan.FromTicks(group.Sum(metric => metric.EnhancementDuration!.Value.Ticks));
                return new ModelPerformanceStat(
                    group.First().AiEnhancementModelName!.Trim(),
                    sessionCount,
                    totalProcessing,
                    TimeSpan.FromTicks(totalProcessing.Ticks / Math.Max(1, sessionCount)),
                    TimeSpan.Zero,
                    0);
            })
            .OrderBy(stat => stat.AverageProcessingDuration)
            .ThenBy(stat => stat.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static TimeSpan ClampDuration(TimeSpan duration) =>
        duration > TimeSpan.Zero ? duration : TimeSpan.Zero;
}
