namespace VoiceInk.Windows.Core.PowerMode;

public sealed record PowerModePagePresentation(
    string Title,
    string Description,
    string ManualSwitchingSummary,
    string MatchGuidance,
    string OverrideGuidance,
    string SessionGuidance,
    string CountLabel,
    bool IsEmpty,
    string EmptyTitle,
    string EmptyDescription,
    IReadOnlyList<PowerModeRuleRowPresentation> RuleRows);

public sealed record PowerModeRuleRowPresentation(
    Guid Id,
    string Title,
    string TargetSummary,
    string OverrideSummary,
    string ShortcutSummary,
    string StatusBadge);

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
            "Switch modes from the recorder, tray, global shortcuts, or direct rule shortcuts. Enabled rules keep their list order for number-slot selection.",
            "Specific process, title, and sanitized URL rules are checked in list order; the default fallback applies only when no specific rule matches.",
            "Rule overrides temporarily layer over Settings while the selected Power Mode is active; blank override fields keep the current Settings value.",
            "Automatic matches and manual selections are resolved for each recording, so rule overrides do not rewrite your base Settings.",
            isEmpty ? "0 Power Modes" : $"{total} {Pluralize(total, "Power Mode", "Power Modes")} ({enabled} enabled, {disabled} disabled)",
            isEmpty,
            isEmpty ? "No Power Modes Yet" : string.Empty,
            isEmpty ? "Create your first power mode to automate your VoiceInk workflow based on apps and websites." : string.Empty,
            rules.Select(PresentRuleRow).ToArray());
    }

    private static PowerModeRuleRowPresentation PresentRuleRow(PowerModeRule rule) =>
        new(
            rule.Id,
            Display(rule.Name, rule.Emoji),
            TargetSummary(rule),
            OverrideSummary(rule),
            string.IsNullOrWhiteSpace(rule.Shortcut) ? "No direct shortcut" : $"Shortcut: {rule.Shortcut.Trim()}",
            rule.IsEnabled ? "Enabled" : "Disabled");

    private static string TargetSummary(PowerModeRule rule)
    {
        if (rule.IsDefault)
        {
            return "Default fallback";
        }

        var parts = new[]
        {
            string.IsNullOrWhiteSpace(rule.ProcessNamePattern) ? null : $"Process: {rule.ProcessNamePattern.Trim()}",
            string.IsNullOrWhiteSpace(rule.WindowTitlePattern) ? null : $"Title: {rule.WindowTitlePattern.Trim()}",
            string.IsNullOrWhiteSpace(rule.BrowserUrlPattern) ? null : $"URL: {rule.BrowserUrlPattern.Trim()}"
        }.Where(part => part is not null);
        var summary = string.Join("; ", parts);
        return string.IsNullOrWhiteSpace(summary) ? "No target" : summary;
    }

    private static string OverrideSummary(PowerModeRule rule)
    {
        var overrides = new List<string>();
        if (!string.IsNullOrWhiteSpace(rule.ModelPathOverride))
        {
            overrides.Add("model");
        }

        if (!string.IsNullOrWhiteSpace(rule.LanguageOverride))
        {
            overrides.Add("language");
        }

        if (rule.IsEnhancementEnabledOverride is not null)
        {
            overrides.Add("enhancement");
        }

        if (rule.SelectedEnhancementPromptIdOverride is not null)
        {
            overrides.Add("prompt");
        }

        if (rule.AppendTrailingSpaceOverride is not null)
        {
            overrides.Add("trailing space");
        }

        if (rule.RemoveFillerWordsOverride is not null)
        {
            overrides.Add("fillers");
        }

        if (rule.LowercaseTranscriptionOverride is not null)
        {
            overrides.Add("lowercase");
        }

        if (rule.PunctuationCleanupModeOverride is not null)
        {
            overrides.Add("punctuation");
        }

        if (rule.AutoSendKey != PowerModeAutoSendKey.None)
        {
            overrides.Add("auto-send");
        }

        return overrides.Count == 0
            ? "No overrides"
            : $"{overrides.Count} {Pluralize(overrides.Count, "override", "overrides")}: {string.Join(", ", overrides)}";
    }

    private static string Display(string name, string emoji)
    {
        var trimmedName = name.Trim();
        var trimmedEmoji = emoji.Trim();
        if (trimmedEmoji == "*")
        {
            trimmedEmoji = string.Empty;
        }

        return (trimmedEmoji, trimmedName) switch
        {
            ({ Length: > 0 }, { Length: > 0 }) => $"{trimmedEmoji} {trimmedName}",
            ({ Length: > 0 }, _) => trimmedEmoji,
            (_, { Length: > 0 }) => trimmedName,
            _ => "Unnamed Power Mode"
        };
    }

    private static string Pluralize(int count, string singular, string plural) =>
        count == 1 ? singular : plural;
}
