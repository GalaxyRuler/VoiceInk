namespace VoiceInk.Windows.Core.Metrics;

public sealed record SessionMetricsTimeFilter(string Id, string Label)
{
    public static SessionMetricsTimeFilter Last7Days { get; } = new("last7Days", "Last 7 Days");
    public static SessionMetricsTimeFilter Last30Days { get; } = new("last30Days", "Last 30 Days");
    public static SessionMetricsTimeFilter ThisYear { get; } = new("thisYear", "This Year");
    public static SessionMetricsTimeFilter AllTime { get; } = new("allTime", "All Time");

    public static IReadOnlyList<SessionMetricsTimeFilter> AllChoices { get; } =
    [
        Last7Days,
        Last30Days,
        ThisYear,
        AllTime
    ];

    public DateTimeOffset? Since(DateTimeOffset now) =>
        Since(now, timeZone: null);

    public DateTimeOffset? Since(DateTimeOffset now, TimeZoneInfo? timeZone) =>
        Id switch
        {
            "last7Days" => now.AddDays(-7),
            "last30Days" => now.AddDays(-30),
            "thisYear" => StartOfYear(now, timeZone),
            "allTime" => null,
            _ => Last7Days.Since(now, timeZone)
        };

    public static SessionMetricsTimeFilter Resolve(string? id) =>
        AllChoices.FirstOrDefault(choice => string.Equals(choice.Id, id, StringComparison.OrdinalIgnoreCase))
        ?? Last7Days;

    private static DateTimeOffset StartOfYear(DateTimeOffset now, TimeZoneInfo? timeZone)
    {
        if (timeZone is null)
        {
            return new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);
        }

        var localNow = TimeZoneInfo.ConvertTime(now, timeZone);
        var localStart = new DateTime(localNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        return new DateTimeOffset(localStart, timeZone.GetUtcOffset(localStart));
    }
}
