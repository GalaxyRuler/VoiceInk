using System.Globalization;

namespace VoiceInk.Windows.Core.Metrics;

public sealed record SessionMetricsDashboardPresentation(
    string FilterLabel,
    string HeroTitle,
    string HeroSubtitle,
    bool IsEmpty,
    string AudioDurationDisplay,
    IReadOnlyList<SessionMetricsDashboardCard> Cards);

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
