namespace VoiceInk.Windows.Core.PowerMode;

public static class PowerModeRuleValidator
{
    public static PowerModeRuleValidationResult Validate(PowerModeRule rule)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        var hasMatchFields = HasMatchFields(rule);
        if (rule.IsEnabled && !rule.IsDefault && !hasMatchFields)
        {
            errors.Add("Enabled Power Mode rules need a process, window title, or browser URL match.");
        }

        if (rule.IsDefault && hasMatchFields)
        {
            warnings.Add("Default Power Mode rules ignore process, window title, and browser URL matches.");
        }

        return new PowerModeRuleValidationResult(errors, warnings);
    }

    public static PowerModeRuleValidationResult Validate(IReadOnlyList<PowerModeRule> rules)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        foreach (var rule in rules)
        {
            var ruleResult = Validate(rule);
            errors.AddRange(ruleResult.Errors);
            warnings.AddRange(ruleResult.Warnings);
        }

        if (rules.Count(rule => rule.IsEnabled && rule.IsDefault) > 1)
        {
            errors.Add("Only one enabled Power Mode rule can be the default fallback.");
        }

        var seenSignatures = new Dictionary<string, PowerModeRule>(StringComparer.OrdinalIgnoreCase);
        foreach (var rule in rules.Where(rule => rule.IsEnabled && !rule.IsDefault && HasMatchFields(rule)))
        {
            var signature = MatchSignature(rule);
            if (seenSignatures.TryGetValue(signature, out var previousRule))
            {
                warnings.Add($"{DisplayName(rule)} duplicates the match fields from {DisplayName(previousRule)}; only the first matching rule will apply.");
                continue;
            }

            seenSignatures.Add(signature, rule);
        }

        return new PowerModeRuleValidationResult(errors, warnings);
    }

    private static bool HasMatchFields(PowerModeRule rule) =>
        !string.IsNullOrWhiteSpace(rule.ProcessNamePattern)
        || !string.IsNullOrWhiteSpace(rule.WindowTitlePattern)
        || !string.IsNullOrWhiteSpace(rule.BrowserUrlPattern);

    private static string MatchSignature(PowerModeRule rule) =>
        string.Join(
            "|",
            rule.MatchKind.ToString(),
            Normalize(rule.ProcessNamePattern),
            Normalize(rule.WindowTitlePattern),
            Normalize(rule.BrowserUrlPattern));

    private static string Normalize(string value) =>
        value.Trim().ToUpperInvariant();

    private static string DisplayName(PowerModeRule rule) =>
        string.IsNullOrWhiteSpace(rule.Name) ? "Unnamed rule" : rule.Name.Trim();
}
