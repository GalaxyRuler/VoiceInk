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
                    Assert.Equal("Export", row.Title);
                    Assert.Equal("CSV export includes dashboard totals and model performance summaries.", row.Detail);
                    Assert.Equal("CSV", row.StatusBadge);
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
