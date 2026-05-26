namespace VoiceInk.Windows.Core.PowerMode;

public sealed record PowerModePagePresentation(
    string Title,
    string Description,
    string CountLabel,
    bool IsEmpty,
    string EmptyTitle,
    string EmptyDescription);

public static class PowerModePagePresenter
{
    public static PowerModePagePresentation Present(IReadOnlyList<PowerModeRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        var total = rules.Count;
        var enabled = rules.Count(rule => rule.IsEnabled);
        var disabled = total - enabled;
        var isEmpty = total == 0;

        return new PowerModePagePresentation(
            "Power Modes",
            "Automate your workflows with context-aware configurations.",
            isEmpty ? "0 Power Modes" : $"{total} {Pluralize(total, "Power Mode", "Power Modes")} ({enabled} enabled, {disabled} disabled)",
            isEmpty,
            isEmpty ? "No Power Modes Yet" : string.Empty,
            isEmpty ? "Create your first power mode to automate your VoiceInk workflow based on apps and websites." : string.Empty);
    }

    private static string Pluralize(int count, string singular, string plural) =>
        count == 1 ? singular : plural;
}
