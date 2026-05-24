using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Shortcuts;

public static class GlobalShortcutSettings
{
    public static GlobalShortcutRegistrationResult BuildRegistrations(AppSettings settings)
    {
        var registrations = new List<GlobalShortcutRegistration>();
        var errors = new List<string>();
        var usedShortcuts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        AddRegistration(
            settings.Hotkey,
            GlobalShortcutAction.ToggleRecording,
            "Primary Shortcut",
            required: true,
            registrations,
            errors,
            usedShortcuts);
        AddRegistration(
            settings.PasteLastTranscriptionHotkey,
            GlobalShortcutAction.PasteLastTranscription,
            "Paste Last Transcription",
            required: false,
            registrations,
            errors,
            usedShortcuts);
        AddRegistration(
            settings.PasteLastEnhancementHotkey,
            GlobalShortcutAction.PasteLastEnhancedTranscription,
            "Paste Last Enhanced Transcription",
            required: false,
            registrations,
            errors,
            usedShortcuts);
        AddRegistration(
            settings.RetryLastTranscriptionHotkey,
            GlobalShortcutAction.RetryLastTranscription,
            "Retry Last Transcription",
            required: false,
            registrations,
            errors,
            usedShortcuts);

        return new GlobalShortcutRegistrationResult(registrations, errors);
    }

    private static void AddRegistration(
        string shortcutText,
        GlobalShortcutAction action,
        string displayName,
        bool required,
        ICollection<GlobalShortcutRegistration> registrations,
        ICollection<string> errors,
        IDictionary<string, string> usedShortcuts)
    {
        if (string.IsNullOrWhiteSpace(shortcutText))
        {
            if (required)
            {
                errors.Add($"{displayName}: Shortcut is required.");
            }

            return;
        }

        if (!GlobalShortcut.TryParse(shortcutText, out var shortcut, out var error))
        {
            errors.Add($"{displayName}: {error}");
            return;
        }

        if (usedShortcuts.ContainsKey(shortcut!.DisplayText))
        {
            errors.Add($"{displayName} already uses {shortcut.DisplayText}.");
            return;
        }

        usedShortcuts.Add(shortcut.DisplayText, displayName);
        registrations.Add(new GlobalShortcutRegistration(action, shortcut));
    }
}
