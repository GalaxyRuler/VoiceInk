using System.Globalization;
using System.Text;

namespace VoiceInk.Windows.Core.Metrics;

public static class MetricsCsvExporter
{
    public static string Export(
        string filterLabel,
        SessionMetricsSummary summary,
        IReadOnlyList<ModelPerformanceStat> transcriptionModelStats,
        IReadOnlyList<ModelPerformanceStat> enhancementModelStats)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Section,Filter,Metric,Value");
        AppendSummary(builder, filterLabel, "Sessions Recorded", summary.TotalSessions.ToString(CultureInfo.InvariantCulture));
        AppendSummary(builder, filterLabel, "Words Dictated", summary.TotalWords.ToString(CultureInfo.InvariantCulture));
        AppendSummary(builder, filterLabel, "Audio Duration", FormatDuration(summary.TotalAudioDuration));
        AppendSummary(builder, filterLabel, "Words Per Minute", summary.WordsPerMinute.ToString("0.0", CultureInfo.InvariantCulture));
        AppendSummary(builder, filterLabel, "Keystrokes Saved", summary.KeystrokesSaved.ToString(CultureInfo.InvariantCulture));
        AppendSummary(builder, filterLabel, "Time Saved", FormatDuration(summary.TimeSaved));

        builder.AppendLine();
        builder.AppendLine("Section,Filter,Model,Sessions,Average Processing,Average Audio,Speed Factor");
        foreach (var stat in transcriptionModelStats)
        {
            AppendModelStat(builder, "Transcription Models", filterLabel, stat, includeAudioAndSpeed: true);
        }

        builder.AppendLine();
        builder.AppendLine("Section,Filter,Model,Sessions,Average Processing,Average Audio,Speed Factor");
        foreach (var stat in enhancementModelStats)
        {
            AppendModelStat(builder, "Enhancement Models", filterLabel, stat, includeAudioAndSpeed: false);
        }

        return builder.ToString();
    }

    private static void AppendSummary(
        StringBuilder builder,
        string filterLabel,
        string metric,
        string value)
    {
        AppendRow(builder, "Summary", filterLabel, metric, value);
    }

    private static void AppendModelStat(
        StringBuilder builder,
        string section,
        string filterLabel,
        ModelPerformanceStat stat,
        bool includeAudioAndSpeed)
    {
        AppendRow(
            builder,
            section,
            filterLabel,
            stat.Name,
            stat.SessionCount.ToString(CultureInfo.InvariantCulture),
            FormatDuration(stat.AverageProcessingDuration),
            includeAudioAndSpeed ? FormatDuration(stat.AverageAudioDuration) : string.Empty,
            includeAudioAndSpeed ? stat.SpeedFactor.ToString("0.0", CultureInfo.InvariantCulture) : string.Empty);
    }

    private static void AppendRow(StringBuilder builder, params string[] values)
    {
        builder.AppendLine(string.Join(",", values.Select(Escape)));
    }

    private static string Escape(string value)
    {
        if (value.Contains('"', StringComparison.Ordinal)
            || value.Contains(',', StringComparison.Ordinal)
            || value.Contains('\n', StringComparison.Ordinal)
            || value.Contains('\r', StringComparison.Ordinal))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }

    private static string FormatDuration(TimeSpan duration) =>
        duration <= TimeSpan.Zero
            ? "00:00:00"
            : $"{Math.Floor(duration.TotalHours).ToString("00", CultureInfo.InvariantCulture)}:"
            + $"{duration.Minutes.ToString("00", CultureInfo.InvariantCulture)}:"
            + $"{duration.Seconds.ToString("00", CultureInfo.InvariantCulture)}";
}
