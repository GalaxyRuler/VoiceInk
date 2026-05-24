namespace VoiceInk.Windows.Core.Shell;

public sealed record ShellNavigationItem(
    string Tag,
    string Label,
    string Icon,
    bool IsEnabled = true);
