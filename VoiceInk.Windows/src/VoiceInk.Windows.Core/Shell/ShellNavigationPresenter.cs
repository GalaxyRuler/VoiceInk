namespace VoiceInk.Windows.Core.Shell;

public static class ShellNavigationPresenter
{
    public static IReadOnlyList<ShellNavigationItem> BuildItems() =>
    [
        new("Dashboard", "Dashboard", "Home"),
        new("History", "History", "Document"),
        new("AI Models", "AI Models", "Library"),
        new("Audio Input", "Audio Input", "Microphone"),
        new("Dictionary", "Dictionary", "Character"),
        new("Settings", "Settings", "Setting"),
        new("About", "About / Open Source", "Help")
    ];
}
