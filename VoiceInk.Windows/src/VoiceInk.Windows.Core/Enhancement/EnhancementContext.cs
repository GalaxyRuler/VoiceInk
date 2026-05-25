namespace VoiceInk.Windows.Core.Enhancement;

public sealed record EnhancementContext(
    string ClipboardText = "",
    string SelectedText = "")
{
    public static EnhancementContext Empty { get; } = new();
}
