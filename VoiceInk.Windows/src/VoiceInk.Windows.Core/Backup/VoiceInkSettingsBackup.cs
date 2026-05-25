using System.Text.Json;
using System.Text.Json.Serialization;
using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Backup;

public static class VoiceInkSettingsBackup
{
    public const string SchemaVersion = "windows-1.0";
    public const string SecretNoticeText = "API keys are not exported. Reconfigure provider keys locally after import.";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Export(
        AppSettings settings,
        IEnumerable<VocabularyWord> vocabulary,
        IEnumerable<WordReplacement> replacements,
        DateTimeOffset exportedAtUtc)
    {
        var dictionary = DictionaryBackup.Parse(DictionaryBackup.Export(vocabulary, replacements));
        var backup = new VoiceInkSettingsBackupFile
        {
            Version = SchemaVersion,
            Platform = "windows",
            ExportedAtUtc = exportedAtUtc.ToUniversalTime(),
            SecretNotice = SecretNoticeText,
            GeneralSettings = GeneralSettingsForExport(settings),
            CustomPrompts = settings.CustomEnhancementPrompts.ToArray(),
            PowerModeConfigs = settings.PowerModeRules.ToArray(),
            ImportedWhisperModels = settings.ImportedWhisperModels.ToArray(),
            VocabularyWords = dictionary.VocabularyWords
                .Select(word => new VoiceInkSettingsBackupWord(word))
                .ToArray(),
            WordReplacements = dictionary.WordReplacements
                .OrderBy(replacement => replacement.OriginalText, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    replacement => replacement.OriginalText,
                    replacement => replacement.ReplacementText,
                    StringComparer.OrdinalIgnoreCase)
        };

        return JsonSerializer.Serialize(backup, JsonOptions);
    }

    public static VoiceInkSettingsBackupFile Parse(string json)
    {
        var backup = JsonSerializer.Deserialize<VoiceInkSettingsBackupFile>(json, JsonOptions);
        if (backup is null)
        {
            throw new JsonException("Settings backup file is empty.");
        }

        return backup with
        {
            Version = string.IsNullOrWhiteSpace(backup.Version) ? "unknown" : backup.Version,
            Platform = string.IsNullOrWhiteSpace(backup.Platform) ? "unknown" : backup.Platform,
            SecretNotice = string.IsNullOrWhiteSpace(backup.SecretNotice)
                ? SecretNoticeText
                : backup.SecretNotice,
            CustomPrompts = backup.CustomPrompts ?? [],
            PowerModeConfigs = backup.PowerModeConfigs ?? [],
            ImportedWhisperModels = backup.ImportedWhisperModels ?? [],
            VocabularyWords = backup.VocabularyWords ?? [],
            WordReplacements = backup.WordReplacements ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        };
    }

    public static string ExportDictionaryJson(VoiceInkSettingsBackupFile backup)
    {
        var exportedAt = backup.ExportedAtUtc == default
            ? DateTimeOffset.UtcNow
            : backup.ExportedAtUtc;
        var vocabulary = backup.VocabularyWords
            .Select(word => new VocabularyWord(Guid.NewGuid(), word.Word, exportedAt));
        var replacements = backup.WordReplacements
            .Select(pair => new WordReplacement(Guid.NewGuid(), pair.Key, pair.Value, exportedAt));

        return DictionaryBackup.Export(vocabulary, replacements);
    }

    private static AppSettings GeneralSettingsForExport(AppSettings settings) =>
        settings with
        {
            CloudTranscriptionEndpoint = VoiceInkSettingsBackupEndpointSanitizer.Sanitize(
                settings.CloudTranscriptionEndpoint),
            EnhancementEndpoint = VoiceInkSettingsBackupEndpointSanitizer.Sanitize(
                settings.EnhancementEndpoint),
            CustomEnhancementPrompts = [],
            PowerModeRules = [],
            ImportedWhisperModels = []
        };
}
