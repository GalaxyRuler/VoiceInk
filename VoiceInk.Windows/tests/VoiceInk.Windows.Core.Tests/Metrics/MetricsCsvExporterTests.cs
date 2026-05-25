using VoiceInk.Windows.Core.Metrics;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Metrics;

public sealed class MetricsCsvExporterTests
{
    [Fact]
    public void Export_IncludesSummaryAndPerformanceSections()
    {
        var csv = MetricsCsvExporter.Export(
            "Last 7 Days",
            new SessionMetricsSummary(
                TotalSessions: 2,
                TotalWords: 105,
                TotalAudioDuration: TimeSpan.FromMinutes(3),
                WordsPerMinute: 35,
                KeystrokesSaved: 525,
                TimeSaved: TimeSpan.FromMinutes(1)),
            [
                new ModelPerformanceStat(
                    "base",
                    2,
                    TimeSpan.FromSeconds(20),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(45),
                    4.5)
            ],
            [
                new ModelPerformanceStat(
                    "gpt-4o-mini",
                    1,
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(3),
                    TimeSpan.Zero,
                    0)
            ]);

        Assert.Contains("Section,Filter,Metric,Value", csv);
        Assert.Contains("Summary,Last 7 Days,Sessions Recorded,2", csv);
        Assert.Contains("Summary,Last 7 Days,Words Dictated,105", csv);
        Assert.Contains("Summary,Last 7 Days,Words Per Minute,35.0", csv);
        Assert.Contains("Transcription Models,Last 7 Days,base,2,00:00:10,00:00:45,4.5", csv);
        Assert.Contains("Enhancement Models,Last 7 Days,gpt-4o-mini,1,00:00:03,,", csv);
    }

    [Fact]
    public void Export_EscapesCommasQuotesAndNewlines()
    {
        var csv = MetricsCsvExporter.Export(
            "All Time",
            SessionMetricsSummary.Empty,
            [
                new ModelPerformanceStat(
                    "base, \"fast\"\nmodel",
                    1,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(8),
                    4)
            ],
            []);

        Assert.Contains("\"base, \"\"fast\"\"", csv);
        Assert.Contains("model", csv);
    }

    [Fact]
    public void Export_PreservesDurationsLongerThanTwentyFourHours()
    {
        var csv = MetricsCsvExporter.Export(
            "All Time",
            new SessionMetricsSummary(
                TotalSessions: 1,
                TotalWords: 1,
                TotalAudioDuration: TimeSpan.FromHours(25),
                WordsPerMinute: 0,
                KeystrokesSaved: 5,
                TimeSaved: TimeSpan.FromHours(26)),
            [
                new ModelPerformanceStat(
                    "base",
                    1,
                    TimeSpan.FromHours(25),
                    TimeSpan.FromHours(25),
                    TimeSpan.FromHours(25),
                    1)
            ],
            []);

        Assert.Contains("Summary,All Time,Audio Duration,25:00:00", csv);
        Assert.Contains("Summary,All Time,Time Saved,26:00:00", csv);
        Assert.Contains("Transcription Models,All Time,base,1,25:00:00,25:00:00,1.0", csv);
    }
}
