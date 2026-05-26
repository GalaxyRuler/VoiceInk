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
            usedShortcuts,
            recordingShortcutMode: RecordingShortcutModeSettings.Normalize(settings.PrimaryRecordingShortcutMode));
        AddRegistration(
            settings.SecondaryRecordingHotkey,
            GlobalShortcutAction.ToggleRecording,
            "Secondary Shortcut",
            required: false,
            registrations,
            errors,
            usedShortcuts,
            recordingShortcutMode: RecordingShortcutModeSettings.Normalize(settings.SecondaryRecordingShortcutMode));
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
        AddRegistration(
            settings.CancelRecordingHotkey,
            GlobalShortcutAction.CancelRecording,
            "Cancel Recording",
            required: false,
            registrations,
            errors,
            usedShortcuts);
        AddRegistration(
            settings.OpenHistoryHotkey,
            GlobalShortcutAction.OpenHistoryWindow,
            "Open History Window",
            required: false,
            registrations,
            errors,
            usedShortcuts);
        AddRegistration(
            settings.QuickAddDictionaryHotkey,
            GlobalShortcutAction.QuickAddToDictionary,
            "Quick Add to Dictionary",
            required: false,
            registrations,
            errors,
            usedShortcuts);
        AddRegistration(
            settings.ToggleEnhancementHotkey,
            GlobalShortcutAction.ToggleEnhancement,
            "Toggle Enhancement",
            required: false,
            registrations,
            errors,
            usedShortcuts);
        AddRegistration(
            settings.CyclePowerModeHotkey,
            GlobalShortcutAction.CyclePowerMode,
            "Cycle Power Mode",
            required: false,
            registrations,
            errors,
            usedShortcuts);
        foreach (var rule in settings.PowerModeRules.Where(rule => rule.IsEnabled))
        {
            AddRegistration(
                rule.Shortcut,
                GlobalShortcutAction.SelectPowerModeRule,
                $"Power Mode: {PowerModeRuleDisplayName(rule)}",
                required: false,
                registrations,
                errors,
                usedShortcuts,
                rule.Id);
        }

        return new GlobalShortcutRegistrationResult(registrations, errors);
    }

    private static void AddRegistration(
        string shortcutText,
        GlobalShortcutAction action,
        string displayName,
        bool required,
        ICollection<GlobalShortcutRegistration> registrations,
        ICollection<string> errors,
        IDictionary<string, string> usedShortcuts,
        Guid? powerModeRuleId = null,
        string? recordingShortcutMode = null)
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
        registrations.Add(new GlobalShortcutRegistration(action, shortcut, powerModeRuleId, recordingShortcutMode));
    }

    private static string PowerModeRuleDisplayName(PowerMode.PowerModeRule rule) =>
        string.IsNullOrWhiteSpace(rule.Name) ? "New Power Mode" : rule.Name.Trim();
}
