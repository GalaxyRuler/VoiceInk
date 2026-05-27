namespace VoiceInk.Windows.Core.Notifications;

public static class AppNotificationPresenter
{
    private static readonly HashSet<string> PassiveStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Idle",
        "Recording",
        "Transcribing",
        "Inserting",
        "Loading settings"
    };

    private static readonly string[] ErrorMarkers =
    [
        "failed",
        "failure",
        "unavailable",
        "no longer available",
        "not found",
        "invalid",
        "must ",
        "missing",
        "rejected",
        "denied",
        "could not",
        "error"
    ];

    private static readonly string[] WarningMarkers =
    [
        "canceled",
        "cancelled",
        "select ",
        "choose ",
        "needs ",
        "disabled",
        "no ",
        "closing"
    ];

    private static readonly string[] SuccessMarkers =
    [
        "saved",
        "updated",
        "refreshed",
        "opened",
        "copied",
        "exported",
        "imported",
        "selected",
        "enabled",
        "removed",
        "deleted",
        "queued",
        "reset",
        "added",
        "captured",
        "sorted"
    ];

    public static AppNotificationPresentation? FromStatus(string? status)
    {
        var message = status?.Trim();
        if (string.IsNullOrWhiteSpace(message) || PassiveStatuses.Contains(message))
        {
            return null;
        }

        var kind = KindFor(message);
        return new AppNotificationPresentation(message, kind, DurationFor(kind));
    }

    private static AppNotificationKind KindFor(string message)
    {
        if (ContainsAny(message, ErrorMarkers))
        {
            return AppNotificationKind.Error;
        }

        if (ContainsAny(message, WarningMarkers))
        {
            return AppNotificationKind.Warning;
        }

        return ContainsAny(message, SuccessMarkers)
            ? AppNotificationKind.Success
            : AppNotificationKind.Info;
    }

    private static TimeSpan DurationFor(AppNotificationKind kind) =>
        kind switch
        {
            AppNotificationKind.Error => TimeSpan.FromSeconds(5),
            AppNotificationKind.Warning => TimeSpan.FromSeconds(4),
            _ => TimeSpan.FromSeconds(3)
        };

    private static bool ContainsAny(string message, IEnumerable<string> markers) =>
        markers.Any(marker => message.Contains(marker, StringComparison.OrdinalIgnoreCase));
}
