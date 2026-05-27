using System.Globalization;

namespace VoiceInk.Windows.Core.Metrics;

public sealed record SessionMetricsDashboardPresentation(
    string FilterLabel,
    string HeroTitle,
    string HeroSubtitle,
    bool IsEmpty,
    string AudioDurationDisplay,
    IReadOnlyList<SessionMetricsDataGuidanceRow> DataGuidanceRows,
    IReadOnlyList<SessionMetricsActionRow> ActionRows,
    IReadOnlyList<SessionMetricsDiagnosticsRow> DiagnosticsRows,
    IReadOnlyList<SessionMetricsDashboardCard> Cards);

public sealed record SessionMetricsDataGuidanceRow(
    string Title,
    string Value,
    string Detail,
    string StatusBadge);

public sealed record SessionMetricsActionRow(
    string Title,
    string Value,
    string Detail,
    string StatusBadge);

public sealed record SessionMetricsDiagnosticsRow(
    string Title,
    string Detail,
    string StatusBadge);

public sealed record SessionMetricsDashboardCard(
    string IconGlyph,
    string Title,
    string Value,
    string Detail,
    string Accent);

public static class SessionMetricsDashboardPresenter
{
    public static SessionMetricsDashboardPresentation Present(
        string filterLabel,
        SessionMetricsSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        var culture = CultureInfo.CurrentCulture;
        var normalizedFilterLabel = string.IsNullOrWhiteSpace(filterLabel)
            ? "All time"
            : filterLabel.Trim();
        var isEmpty = summary.TotalSessions <= 0;
        var heroTitle = isEmpty
            ? "No Recorder Sessions Yet"
            : $"You have saved {FormatDuration(summary.TimeSaved, culture)} with VoiceInk";
        var heroSubtitle = isEmpty
            ? "Start your first recording to unlock value insights."
            : $"Dictated {summary.TotalWords.ToString("N0", culture)} words across {summary.TotalSessions.ToString("N0", culture)} {Pluralize(summary.TotalSessions, "session", "sessions")}.";

        return new SessionMetricsDashboardPresentation(
            normalizedFilterLabel,
            heroTitle,
            heroSubtitle,
            isEmpty,
            $"Audio Duration: {FormatDuration(summary.TotalAudioDuration, culture)}",
            DataGuidanceRows(),
            ActionRows(normalizedFilterLabel, summary),
            DiagnosticsRows(normalizedFilterLabel, summary, culture),
            isEmpty
                ? []
                :
            [
                new(
                    "\uE720",
                    "Sessions Recorded",
                    Math.Max(0, summary.TotalSessions).ToString("N0", culture),
                    "VoiceInk sessions completed",
                    "Purple"),
                new(
                    "\uE8D2",
                    "Words Dictated",
                    Math.Max(0, summary.TotalWords).ToString("N0", culture),
                    "words generated",
                    "Accent"),
                new(
                    "\uE9D9",
                    "Words Per Minute",
                    summary.WordsPerMinute > 0 ? summary.WordsPerMinute.ToString("0.0", culture) : "0",
                    "VoiceInk vs. typing by hand",
                    "Yellow"),
                new(
                    "\uE765",
                    "Keystrokes Saved",
                    Math.Max(0, summary.KeystrokesSaved).ToString("N0", culture),
                    "fewer keystrokes",
                    "Orange")
            ]);
    }

    private static IReadOnlyList<SessionMetricsDataGuidanceRow> DataGuidanceRows() =>
    [
        new(
            "Sessions and Words",
            "Completed work",
            "Counts come from completed recorder, file transcription, and retry rows saved locally.",
            "SQLite"),
        new(
            "Words Per Minute",
            "Words / audio",
            "WPM is calculated from dictated words and recorded audio duration in the selected filter.",
            "Derived"),
        new(
            "Saved Effort",
            "Estimate",
            "Keystrokes and time saved are local productivity estimates, not telemetry.",
            "Local"),
        new(
            "Typing Baseline",
            "35 WPM / 5 keys",
            "Time saved compares dictated words against a 35 WPM typing estimate, then subtracts recorded audio duration; keystrokes saved use 5 keys per word.",
            "Estimate"),
        new(
            "Export and Reset",
            "Metrics only",
            "CSV export writes a local file; reset clears metrics without deleting History or recordings.",
            "User action"),
        new(
            "Model Performance",
            "Local averages",
            "Model rows summarize completed local records in the selected filter, not remote telemetry.",
            "Interpretation")
    ];

    private static IReadOnlyList<SessionMetricsActionRow> ActionRows(
        string filterLabel,
        SessionMetricsSummary summary) =>
    [
        new(
            "Filter",
            filterLabel,
            "Dashboard totals and model performance use this time window.",
            "Active"),
        new(
            "Export CSV",
            summary.TotalSessions > 0 ? "Ready" : "Empty",
            "Exports dashboard totals and model performance summaries to a local file.",
            "Local file"),
        new(
            "Model Performance",
            summary.TotalSessions > 0 ? "Available" : "Waiting",
            "Transcription and enhancement model rows update from local metrics.",
            "Local"),
        new(
            "Reset Metrics",
            "Local metrics only",
            "Reset keeps History, recordings, settings, and diagnostics intact.",
            "Confirmed")
    ];

    private static IReadOnlyList<SessionMetricsDiagnosticsRow> DiagnosticsRows(
        string filterLabel,
        SessionMetricsSummary summary,
        CultureInfo culture) =>
    [
        new(
            "Data Source",
            $"{Math.Max(0, summary.TotalSessions).ToString("N0", culture)} completed {Pluralize(summary.TotalSessions, "session", "sessions")} in {filterLabel}.",
            "SQLite"),
        new(
            "Privacy",
            "Metrics stay local to this Windows profile unless you export CSV.",
            "Local"),
        new(
            "Audio Duration",
            $"Recorded audio in this filter totals {FormatDuration(summary.TotalAudioDuration, culture)}.",
            "Local"),
        new(
            "Export",
            "CSV export includes dashboard totals and model performance summaries.",
            "CSV"),
        new(
            "Windows Diagnostics",
            "VoiceInk metrics export is separate from Windows Diagnostic Data Viewer exports.",
            "Separate"),
        new(
            "Reset",
            "Reset deletes local metrics only; History and recordings stay unchanged.",
            "Metrics only")
    ];

    public static string FormatDuration(TimeSpan duration) =>
        FormatDuration(duration, CultureInfo.CurrentCulture);

    private static string FormatDuration(TimeSpan duration, CultureInfo culture)
    {
        if (duration <= TimeSpan.Zero)
        {
            return "0s";
        }

        if (duration.TotalHours >= 1)
        {
            return $"{(int)duration.TotalHours}h {duration.Minutes}m";
        }

        if (duration.TotalMinutes >= 1)
        {
            return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";
        }

        return $"{duration.TotalSeconds.ToString("0.#", culture)}s";
    }

    private static string Pluralize(int count, string singular, string plural) =>
        count == 1 ? singular : plural;
}
