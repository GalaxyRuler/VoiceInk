using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.PowerMode;

public static class PowerModeMatcher
{
    public static PowerModeResolution Resolve(AppSettings settings, PowerModeTarget? target)
    {
        if (target is null)
        {
            return new PowerModeResolution(null, settings, null);
        }

        var rule = settings.PowerModeRules.FirstOrDefault(rule => IsSpecificMatch(rule, target))
            ?? settings.PowerModeRules.FirstOrDefault(rule => rule.IsEnabled && rule.IsDefault);

        return new PowerModeResolution(rule, Apply(rule, settings), target);
    }

    private static bool IsSpecificMatch(PowerModeRule rule, PowerModeTarget target)
    {
        if (!rule.IsEnabled || rule.IsDefault)
        {
            return false;
        }

        var processPattern = rule.ProcessNamePattern.Trim();
        var titlePattern = rule.WindowTitlePattern.Trim();
        if (processPattern.Length == 0 && titlePattern.Length == 0)
        {
            return false;
        }

        return MatchesIfConfigured(target.ProcessName, processPattern, rule.MatchKind)
            && MatchesIfConfigured(target.WindowTitle, titlePattern, rule.MatchKind);
    }

    private static bool MatchesIfConfigured(string value, string pattern, PowerModeMatchKind matchKind)
    {
        if (pattern.Length == 0)
        {
            return true;
        }

        return matchKind switch
        {
            PowerModeMatchKind.Equals => string.Equals(value.Trim(), pattern, StringComparison.OrdinalIgnoreCase),
            _ => value.Contains(pattern, StringComparison.OrdinalIgnoreCase)
        };
    }

    private static AppSettings Apply(PowerModeRule? rule, AppSettings settings)
    {
        if (rule is null)
        {
            return settings;
        }

        return settings with
        {
            ModelPath = OverrideString(rule.ModelPathOverride, settings.ModelPath),
            Language = OverrideString(rule.LanguageOverride, settings.Language),
            IsEnhancementEnabled = rule.IsEnhancementEnabledOverride ?? settings.IsEnhancementEnabled,
            SelectedEnhancementPromptId = rule.SelectedEnhancementPromptIdOverride ?? settings.SelectedEnhancementPromptId,
            AppendTrailingSpace = rule.AppendTrailingSpaceOverride ?? settings.AppendTrailingSpace,
            RemoveFillerWords = rule.RemoveFillerWordsOverride ?? settings.RemoveFillerWords,
            PunctuationCleanupMode = rule.PunctuationCleanupModeOverride ?? settings.PunctuationCleanupMode,
            LowercaseTranscription = rule.LowercaseTranscriptionOverride ?? settings.LowercaseTranscription
        };
    }

    private static string OverrideString(string? overrideValue, string baseValue) =>
        string.IsNullOrWhiteSpace(overrideValue) ? baseValue : overrideValue.Trim();
}
