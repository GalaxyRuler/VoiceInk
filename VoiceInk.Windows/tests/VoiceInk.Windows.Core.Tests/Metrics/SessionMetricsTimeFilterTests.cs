using VoiceInk.Windows.Core.Metrics;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Metrics;

public sealed class SessionMetricsTimeFilterTests
{
    [Fact]
    public void AllChoices_ReturnsMacStyleFilterOrderAndLabels()
    {
        var choices = SessionMetricsTimeFilter.AllChoices;

        Assert.Collection(
            choices,
            choice => Assert.Equal(("last7Days", "Last 7 Days"), (choice.Id, choice.Label)),
            choice => Assert.Equal(("last30Days", "Last 30 Days"), (choice.Id, choice.Label)),
            choice => Assert.Equal(("thisYear", "This Year"), (choice.Id, choice.Label)),
            choice => Assert.Equal(("allTime", "All Time"), (choice.Id, choice.Label)));
    }

    [Fact]
    public void Since_ReturnsStartForRelativeFilters()
    {
        var now = new DateTimeOffset(2026, 5, 25, 12, 30, 0, TimeSpan.FromHours(3));

        Assert.Equal(now.AddDays(-7), SessionMetricsTimeFilter.Last7Days.Since(now));
        Assert.Equal(now.AddDays(-30), SessionMetricsTimeFilter.Last30Days.Since(now));
        Assert.Equal(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(3)), SessionMetricsTimeFilter.ThisYear.Since(now));
        Assert.Null(SessionMetricsTimeFilter.AllTime.Since(now));
    }

    [Fact]
    public void Since_UsesTimeZoneRulesForThisYear()
    {
        var eastern = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        var now = new DateTimeOffset(2026, 7, 4, 12, 0, 0, TimeSpan.FromHours(-4));

        var since = SessionMetricsTimeFilter.ThisYear.Since(now, eastern);

        Assert.Equal(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(-5)), since);
    }

    [Fact]
    public void Resolve_ReturnsDefaultForUnknownIds()
    {
        Assert.Equal(SessionMetricsTimeFilter.Last7Days, SessionMetricsTimeFilter.Resolve(null));
        Assert.Equal(SessionMetricsTimeFilter.Last7Days, SessionMetricsTimeFilter.Resolve("missing"));
        Assert.Equal(SessionMetricsTimeFilter.AllTime, SessionMetricsTimeFilter.Resolve("allTime"));
    }
}
