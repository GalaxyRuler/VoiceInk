using VoiceInk.Windows.Core.Backup;
using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.Models;
using VoiceInk.Windows.Core.PowerMode;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Text;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Backup;

public sealed class VoiceInkSettingsBackupTests
{
    [Fact]
    public void Export_RoundTripsSettingsPromptsPowerModesModelsAndDictionary()
    {
        var promptId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var exportedAt = DateTimeOffset.Parse("2026-05-25T12:34:56Z");
        var settings = RichSettings(promptId);
        var vocabulary = new[]
        {
            new VocabularyWord(Guid.NewGuid(), "VoiceInk", exportedAt),
            new VocabularyWord(Guid.NewGuid(), "WinUI", exportedAt)
        };
        var replacements = new[]
        {
            new WordReplacement(Guid.NewGuid(), "voice ink", "VoiceInk", exportedAt),
            new WordReplacement(Guid.NewGuid(), "disabled", "Ignored", exportedAt, IsEnabled: false)
        };

        var json = VoiceInkSettingsBackup.Export(settings, vocabulary, replacements, exportedAt);
        var backup = VoiceInkSettingsBackup.Parse(json);

        Assert.Equal("windows-1.0", backup.Version);
        Assert.Equal("windows", backup.Platform);
        Assert.Equal(exportedAt, backup.ExportedAtUtc);
        Assert.Contains("API keys are not exported", backup.SecretNotice);
        Assert.NotNull(backup.GeneralSettings);
        Assert.Empty(backup.GeneralSettings.CustomEnhancementPrompts);
        Assert.Empty(backup.GeneralSettings.PowerModeRules);
        Assert.Empty(backup.GeneralSettings.ImportedWhisperModels);
        Assert.Equal(settings.ModelPath, backup.GeneralSettings.ModelPath);
        Assert.Equal(settings.LaunchAtLogin, backup.GeneralSettings.LaunchAtLogin);
        Assert.Equal(settings.PrewarmModelOnWake, backup.GeneralSettings.PrewarmModelOnWake);
        Assert.Equal(settings.ShowLiveTranscriptPreview, backup.GeneralSettings.ShowLiveTranscriptPreview);
        Assert.Equal(settings.RecorderStyle, backup.GeneralSettings.RecorderStyle);
        Assert.Equal(settings.IsSoundFeedbackEnabled, backup.GeneralSettings.IsSoundFeedbackEnabled);
        Assert.Equal(settings.StartSoundMode, backup.GeneralSettings.StartSoundMode);
        Assert.Equal(settings.StopSoundMode, backup.GeneralSettings.StopSoundMode);
        Assert.Equal(settings.CustomStartSoundPath, backup.GeneralSettings.CustomStartSoundPath);
        Assert.Equal(settings.CustomStopSoundPath, backup.GeneralSettings.CustomStopSoundPath);
        Assert.Equal(settings.IsSystemMuteEnabled, backup.GeneralSettings.IsSystemMuteEnabled);
        Assert.Equal(settings.IsPauseMediaEnabled, backup.GeneralSettings.IsPauseMediaEnabled);
        Assert.Equal(settings.AudioResumptionDelaySeconds, backup.GeneralSettings.AudioResumptionDelaySeconds);
        Assert.Equal(settings.CloudTranscriptionEndpoint, backup.GeneralSettings.CloudTranscriptionEndpoint);
        Assert.Equal(settings.EnhancementEndpoint, backup.GeneralSettings.EnhancementEndpoint);
        Assert.Equal(settings.SelectedEnhancementPromptId, backup.GeneralSettings.SelectedEnhancementPromptId);
        Assert.Equal(settings.SelectedPowerModeRuleId, backup.GeneralSettings.SelectedPowerModeRuleId);
        var prompt = Assert.Single(backup.CustomPrompts);
        Assert.Equal(promptId, prompt.Id);
        Assert.Equal("Standup", prompt.Title);
        Assert.Equal(["standup mode"], prompt.TriggerWords);
        Assert.Equal(settings.PowerModeRules, backup.PowerModeConfigs);
        Assert.Equal(settings.ImportedWhisperModels, backup.ImportedWhisperModels);
        Assert.Equal(["VoiceInk", "WinUI"], backup.VocabularyWords.Select(word => word.Word).ToArray());
        Assert.Equal("VoiceInk", backup.WordReplacements["voice ink"]);
        Assert.False(backup.WordReplacements.ContainsKey("disabled"));
    }

