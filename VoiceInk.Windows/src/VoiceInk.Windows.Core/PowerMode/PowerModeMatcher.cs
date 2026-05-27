using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Enhancement;

namespace VoiceInk.Windows.Core.PowerMode;

public static class PowerModeMatcher
{
    public static PowerModeResolution Resolve(AppSettings settings, PowerModeTarget? target)
    {
        if (!settings.IsPowerModeEnabled)
        {
            return new PowerModeResolution(null, settings, target);
        }

        var explicitRule = settings.SelectedPowerModeRuleId is { } selectedRuleId
            ? settings.PowerModeRules.FirstOrDefault(rule => rule.IsEnabled && rule.Id == selectedRuleId)
            : null;
        if (explicitRule is not null)
        {
            return new PowerModeResolution(explicitRule, Apply(explicitRule, settings), target);
        }

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

        var processPatterns = SplitPatterns(rule.ProcessNamePattern);
        var titlePatterns = SplitPatterns(rule.WindowTitlePattern);
        var browserUrlPatterns = SplitPatterns(rule.BrowserUrlPattern);
        if (processPatterns.Length == 0 && titlePatterns.Length == 0 && browserUrlPatterns.Length == 0)
        {
            return false;
        }

        return MatchesAnyIfConfigured(target.ProcessName, processPatterns, rule.MatchKind)
            && MatchesAnyIfConfigured(target.WindowTitle, titlePatterns, rule.MatchKind)
            && BrowserUrlMatchesAnyIfConfigured(target.BrowserUrl, browserUrlPatterns, rule.MatchKind);
    }

    private static bool BrowserUrlMatchesAnyIfConfigured(string value, IReadOnlyList<string> patterns, PowerModeMatchKind matchKind)
    {
        if (patterns.Count == 0)
        {
            return true;
        }

        var sanitizedValue = BrowserUrlContextSanitizer.Sanitize(value);
        if (sanitizedValue.Length == 0)
        {
            return false;
        }

        return patterns.Any(pattern =>
        {
            var sanitizedPattern = BrowserUrlContextSanitizer.Sanitize(pattern);
            var effectivePattern = sanitizedPattern.Length == 0 ? pattern : sanitizedPattern;
            return Matches(sanitizedValue, effectivePattern, matchKind);
        });
    }

    private static bool MatchesAnyIfConfigured(string value, IReadOnlyList<string> patterns, PowerModeMatchKind matchKind)
    {
        if (patterns.Count == 0)
        {
            return true;
        }

        return patterns.Any(pattern => Matches(value, pattern, matchKind));
    }

    private static bool Matches(string value, string pattern, PowerModeMatchKind matchKind) =>
        matchKind switch
        {
            PowerModeMatchKind.Equals => string.Equals(value.Trim(), pattern, StringComparison.OrdinalIgnoreCase),
            _ => value.Contains(pattern, StringComparison.OrdinalIgnoreCase)
        };

    private static string[] SplitPatterns(string value) =>
        value.Split([';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

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
            UseOcrContext = rule.UseOcrContextOverride ?? settings.UseOcrContext,
            AppendTrailingSpace = rule.AppendTrailingSpaceOverride ?? settings.AppendTrailingSpace,
            RemoveFillerWords = rule.RemoveFillerWordsOverride ?? settings.RemoveFillerWords,
            IsTextFormattingEnabled = rule.IsTextFormattingEnabledOverride ?? settings.IsTextFormattingEnabled,
            PunctuationCleanupMode = rule.PunctuationCleanupModeOverride ?? settings.PunctuationCleanupMode,
            LowercaseTranscription = rule.LowercaseTranscriptionOverride ?? settings.LowercaseTranscription
        };
    }

    private static string OverrideString(string? overrideValue, string baseValue) =>
        string.IsNullOrWhiteSpace(overrideValue) ? baseValue : overrideValue.Trim();
}
