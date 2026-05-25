namespace VoiceInk.Windows.Core.Enhancement;

public sealed record EnhancementContext(string ClipboardText)
{
    public static EnhancementContext Empty { get; } = new(string.Empty);
}