    [Fact]
    public void Export_DoesNotWriteApiKeysOrSecretFields()
    {
        var settings = RichSettings(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        var json = VoiceInkSettingsBackup.Export(settings, [], [], DateTimeOffset.Parse("2026-05-25T12:34:56Z"));

        Assert.Contains("API keys are not exported", json);
        Assert.DoesNotContain("\"apiKey\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"secretKey\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"credential\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"token\"", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Export_RedactsCredentialBearingEndpoints()
    {
        var settings = RichSettings(Guid.Parse("11111111-1111-1111-1111-111111111111")) with
        {
            CloudTranscriptionEndpoint = "https://user:sk-cloud-secret@api.example.test/v1/audio/transcriptions",
            EnhancementEndpoint = "https://api.example.test/v1/chat/completions?api_key=sk-enhancement-secret"
        };

        var json = VoiceInkSettingsBackup.Export(settings, [], [], DateTimeOffset.Parse("2026-05-25T12:34:56Z"));
        var backup = VoiceInkSettingsBackup.Parse(json);

        Assert.Equal(string.Empty, backup.GeneralSettings?.CloudTranscriptionEndpoint);
        Assert.Equal(string.Empty, backup.GeneralSettings?.EnhancementEndpoint);
        Assert.DoesNotContain("sk-cloud-secret", json, StringComparison.Ordinal);
        Assert.DoesNotContain("sk-enhancement-secret", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Export_RedactsMalformedOrPrefixedSecretEndpointQueries()
    {
        var settings = RichSettings(Guid.Parse("11111111-1111-1111-1111-111111111111")) with
        {
            CloudTranscriptionEndpoint = "api.example.test/v1/audio/transcriptions?api_key=sk-relative-secret",
            EnhancementEndpoint = "https://api.example.test/v1/chat/completions?x-api-key=sk-prefixed-secret"
        };

        var json = VoiceInkSettingsBackup.Export(settings, [], [], DateTimeOffset.Parse("2026-05-25T12:34:56Z"));
        var backup = VoiceInkSettingsBackup.Parse(json);

        Assert.Equal(string.Empty, backup.GeneralSettings?.CloudTranscriptionEndpoint);
        Assert.Equal(string.Empty, backup.GeneralSettings?.EnhancementEndpoint);
        Assert.DoesNotContain("sk-relative-secret", json, StringComparison.Ordinal);
        Assert.DoesNotContain("sk-prefixed-secret", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Export_RedactsProtocolRelativeCredentialBearingEndpoints()
    {
        var settings = RichSettings(Guid.Parse("11111111-1111-1111-1111-111111111111")) with
        {
            CloudTranscriptionEndpoint = "//user:sk-protocol-relative@api.example.test/v1/audio/transcriptions"
        };

        var json = VoiceInkSettingsBackup.Export(settings, [], [], DateTimeOffset.Parse("2026-05-25T12:34:56Z"));
        var backup = VoiceInkSettingsBackup.Parse(json);

        Assert.Equal(string.Empty, backup.GeneralSettings?.CloudTranscriptionEndpoint);
        Assert.DoesNotContain("sk-protocol-relative", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Export_RedactsPrefixedTokenSecretAndCredentialQueries()
    {
        var settings = RichSettings(Guid.Parse("11111111-1111-1111-1111-111111111111")) with
        {
            CloudTranscriptionEndpoint = "https://api.example.test/v1/audio/transcriptions?refresh_token=sk-refresh-token",
            EnhancementEndpoint = "https://api.example.test/v1/chat/completions?x-secret=sk-query-secret&x-credential=sk-query-credential"
        };

        var json = VoiceInkSettingsBackup.Export(settings, [], [], DateTimeOffset.Parse("2026-05-25T12:34:56Z"));
        var backup = VoiceInkSettingsBackup.Parse(json);

        Assert.Equal(string.Empty, backup.GeneralSettings?.CloudTranscriptionEndpoint);
        Assert.Equal(string.Empty, backup.GeneralSettings?.EnhancementEndpoint);
        Assert.DoesNotContain("sk-refresh-token", json, StringComparison.Ordinal);
        Assert.DoesNotContain("sk-query-secret", json, StringComparison.Ordinal);
        Assert.DoesNotContain("sk-query-credential", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Merge_GeneralSettingsPreservesUnselectedCategoryData()
    {
        var currentPromptId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var importedPromptId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var current = RichSettings(currentPromptId) with
        {
            ModelPath = "C:\\Models\\current.bin",
            Hotkey = "Ctrl+Alt+Space",
            CloudTranscriptionEndpoint = "https://current.example.test/v1/audio/transcriptions"
        };
        var backup = VoiceInkSettingsBackup.Parse(VoiceInkSettingsBackup.Export(
            RichSettings(importedPromptId),
            [],
            [],
            DateTimeOffset.Parse("2026-05-25T12:34:56Z")));

        var merged = VoiceInkSettingsBackupMerger.Merge(
            current,
            backup,
            new[] { VoiceInkSettingsBackupCategory.General });

        Assert.Equal("C:\\Models\\ggml-base.en.bin", merged.ModelPath);
        Assert.True(merged.LaunchAtLogin);
        Assert.False(merged.PrewarmModelOnWake);
        Assert.True(merged.ShowLiveTranscriptPreview);
        Assert.Equal("notch", merged.RecorderStyle);
        Assert.False(merged.IsSoundFeedbackEnabled);
        Assert.Equal("custom", merged.StartSoundMode);
        Assert.Equal("custom", merged.StopSoundMode);
        Assert.Equal(@"C:\VoiceInk\Sounds\CustomStartSound.wav", merged.CustomStartSoundPath);
        Assert.Equal(@"C:\VoiceInk\Sounds\CustomStopSound.mp3", merged.CustomStopSoundPath);
        Assert.True(merged.IsSystemMuteEnabled);
        Assert.True(merged.IsPauseMediaEnabled);
        Assert.Equal(4.0, merged.AudioResumptionDelaySeconds);
        Assert.Equal("Ctrl+Shift+D", merged.Hotkey);
        Assert.Equal("https://api.example.test/v1/audio/transcriptions", merged.CloudTranscriptionEndpoint);
        Assert.Equal(current.CustomEnhancementPrompts, merged.CustomEnhancementPrompts);
        Assert.Equal(current.SelectedEnhancementPromptId, merged.SelectedEnhancementPromptId);
        Assert.Equal(current.PowerModeRules, merged.PowerModeRules);
        Assert.Equal(current.SelectedPowerModeRuleId, merged.SelectedPowerModeRuleId);
        Assert.Equal(current.ImportedWhisperModels, merged.ImportedWhisperModels);
    }

    [Fact]
    public void Merge_SelectedCategoriesReplaceOnlyThoseSettings()
    {
        var current = new AppSettings
        {
            ModelPath = "C:\\Models\\current.bin",
            Hotkey = "Ctrl+Alt+Space",
            CustomEnhancementPrompts =
            [
                new EnhancementPrompt(
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    "Current",
                    "Keep current prompt.",
                    "document",
                    "Current prompt",
                    IsPredefined: false,
                    TriggerWords: [],
                    UseSystemInstructions: false)
            ],
            ImportedWhisperModels =
            [
                new LocalWhisperModel(
                    "C:\\Models\\current.bin",
                    "current",
                    DateTimeOffset.Parse("2026-05-24T12:00:00Z"))
            ],
            PowerModeRules =
            [
                new PowerModeRule { Name = "Current", ProcessNamePattern = "current.exe" }
            ]
        };
        var promptId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var backup = VoiceInkSettingsBackup.Parse(VoiceInkSettingsBackup.Export(
            RichSettings(promptId),
            [],
            [],
            DateTimeOffset.Parse("2026-05-25T12:34:56Z")));

        var merged = VoiceInkSettingsBackupMerger.Merge(
            current,
            backup,
            new[]
            {
                VoiceInkSettingsBackupCategory.CustomPrompts,
                VoiceInkSettingsBackupCategory.PowerMode,
                VoiceInkSettingsBackupCategory.CustomModelDefinitions
            });

        Assert.Equal(current.ModelPath, merged.ModelPath);
        Assert.Equal(current.Hotkey, merged.Hotkey);
        Assert.Equal("Standup", Assert.Single(merged.CustomEnhancementPrompts).Title);
        Assert.Equal(promptId, merged.SelectedEnhancementPromptId);
        Assert.Equal("Terminal", Assert.Single(merged.PowerModeRules).Name);
        Assert.Equal(Guid.Parse("33333333-3333-3333-3333-333333333333"), merged.SelectedPowerModeRuleId);
        Assert.Equal("ggml-base.en", Assert.Single(merged.ImportedWhisperModels).DisplayName);
    }

    [Fact]
    public void ExportDictionaryJson_CanBeParsedByDictionaryBackup()
    {
        var exportedAt = DateTimeOffset.Parse("2026-05-25T12:34:56Z");
        var backup = VoiceInkSettingsBackup.Parse(VoiceInkSettingsBackup.Export(
            new AppSettings(),
            [new VocabularyWord(Guid.NewGuid(), "VoiceInk", exportedAt)],
            [new WordReplacement(Guid.NewGuid(), "voice ink", "VoiceInk", exportedAt)],
            exportedAt));

        var dictionaryJson = VoiceInkSettingsBackup.ExportDictionaryJson(backup);
        var dictionary = DictionaryBackup.Parse(dictionaryJson);

        Assert.Equal(["VoiceInk"], dictionary.VocabularyWords);
        var replacement = Assert.Single(dictionary.WordReplacements);
        Assert.Equal("voice ink", replacement.OriginalText);
        Assert.Equal("VoiceInk", replacement.ReplacementText);
    }

    private static AppSettings RichSettings(Guid promptId)
    {
        var now = DateTimeOffset.Parse("2026-05-25T12:34:56Z");
        return new AppSettings
        {
            HasCompletedOnboarding = true,
            ModelPath = "C:\\Models\\ggml-base.en.bin",
            Language = "en",
            AppendTrailingSpace = true,
            RestoreClipboard = false,
            LaunchAtLogin = true,
            PrewarmModelOnWake = false,
            ShowLiveTranscriptPreview = true,
            RecorderStyle = "notch",
            IsSoundFeedbackEnabled = false,
            StartSoundMode = "custom",
            StopSoundMode = "custom",
            CustomStartSoundPath = @"C:\VoiceInk\Sounds\CustomStartSound.wav",
            CustomStopSoundPath = @"C:\VoiceInk\Sounds\CustomStopSound.mp3",
            IsSystemMuteEnabled = true,
            IsPauseMediaEnabled = true,
            AudioResumptionDelaySeconds = 4.0,
            Hotkey = "Ctrl+Shift+D",
            SecondaryRecordingHotkey = "Ctrl+Alt+S",
            PasteLastTranscriptionHotkey = "Ctrl+Alt+V",
            PasteLastEnhancementHotkey = "Ctrl+Alt+E",
            RetryLastTranscriptionHotkey = "Ctrl+Alt+R",
            CancelRecordingHotkey = "Ctrl+Alt+C",
            OpenHistoryHotkey = "Ctrl+Alt+H",
            QuickAddDictionaryHotkey = "Ctrl+Alt+D",
            ToggleEnhancementHotkey = "Ctrl+Alt+X",
            AudioInputDeviceNumber = 2,
            AudioInputDeviceName = "USB Microphone",
            ImportedWhisperModels =
            [
                new LocalWhisperModel("C:\\Models\\ggml-base.en.bin", "ggml-base.en", now)
            ],
            TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
            CloudTranscriptionProviderId = "groq",
            CloudTranscriptionEndpoint = "https://api.example.test/v1/audio/transcriptions",
            CloudTranscriptionModel = "whisper-large-v3",
            IsEnhancementEnabled = true,
            EnhancementProviderId = "gemini",
            EnhancementEndpoint = "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions",
            EnhancementModel = "gemini-2.5-flash-lite",
            CustomEnhancementPrompts =
            [
                new EnhancementPrompt(
                    promptId,
                    "Standup",
                    "Format as a terse standup update.",
                    "list.bullet",
                    "Daily update",
                    IsPredefined: false,
                    TriggerWords: ["standup mode"],
                    UseSystemInstructions: true)
            ],
            SelectedEnhancementPromptId = promptId,
            EnhancementTimeoutSeconds = 11,
            EnhancementRetryOnTimeout = false,
            SkipShortEnhancement = false,
            ShortEnhancementWordThreshold = 8,
            UseClipboardContext = true,
            RemoveFillerWords = false,
            PunctuationCleanupMode = PunctuationCleanupMode.RemoveTrailingPeriod,
            LowercaseTranscription = true,
            SelectedPowerModeRuleId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Terminal",
                    Emoji = ">",
                    ProcessNamePattern = "wt.exe",
                    WindowTitlePattern = "PowerShell",
                    ModelPathOverride = "C:\\Models\\terminal.bin",
                    LanguageOverride = "en",
                    IsEnhancementEnabledOverride = false,
                    SelectedEnhancementPromptIdOverride = promptId,
                    AppendTrailingSpaceOverride = false,
                    RemoveFillerWordsOverride = true,
                    PunctuationCleanupModeOverride = PunctuationCleanupMode.Keep,
                    LowercaseTranscriptionOverride = false
                }
            ]
        };
    }
}
