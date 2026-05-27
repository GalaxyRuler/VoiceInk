namespace VoiceInk.Windows.Core.PowerMode;

public static class PowerModeInstalledApplicationPresenter
{
    public static IReadOnlyList<PowerModeInstalledApplicationChoice> Present(
        IEnumerable<PowerModeInstalledApplicationChoice> choices) =>
        choices
            .Select(Normalize)
            .Where(choice => !string.IsNullOrWhiteSpace(choice.DisplayName)
                && !string.IsNullOrWhiteSpace(choice.ProcessName))
            .GroupBy(choice => choice.ProcessName, StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderBy(choice => choice.DisplayName.Length)
                .ThenBy(choice => choice.DisplayName, StringComparer.OrdinalIgnoreCase)
                .First())
            .OrderBy(choice => choice.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public static PowerModeTargetFields AppendChoice(
        PowerModeTargetFields current,
        PowerModeInstalledApplicationChoice choice) =>
        PowerModeTargetFieldComposer.AppendTarget(
            current,
            new PowerModeTarget(NormalizeProcessName(choice.ProcessName), string.Empty));

    private static PowerModeInstalledApplicationChoice Normalize(PowerModeInstalledApplicationChoice choice)
    {
        var displayName = string.IsNullOrWhiteSpace(choice.DisplayName)
            ? NormalizeProcessName(choice.ProcessName)
            : choice.DisplayName.Trim();
        return choice with
        {
            DisplayName = displayName,
            ProcessName = NormalizeProcessName(choice.ProcessName),
            Source = choice.Source.Trim()
        };
    }

    private static string NormalizeProcessName(string processName)
    {
        var trimmed = processName.Trim();
        if (trimmed.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed[..^4];
        }

        return trimmed;
    }
}
