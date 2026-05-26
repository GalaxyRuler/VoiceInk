namespace VoiceInk.Windows.Core.Shortcuts;

public enum MiniRecorderShortcutKind
{
    Prompt,
    PowerMode
}

public sealed record MiniRecorderShortcut(
    MiniRecorderShortcutKind Kind,
    int SlotIndex);
