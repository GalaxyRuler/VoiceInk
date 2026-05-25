using VoiceInk.Windows.Core.Metrics;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Metrics;

public sealed class SessionMetricsAggregatorTests
{
    [Fact]
    public void Summarize_ComputesDashboardTotalsFromSessionMetrics()
    {
        var metrics = new[]
        {
            new SessionMetric(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateTimeOffset(2026, 5, 25, 10, 0, 0, TimeSpan.Zero),
                "recorder",
                70,
                TimeSpan.FromMinutes(1),
                "ggml-base.en.bin",
                TimeSpan.FromSeconds(15),
                4,
                "Docs",
                "gpt-4o-mini",
                TimeSpan.FromSeconds(2)),
            new SessionMetric(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateTimeOffset(2026, 5, 25, 10, 5, 0, TimeSpan.Zero),
                "audio-file",
                35,
                TimeSpan.FromMinutes(2),
                "whisper-large-v3-turbo",
                TimeSpan.FromSeconds(40),
                3,
                null,
                "llama3.2",
                TimeSpan.FromSeconds(6))
        };

        var summary = SessionMetricsAggregator.Summarize(metrics);

        Assert.Equal(2, summary.TotalSessions);
        Assert.Equal(105, summary.TotalWords);
        Assert.Equal(TimeSpan.FromMinutes(3), summary.TotalAudioDuration);
        Assert.Equal(35, summary.WordsPerMinute);
        Assert.Equal(525, summary.KeystrokesSaved);
        Assert.Equal(TimeSpan.Zero, summary.TimeSaved);
    }

    [Fact]
    public void Summarize_ClampsNegativeCountsAndDurations()
    {
        var metrics = new[]
        {
            new SessionMetric(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                "recorder",
                -20,
                TimeSpan.FromSeconds(-12),
                "base",
                TimeSpan.FromSeconds(-1),
                null,
                null,
                "enhancer",
                TimeSpan.FromSeconds(-5))
        };

        var summary = SessionMetricsAggregator.Summarize(metrics);

        Assert.Equal(1, summary.TotalSessions);
        Assert.Equal(0, summary.TotalWords);
        Assert.Equal(TimeSpan.Zero, summary.TotalAudioDuration);
        Assert.Equal(0, summary.WordsPerMinute);
        Assert.Equal(0, summary.KeystrokesSaved);
        Assert.Equal(TimeSpan.Zero, summary.TimeSaved);
        Assert.Empty(SessionMetricsAggregator.TranscriptionModelPerformance(metrics));
        Assert.Empty(SessionMetricsAggregator.EnhancementModelPerformance(metrics));
    }

    [Fact]
    public void TranscriptionModelPerformance_AveragesAndSortsByProcessingTime()
    {
        var metrics = new[]
        {
            Metric("base", TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(15)),
            Metric("base", TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)),
            Metric("large", TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(30)),
            Metric(null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1)),
            Metric("ignored", TimeSpan.FromSeconds(10), TimeSpan.Zero)
        };

        var stats = SessionMetricsAggregator.TranscriptionModelPerformance(metrics);

        Assert.Collection(
            stats,
            stat =>
            {
                Assert.Equal("base", stat.Name);
                Assert.Equal(2, stat.SessionCount);
                Assert.Equal(TimeSpan.FromSeconds(10), stat.AverageProcessingDuration);
                Assert.Equal(TimeSpan.FromSeconds(45), stat.AverageAudioDuration);
                Assert.Equal(4.5, stat.SpeedFactor);
            },
            stat =>
            {
                Assert.Equal("large", stat.Name);
                Assert.Equal(1, stat.SessionCount);
                Assert.Equal(TimeSpan.FromSeconds(30), stat.AverageProcessingDuration);
                Assert.Equal(TimeSpan.FromSeconds(60), stat.AverageAudioDuration);
                Assert.Equal(2, stat.SpeedFactor);
            });
    }

    [Fact]
    public void EnhancementModelPerformance_AveragesAndSortsByDuration()
    {
        var metrics = new[]
        {
            Metric(enhancementModelName: "gpt-4o-mini", enhancementDuration: TimeSpan.FromSeconds(4)),
            Metric(enhancementModelName: "gpt-4o-mini", enhancementDuration: TimeSpan.FromSeconds(2)),
            Metric(enhancementModelName: "local-ollama", enhancementDuration: TimeSpan.FromSeconds(8)),
            Metric(enhancementModelName: null, enhancementDuration: TimeSpan.FromSeconds(1)),
            Metric(enhancementModelName: "ignored", enhancementDuration: TimeSpan.Zero)
        };

        var stats = SessionMetricsAggregator.EnhancementModelPerformance(metrics);

        Assert.Collection(
            stats,
            stat =>
            {
                Assert.Equal("gpt-4o-mini", stat.Name);
                Assert.Equal(2, stat.SessionCount);
                Assert.Equal(TimeSpan.FromSeconds(3), stat.AverageProcessingDuration);
                Assert.Equal(TimeSpan.Zero, stat.AverageAudioDuration);
                Assert.Equal(0, stat.SpeedFactor);
            },
            stat =>
            {
                Assert.Equal("local-ollama", stat.Name);
                Assert.Equal(1, stat.SessionCount);
                Assert.Equal(TimeSpan.FromSeconds(8), stat.AverageProcessingDuration);
            });
    }

    private static SessionMetric Metric(
        string? transcriptionModelName = "base",
        TimeSpan? audioDuration = null,
        TimeSpan? transcriptionDuration = null,
        string? enhancementModelName = null,
        TimeSpan? enhancementDuration = null) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "recorder",
            10,
            audioDuration ?? TimeSpan.FromSeconds(1),
            transcriptionModelName,
            transcriptionDuration,
            transcriptionDuration is null || transcriptionDuration <= TimeSpan.Zero
                ? null
                : (audioDuration ?? TimeSpan.FromSeconds(1)).TotalSeconds / transcriptionDuration.Value.TotalSeconds,
            null,
            enhancementModelName,
            enhancementDuration);
}
