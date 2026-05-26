namespace VoiceInk.Windows.Core.Shell;

public sealed record TrayMenuOption(
    string Id,
    string Label,
    bool IsChecked = false,
    bool IsEnabled = true);
