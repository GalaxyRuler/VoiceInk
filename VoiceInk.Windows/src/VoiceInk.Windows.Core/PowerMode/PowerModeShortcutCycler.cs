using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.PowerMode;

public static class PowerModeShortcutCycler
{
    public static Guid? NextRuleId(AppSettings settings, IReadOnlyList<PowerModeRule> rules)
    {
        var enabledRules = rules
            .Where(rule => rule.IsEnabled)
            .ToArray();
        if (enabledRules.Length == 0)
        {
            return null;
        }

        if (settings.SelectedPowerModeRuleId is not { } selectedRuleId)
        {
            return enabledRules[0].Id;
        }

        var selectedIndex = Array.FindIndex(enabledRules, rule => rule.Id == selectedRuleId);
        if (selectedIndex < 0)
        {
            return enabledRules[0].Id;
        }

        return selectedIndex + 1 < enabledRules.Length
            ? enabledRules[selectedIndex + 1].Id
            : null;
    }
}
