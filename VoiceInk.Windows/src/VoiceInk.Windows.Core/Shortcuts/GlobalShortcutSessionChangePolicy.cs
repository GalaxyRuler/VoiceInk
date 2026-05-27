namespace VoiceInk.Windows.Core.Shortcuts;

public static class GlobalShortcutSessionChangePolicy
{
    public const int WmWtsSessionChange = 0x02B1;
    public const int WtsSessionLock = 0x7;
    public const int WtsSessionUnlock = 0x8;
    public const int WtsSessionDesktopReady = 0xF;

    public static bool ShouldResetPressedState(int message, int eventCode) =>
        message == WmWtsSessionChange
        && eventCode is WtsSessionLock or WtsSessionUnlock or WtsSessionDesktopReady;
}
