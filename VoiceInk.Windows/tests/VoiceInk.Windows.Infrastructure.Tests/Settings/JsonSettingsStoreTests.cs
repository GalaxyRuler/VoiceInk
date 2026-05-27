using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Text;
using VoiceInk.Windows.Core.Models;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.PowerMode;
using VoiceInk.Windows.Infrastructure.Settings;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Settings;

public sealed class JsonSettingsStoreTests
{
    [Fact]
    public async Task LoadAsync_CreatesDefaultSettingsWhenFileIsMissing()
    {
        using var temp = new TempDirectory();
        var path = Path.Combine(temp.Path, "settings.json");
        var store = new JsonSettingsStore(path);

        var settings = await store.LoadAsync(CancellationToken.None);

        Assert.Equal(new AppSettings(), settings);
        Assert.True(File.Exists(path));

        var reloaded = await store.LoadAsync(CancellationToken.None);
        Assert.Equal(new AppSettings(), reloaded);
        Assert.True(reloaded.PrewarmModelOnWake);
        Assert.True(reloaded.IsVadEnabled);
        Assert.True(reloaded.IsTextFormattingEnabled);
        Assert.True(reloaded.AppendTrailingSpace);
        Assert.False(reloaded.IsMiddleClickRecordingEnabled);
        Assert.Equal(200, reloaded.MiddleClickActivationDelayMilliseconds);
        Assert.Equal("en", reloaded.Language);
    }

