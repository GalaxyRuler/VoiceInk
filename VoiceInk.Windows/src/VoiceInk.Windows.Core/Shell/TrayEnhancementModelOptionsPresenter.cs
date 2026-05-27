namespace VoiceInk.Windows.Core.Shell;

public static class TrayEnhancementModelOptionsPresenter
{
    public static IReadOnlyList<TrayMenuOption> FromModels(
        IReadOnlyList<string> modelChoices,
        string? selectedModel)
    {
        var selected = selectedModel?.Trim() ?? string.Empty;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var options = new List<TrayMenuOption>();

        foreach (var model in modelChoices)
        {
            var trimmed = model.Trim();
            if (trimmed.Length == 0 || !seen.Add(trimmed))
            {
                continue;
            }

            options.Add(new TrayMenuOption(
                trimmed,
                trimmed,
                string.Equals(trimmed, selected, StringComparison.OrdinalIgnoreCase)));
        }

        if (selected.Length > 0
            && !options.Any(option => string.Equals(option.Id, selected, StringComparison.OrdinalIgnoreCase)))
        {
            options.Insert(0, new TrayMenuOption(
                selected,
                $"Custom: {selected}",
                IsChecked: true));
        }

        return options;
    }
}
