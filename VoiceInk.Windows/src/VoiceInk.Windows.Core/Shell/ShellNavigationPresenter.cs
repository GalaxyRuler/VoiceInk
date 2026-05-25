namespace VoiceInk.Windows.Core.Shell;

public static class ShellNavigationPresenter
{
    public static IReadOnlyList<ShellNavigationItem> BuildItems() =>
    [
        new("Dashboard", "Dashboard", "Home"),
        new("Transcribe Audio", "Transcribe Audio", "Audio"),
        new("History", "History", "Document"),
        new("Metrics", "Metrics", "Calculator"),
        new("AI Models", "AI Models", "Library"),
        new("Enhancement", "Enhancement", "Edit"),
        new("Power Mode", "Power Mode", "LightningBolt"),
        new("Audio Input", "Audio Input", "Microphone"),
        new("Dictionary", "Dictionary", "Character"),
        new("Settings", "Settings", "Setting"),
        new("About", "About / Open Source", "Help")
    ];
}
