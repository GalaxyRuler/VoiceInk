namespace VoiceInk.Windows.Core.Notifications;

public sealed record AppNotificationPresentation(
    string Message,
    AppNotificationKind Kind,
    TimeSpan Duration);
