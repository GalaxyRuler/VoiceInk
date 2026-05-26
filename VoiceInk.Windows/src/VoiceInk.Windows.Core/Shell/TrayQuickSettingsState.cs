namespace VoiceInk.Windows.Core.Shell;

public sealed record TrayQuickSettingsState(
    IReadOnlyList<TrayMenuOption> TranscriptionModels,
    IReadOnlyList<TrayMenuOption> TranscriptionProviders,
    IReadOnlyList<TrayMenuOption> EnhancementPrompts,
    IReadOnlyList<TrayMenuOption> EnhancementProviders,
    IReadOnlyList<TrayMenuOption> Languages,
    IReadOnlyList<TrayMenuOption> AudioInputs,
    IReadOnlyList<TrayMenuOption> PowerModes,
    bool IsEnhancementEnabled,
    bool UseClipboardContext,
    bool UseOcrContext)
{
    public static TrayQuickSettingsState Empty { get; } = new(
        [],
        [],
        [],
        [],
        [],
        [],
        [],
        IsEnhancementEnabled: false,
        UseClipboardContext: false,
        UseOcrContext: false);
}
