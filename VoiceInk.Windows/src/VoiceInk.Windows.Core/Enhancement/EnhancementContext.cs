namespace VoiceInk.Windows.Core.Enhancement;

public sealed record EnhancementContext(
    string ClipboardText = "",
    string SelectedText = "",
    string ActiveWindowProcessName = "",
    string ActiveWindowTitle = "",
    string BrowserUrl = "")
{
    public static EnhancementContext Empty { get; } = new();
}
