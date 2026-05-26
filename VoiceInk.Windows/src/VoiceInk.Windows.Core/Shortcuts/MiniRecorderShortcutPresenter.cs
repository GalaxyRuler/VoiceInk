namespace VoiceInk.Windows.Core.Shortcuts;

public static class MiniRecorderShortcutPresenter
{
    public static bool TryCreate(
        bool control,
        bool alt,
        bool shift,
        int virtualKey,
        out MiniRecorderShortcut? shortcut)
    {
        shortcut = null;
        var slot = DigitSlot(virtualKey);
        if (slot is null || shift || control == alt)
        {
            return false;
        }

        shortcut = new MiniRecorderShortcut(
            control ? MiniRecorderShortcutKind.Prompt : MiniRecorderShortcutKind.PowerMode,
            slot.Value);
        return true;
    }

    private static int? DigitSlot(int virtualKey) =>
        virtualKey switch
        {
            >= 0x31 and <= 0x39 => virtualKey - 0x31,
            0x30 => 9,
            _ => null
        };
}
