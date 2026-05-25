using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Backup;

public static class VoiceInkSettingsBackupMerger
{
    public static AppSettings Merge(
        AppSettings current,
        VoiceInkSettingsBackupFile backup,
        IEnumerable<VoiceInkSettingsBackupCategory> categories)
    {
        var selected = categories.ToHashSet();
        var result = current;

        if (selected.Contains(VoiceInkSettingsBackupCategory.General)
            && backup.GeneralSettings is not null)
        {
            result = backup.GeneralSettings with
            {
                CustomEnhancementPrompts = current.CustomEnhancementPrompts,
                SelectedEnhancementPromptId = current.SelectedEnhancementPromptId,
                PowerModeRules = current.PowerModeRules,
                ImportedWhisperModels = current.ImportedWhisperModels
            };
        }

        if (selected.Contains(VoiceInkSettingsBackupCategory.CustomPrompts))
        {
            result = result with
            {
                CustomEnhancementPrompts = backup.CustomPrompts.ToArray(),
                SelectedEnhancementPromptId = backup.GeneralSettings?.SelectedEnhancementPromptId
                    ?? result.SelectedEnhancementPromptId
            };
        }

        if (selected.Contains(VoiceInkSettingsBackupCategory.PowerMode))
        {
            result = result with { PowerModeRules = backup.PowerModeConfigs.ToArray() };
        }

        if (selected.Contains(VoiceInkSettingsBackupCategory.CustomModelDefinitions))
        {
            result = result with { ImportedWhisperModels = backup.ImportedWhisperModels.ToArray() };
        }

        return result;
    }
}
