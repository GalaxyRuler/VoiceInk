using System.Globalization;
using VoiceInk.Windows.Core.Metrics;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Metrics;

public sealed class SessionMetricsDashboardPresenterTests
{
    [Fact]
    public void Present_BuildsMacStyleHeroAndMetricCards()
    {
        WithCulture("en-US", () =>
        {
            var summary = new SessionMetricsSummary(
                TotalSessions: 2,
                TotalWords: 1_234,
                TotalAudioDuration: TimeSpan.FromMinutes(10),
                WordsPerMinute: 123.4,
                KeystrokesSaved: 6_170,
                TimeSaved: TimeSpan.FromMinutes(25));

            var presentation = SessionMetricsDashboardPresenter.Present("Last 7 days", summary);

            Assert.False(presentation.IsEmpty);
            Assert.Equal("Last 7 days", presentation.FilterLabel);
            Assert.Equal("You have saved 25m 0s with VoiceInk", presentation.HeroTitle);
            Assert.Equal("Dictated 1,234 words across 2 sessions.", presentation.HeroSubtitle);
            Assert.Equal("Audio Duration: 10m 0s", presentation.AudioDurationDisplay);
            Assert.Collection(
                presentation.DataGuidanceRows,
                row =>
                {
                    Assert.Equal("Sessions and Words", row.Title);
                    Assert.Equal("Completed work", row.Value);
                    Assert.Equal("Counts come from completed recorder, file transcription, and retry rows saved locally.", row.Detail);
                    Assert.Equal("SQLite", row.StatusBadge);
                },
                row =>
                {
                    Assert.Equal("Words Per Minute", row.Title);
                    Assert.Equal("Words / audio", row.Value);
                    Assert.Equal("WPM is calculated from dictated words and recorded audio duration in the selected filter.", row.Detail);
                    Assert.Equal("Derived", row.StatusBadge);
                },
                row =>
                {
                    Assert.Equal("Saved Effort", row.Title);
                    Assert.Equal("Estimate", row.Value);
                    Assert.Equal("Keystrokes and time saved are local productivity estimates, not telemetry.", row.Detail);
                    Assert.Equal("Local", row.StatusBadge);
                },
                row =>
                {
                    Assert.Equal("Export and Reset", row.Title);
                    Assert.Equal("Metrics only", row.Value);
                    Assert.Equal("CSV export writes a local file; reset clears metrics without deleting History or recordings.", row.Detail);
                    Assert.Equal("User action", row.StatusBadge);
                },
                row =>
                {
                    Assert.Equal("Model Performance", row.Title);
                    Assert.Equal("Local averages", row.Value);
                    Assert.Equal("Model rows summarize completed local records in the selected filter, not remote telemetry.", row.Detail);
                    Assert.Equal("Interpretation", row.StatusBadge);
                });
            Assert.Collection(
                presentation.ActionRows,
                row =>
                {
                    Assert.Equal("Filter", row.Title);
                    Assert.Equal("Last 7 days", row.Value);
                    Assert.Equal("Dashboard totals and model performance use this time window.", row.Detail);
                    Assert.Equal("Active", row.StatusBadge);
                },
                row =>
                {
                    Assert.Equal("Export CSV", row.Title);
                    Assert.Equal("Ready", row.Value);
                    Assert.Equal("Exports dashboard totals and model performance summaries to a local file.", row.Detail);
                    Assert.Equal("Local file", row.StatusBadge);
                },
                row =>
                {
                    Assert.Equal("Model Performance", row.Title);
                    Assert.Equal("Available", row.Value);
                    Assert.Equal("Transcription and enhancement model rows update from local metrics.", row.Detail);
                    Assert.Equal("Local", row.StatusBadge);
                },
                row =>
                {
                    Assert.Equal("Reset Metrics", row.Title);
                    Assert.Equal("Local metrics only", row.Value);
                    Assert.Equal("Reset keeps History, recordings, settings, and diagnostics intact.", row.Detail);
                    Assert.Equal("Confirmed", row.StatusBadge);
                });
            Assert.Collection(
                presentation.DiagnosticsRows,
                row =>
                {
                    Assert.Equal("Data Source", row.Title);
                    Assert.Equal("2 completed sessions in Last 7 days.", row.Detail);
                    Assert.Equal("SQLite", row.StatusBadge);
                },
                row =>
                {
                    Assert.Equal("Privacy", row.Title);
                    Assert.Equal("Metrics stay local to this Windows profile unless you export CSV.", row.Detail);
                    Assert.Equal("Local", row.StatusBadge);
                },
                row =>
                {
                    Assert.Equal("Audio Duration", row.Title);
                    Assert.Equal("Recorded audio in this filter totals 10m 0s.", row.Detail);
                    Assert.Equal("Local", row.StatusBadge);
                },
                row =>
                {
                    Assert.Equal("Export", row.Title);
                    Assert.Equal("CSV export includes dashboard totals and model performance summaries.", row.Detail);
                    Assert.Equal("CSV", row.StatusBadge);
                },
                row =>
                {
                    Assert.Equal("Windows Diagnostics", row.Title);
                    Assert.Equal("VoiceInk metrics export is separate from Windows Diagnostic Data Viewer exports.", row.Detail);
                    Assert.Equal("Separate", row.StatusBadge);
                },
                row =>
                {
                    Assert.Equal("Reset", row.Title);
                    Assert.Equal("Reset deletes local metrics only; History and recordings stay unchanged.", row.Detail);
                    Assert.Equal("Metrics only", row.StatusBadge);
                });
            Assert.Collection(
                presentation.Cards,
                card =>
                {
                    Assert.Equal("Sessions Recorded", card.Title);
                    Assert.Equal("2", card.Value);
                    Assert.Equal("VoiceInk sessions completed", card.Detail);
                },
                card =>
                {
                    Assert.Equal("Words Dictated", card.Title);
                    Assert.Equal("1,234", card.Value);
                    Assert.Equal("words generated", card.Detail);
                },
                card =>
                {
                    Assert.Equal("Words Per Minute", card.Title);
                    Assert.Equal("123.4", card.Value);
                    Assert.Equal("VoiceInk vs. typing by hand", card.Detail);
                },
                card =>
                {
                    Assert.Equal("Keystrokes Saved", card.Title);
                    Assert.Equal("6,170", card.Value);
                    Assert.Equal("fewer keystrokes", card.Detail);
                });
        });
    }

    [Fact]
    public void Present_BuildsMacStyleEmptyState()
    {
        WithCulture("en-US", () =>
        {
            var presentation = SessionMetricsDashboardPresenter.Present("All time", SessionMetricsSummary.Empty);

            Assert.True(presentation.IsEmpty);
            Assert.Equal("No Recorder Sessions Yet", presentation.HeroTitle);
            Assert.Equal("Start your first recording to unlock value insights.", presentation.HeroSubtitle);
            Assert.Empty(presentation.Cards);
            Assert.Equal("Audio Duration: 0s", presentation.AudioDurationDisplay);
            Assert.Equal("0 completed sessions in All time.", presentation.DiagnosticsRows[0].Detail);
            Assert.Equal("Empty", presentation.ActionRows[1].Value);
            Assert.Equal("Waiting", presentation.ActionRows[2].Value);
        });
    }

    private static void WithCulture(string cultureName, Action action)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
        try
        {
            action();
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
