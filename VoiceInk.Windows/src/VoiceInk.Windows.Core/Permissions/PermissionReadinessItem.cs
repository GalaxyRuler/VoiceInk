namespace VoiceInk.Windows.Core.Permissions;

public sealed record PermissionReadinessItem(
    string Title,
    string Description,
    string Status,
    bool IsReady,
    string ActionText,
    string ActionTarget,
    string FallbackGuidance = "")
{
    public string AccessibleName =>
        string.Join(
            ", ",
            new[] { Title, Status, Description, FallbackGuidance }
                .Where(part => !string.IsNullOrWhiteSpace(part)));
}
