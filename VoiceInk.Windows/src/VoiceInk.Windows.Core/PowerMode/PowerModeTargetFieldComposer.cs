namespace VoiceInk.Windows.Core.PowerMode;

public sealed record PowerModeTargetFields(
    string ProcessNamePattern,
    string WindowTitlePattern,
    string BrowserUrlPattern);

public static class PowerModeTargetFieldComposer
{
    public static PowerModeTargetFields AppendTarget(PowerModeTargetFields current, PowerModeTarget target) =>
        new(
            AppendAlternative(current.ProcessNamePattern, target.ProcessName),
            AppendAlternative(current.WindowTitlePattern, target.WindowTitle),
            AppendAlternative(current.BrowserUrlPattern, target.BrowserUrl));

    private static string AppendAlternative(string current, string next)
    {
        var alternatives = SplitAlternatives(current).ToList();
        var trimmedNext = next.Trim();
        if (trimmedNext.Length > 0
            && !alternatives.Any(item => string.Equals(item, trimmedNext, StringComparison.OrdinalIgnoreCase)))
        {
            alternatives.Add(trimmedNext);
        }

        return string.Join("; ", alternatives);
    }

    private static string[] SplitAlternatives(string value) =>
        value.Split([';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
