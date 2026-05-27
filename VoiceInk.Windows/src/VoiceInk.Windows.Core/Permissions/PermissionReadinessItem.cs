namespace VoiceInk.Windows.Core.Permissions;

public sealed record PermissionReadinessItem(
    string Title,
    string Description,
    string Status,
    bool IsReady,
    string ActionText,
    string ActionTarget,
    string FallbackGuidance = "");
