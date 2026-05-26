namespace VoiceInk.Windows.Core.Permissions;

public sealed record PermissionsReadinessStatus(
    string SummarySeverity,
    string SummaryTitle,
    string SummaryMessage,
    IReadOnlyList<PermissionReadinessItem> Items);
