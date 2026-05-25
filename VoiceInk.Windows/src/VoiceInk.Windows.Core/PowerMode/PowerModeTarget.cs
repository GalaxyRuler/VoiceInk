namespace VoiceInk.Windows.Core.PowerMode;

public sealed record PowerModeTarget(
    string ProcessName,
    string WindowTitle,
    int? ProcessId = null,
    string BrowserUrl = "");
