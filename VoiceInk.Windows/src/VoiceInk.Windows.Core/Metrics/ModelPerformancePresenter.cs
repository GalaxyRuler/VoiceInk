using System.Globalization;

namespace VoiceInk.Windows.Core.Metrics;

public sealed record ModelPerformanceRow(
    string Title,
    string Subtitle,
    string Detail,
    string PrimaryValue,
    string StatusBadge,
    bool IsEmpty = false)
{
    public string DisplayText =>
        string.IsNullOrWhiteSpace(Subtitle)
            ? $"{Title} - {Detail}"
            : $"{Title} - {Subtitle}; {Detail}";

    public string AccessibleName =>
        string.Join(
            ", ",
            new[] { Title, PrimaryValue, StatusBadge, Subtitle, Detail }
                .Where(part => !string.IsNullOrWhiteSpace(part)));
}

public static class ModelPerformancePresenter
{
    public static IReadOnlyList<ModelPerformanceRow> PresentTranscription(
        IEnumerable<ModelPerformanceStat> stats)
    {
        var rows = stats
            .Select(stat => new ModelPerformanceRow(
                stat.Name,
                $"{stat.SessionCount.ToString("N0", CultureInfo.CurrentCulture)} {Pluralize(stat.SessionCount, "session", "sessions")} - {stat.SpeedFactor:0.0}x realtime",
                $"{SessionMetricsDashboardPresenter.FormatDuration(stat.AverageProcessingDuration)} avg processing; {SessionMetricsDashboardPresenter.FormatDuration(stat.AverageAudioDuration)} avg audio",
                $"{stat.SpeedFactor:0.0}x",
                stat.SpeedFactor >= 1 ? "Faster than Real-time" : "Slower than Real-time"))
            .ToArray();

        return rows.Length == 0
            ? [new ModelPerformanceRow(
                "No transcription model metrics yet",
                string.Empty,
                "Complete a local or cloud transcription to compare model speed.",
                "No data",
                "Waiting",
                IsEmpty: true)]
            : rows;
    }

    public static IReadOnlyList<ModelPerformanceRow> PresentEnhancement(
        IEnumerable<ModelPerformanceStat> stats)
    {
        var rows = stats
            .Select(stat => new ModelPerformanceRow(
                stat.Name,
                $"{stat.SessionCount.ToString("N0", CultureInfo.CurrentCulture)} {Pluralize(stat.SessionCount, "session", "sessions")}",
                $"{SessionMetricsDashboardPresenter.FormatDuration(stat.AverageProcessingDuration)} avg enhancement processing",
                SessionMetricsDashboardPresenter.FormatDuration(stat.AverageProcessingDuration),
                "Enhancement"))
            .ToArray();

        return rows.Length == 0
            ? [new ModelPerformanceRow(
                "No enhancement model metrics yet",
                string.Empty,
                "Enable enhancement and complete a session to compare enhancement latency.",
                "No data",
                "Waiting",
                IsEmpty: true)]
            : rows;
    }

    private static string Pluralize(int count, string singular, string plural) =>
        count == 1 ? singular : plural;
}
