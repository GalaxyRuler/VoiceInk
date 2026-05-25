using System.Text.Json.Serialization;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.Models;
using VoiceInk.Windows.Core.PowerMode;
using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Backup;

public sealed record VoiceInkSettingsBackupFile
{
    [JsonPropertyName("version")]
    public string Version { get; init; } = VoiceInkSettingsBackup.SchemaVersion;

    [JsonPropertyName("platform")]
    public string Platform { get; init; } = "windows";

    [JsonPropertyName("exportedAtUtc")]
    public DateTimeOffset ExportedAtUtc { get; init; }

    [JsonPropertyName("secretNotice")]
    public string SecretNotice { get; init; } = VoiceInkSettingsBackup.SecretNoticeText;

    [JsonPropertyName("generalSettings")]
    public AppSettings? GeneralSettings { get; init; }

    [JsonPropertyName("customPrompts")]
    public EnhancementPrompt[] CustomPrompts { get; init; } = [];

    [JsonPropertyName("powerModeConfigs")]
    public PowerModeRule[] PowerModeConfigs { get; init; } = [];

    [JsonPropertyName("importedWhisperModels")]
    public LocalWhisperModel[] ImportedWhisperModels { get; init; } = [];

    [JsonPropertyName("vocabularyWords")]
    public VoiceInkSettingsBackupWord[] VocabularyWords { get; init; } = [];

    [JsonPropertyName("wordReplacements")]
    public Dictionary<string, string> WordReplacements { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed record VoiceInkSettingsBackupWord([property: JsonPropertyName("word")] string Word);
