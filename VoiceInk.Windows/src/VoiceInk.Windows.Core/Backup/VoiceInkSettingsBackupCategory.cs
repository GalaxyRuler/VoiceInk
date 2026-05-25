namespace VoiceInk.Windows.Core.Backup;

public enum VoiceInkSettingsBackupCategory
{
    General,
    CustomPrompts,
    PowerMode,
    Dictionary,
    CustomModelDefinitions
}

public sealed record VoiceInkSettingsBackupCategoryChoice(
    VoiceInkSettingsBackupCategory Category,
    string Title);

public static class VoiceInkSettingsBackupCategories
{
    public static IReadOnlyList<VoiceInkSettingsBackupCategoryChoice> All { get; } =
    [
        new(VoiceInkSettingsBackupCategory.General, "General Settings"),
        new(VoiceInkSettingsBackupCategory.CustomPrompts, "Custom Prompts"),
        new(VoiceInkSettingsBackupCategory.PowerMode, "Power Mode"),
        new(VoiceInkSettingsBackupCategory.Dictionary, "Dictionary"),
        new(VoiceInkSettingsBackupCategory.CustomModelDefinitions, "Custom Model Definitions")
    ];
}
