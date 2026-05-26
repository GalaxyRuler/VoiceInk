namespace VoiceInk.Windows.Core.Shortcuts;

public sealed record GlobalShortcutRegistration(
    GlobalShortcutAction Action,
    GlobalShortcut Shortcut,
    Guid? PowerModeRuleId = null,
    string? RecordingShortcutMode = null);
