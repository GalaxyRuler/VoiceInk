namespace VoiceInk.Windows.Core.Shortcuts;

public static class GlobalShortcutRegistrationFailurePresenter
{
    private const int ErrorHotkeyAlreadyRegistered = 1409;

    public static string Describe(
        GlobalShortcutAction action,
        GlobalShortcut shortcut,
        int win32ErrorCode,
        string win32ErrorMessage)
    {
        var detail = $"{win32ErrorCode}: {win32ErrorMessage}";
        if (win32ErrorCode == ErrorHotkeyAlreadyRegistered)
        {
            return $"RegisterHotKey failed for {action} ({shortcut.DisplayText}). "
                + "This shortcut is already registered by another app or Windows component. "
                + "Windows does not expose which app owns it; choose a different shortcut or close the conflicting app. "
                + $"Win32 detail: {detail}.";
        }

        return $"RegisterHotKey failed for {action} ({shortcut.DisplayText}) ({detail}).";
    }
}
