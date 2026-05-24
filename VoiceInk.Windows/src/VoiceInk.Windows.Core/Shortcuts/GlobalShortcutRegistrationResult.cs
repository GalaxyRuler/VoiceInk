namespace VoiceInk.Windows.Core.Shortcuts;

public sealed record GlobalShortcutRegistrationResult(
    IReadOnlyList<GlobalShortcutRegistration> Registrations,
    IReadOnlyList<string> Errors);