    [Fact]
    public async Task SaveAsync_PersistsSettings()
    {
        using var temp = new TempDirectory();
        var path = Path.Combine(temp.Path, "settings.json");
        var store = new JsonSettingsStore(path);
        var expected = new AppSettings
        {
            ModelPath = "C:\\Models\\ggml-base.en.bin",
            Language = "en",
            AppendTrailingSpace = true,
            RestoreClipboard = false,
            ClipboardRestoreDelaySeconds = 3.0,
            PasteMethod = "directText",
            LaunchAtLogin = true,
            StartHiddenToTray = true,
            PrewarmModelOnWake = false,
            IsVadEnabled = false,
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
            CyclePowerModeHotkey = "Ctrl+Alt+P",
            IsMiddleClickRecordingEnabled = true,
            MiddleClickActivationDelayMilliseconds = 350,
            AudioInputDeviceNumber = 2,
            AudioInputDeviceName = "USB Microphone",
            AudioInputEndpointId = "endpoint-usb",
            AudioInputMode = AudioInputModeSettings.Prioritized,
            PrioritizedAudioInputDevices =
            [
                new PrioritizedAudioInputDevice("Dock Microphone", 0, "endpoint-dock"),
                new PrioritizedAudioInputDevice("USB Microphone", 1, "endpoint-usb")
            ],
            HasCompletedOnboarding = true,
            IsTranscriptionCleanupEnabled = true,
            TranscriptionRetentionMinutes = 60,
            IsAudioCleanupEnabled = true,
            AudioRetentionPeriod = 14,
            TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
            CloudTranscriptionProviderId = "groq",
            CloudTranscriptionEndpoint = "https://api.example.test/v1/audio/transcriptions",
            CloudTranscriptionModel = "gpt-4o-transcribe",
            EnhancementProviderId = "gemini",
            EnhancementEndpoint = "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions",
            EnhancementModel = "gemini-2.5-flash-lite",
            CustomEnhancementPrompts =
            [
                new EnhancementPrompt(
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "Standup",
                    "Format as a terse standup update.",
                    "list.bullet",
                    "Daily update",
                    IsPredefined: false,
                    TriggerWords: ["standup mode"],
                    UseSystemInstructions: true)
            ],
            UseClipboardContext = true,
            UseOcrContext = true,
            UseOcrCaptureRegion = true,
            OcrCaptureRegionLeft = 12,
            OcrCaptureRegionTop = 34,
            OcrCaptureRegionWidth = 640,
            OcrCaptureRegionHeight = 360,
            FillerWords = ["um", "like", "you know"],
            IsPowerModeEnabled = false,
            ImportedWhisperModels =
            [
                new LocalWhisperModel(
                    "C:\\Models\\ggml-base.en.bin",
                    "ggml-base.en",
                    DateTimeOffset.Parse("2026-05-24T12:00:00Z"))
            ],
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Chat",
                    IsEnabled = true,
                    UseOcrContextOverride = true,
                    Shortcut = "Ctrl+Alt+1"
                }
            ]
        };

        await store.SaveAsync(expected, CancellationToken.None);
        var actual = await store.LoadAsync(CancellationToken.None);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task SaveAsync_PersistsCustomFillerWords()
    {
        using var temp = new TempDirectory();
        var path = Path.Combine(temp.Path, "settings.json");
        var store = new JsonSettingsStore(path);
        var expected = new AppSettings
        {
            FillerWords = ["um", "like", "you know"]
        };

        await store.SaveAsync(expected, CancellationToken.None);

        var content = await File.ReadAllTextAsync(path);
        Assert.Contains("\"FillerWords\": [", content);
        var actual = await store.LoadAsync(CancellationToken.None);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task SaveAsync_PersistsPunctuationCleanupModeAsMacStyleString()
    {
        using var temp = new TempDirectory();
        var path = Path.Combine(temp.Path, "settings.json");
        var store = new JsonSettingsStore(path);
        var expected = new AppSettings
        {
            PunctuationCleanupMode = PunctuationCleanupMode.RemoveAll,
            RemoveFillerWords = false,
            LowercaseTranscription = true
        };

        await store.SaveAsync(expected, CancellationToken.None);

        var content = await File.ReadAllTextAsync(path);
        Assert.Contains("\"PunctuationCleanupMode\": \"removeAll\"", content);
        var actual = await store.LoadAsync(CancellationToken.None);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task SaveAsync_PersistsPowerModeAutoSendKeyAsMacStyleString()
    {
        using var temp = new TempDirectory();
        var path = Path.Combine(temp.Path, "settings.json");
        var store = new JsonSettingsStore(path);
        var expected = new AppSettings
        {
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Chat",
                    ProcessNamePattern = "teams",
                    AutoSendKey = PowerModeAutoSendKey.CommandEnter
                }
            ]
        };

        await store.SaveAsync(expected, CancellationToken.None);

        var content = await File.ReadAllTextAsync(path);
        Assert.Contains("\"AutoSendKey\": \"commandEnter\"", content);
        var actual = await store.LoadAsync(CancellationToken.None);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("\"removeTrailingPeriod\"", PunctuationCleanupMode.RemoveTrailingPeriod)]
    [InlineData("2", PunctuationCleanupMode.RemoveTrailingPeriod)]
    public async Task LoadAsync_ReadsMacStyleAndLegacyNumericPunctuationCleanupModes(
        string jsonValue,
        PunctuationCleanupMode expectedMode)
    {
        using var temp = new TempDirectory();
        var path = Path.Combine(temp.Path, "settings.json");
        await File.WriteAllTextAsync(
            path,
            $$"""
            {
              "PunctuationCleanupMode": {{jsonValue}},
              "RemoveFillerWords": false,
              "LowercaseTranscription": true
            }
            """);
        var store = new JsonSettingsStore(path);

        var settings = await store.LoadAsync(CancellationToken.None);

        Assert.Equal(expectedMode, settings.PunctuationCleanupMode);
        Assert.False(settings.RemoveFillerWords);
        Assert.True(settings.LowercaseTranscription);
    }

    [Fact]
    public async Task SaveAsync_WhenCancelledBeforeWrite_PreservesExistingSettings()
    {
        using var temp = new TempDirectory();
        var path = Path.Combine(temp.Path, "settings.json");
        var store = new JsonSettingsStore(path);
        var original = new AppSettings
        {
            ModelPath = "C:\\Models\\original.bin",
            Language = "en",
            AppendTrailingSpace = true,
            RestoreClipboard = false,
            Hotkey = "Ctrl+Shift+O"
        };
        var replacement = original with
        {
            ModelPath = "C:\\Models\\replacement.bin",
            Language = "fr",
            Hotkey = "Ctrl+Shift+R"
        };
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await store.SaveAsync(original, CancellationToken.None);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => store.SaveAsync(replacement, cancellation.Token));

        var actual = await store.LoadAsync(CancellationToken.None);
        Assert.Equal(original, actual);
        Assert.Equal([path], Directory.GetFiles(temp.Path));
    }

    [Fact]
    public async Task SaveAsync_CreatesMissingDirectoryAndWritesIndentedJson()
    {
        using var temp = new TempDirectory();
        var path = Path.Combine(temp.Path, "nested", "settings.json");
        var store = new JsonSettingsStore(path);

        await store.SaveAsync(new AppSettings(), CancellationToken.None);

        Assert.True(File.Exists(path));
        var content = await File.ReadAllTextAsync(path);
        Assert.Contains($"{Environment.NewLine}  \"Language\"", content);
    }

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"voiceink-{Guid.NewGuid():N}");

        public TempDirectory()
        {
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
