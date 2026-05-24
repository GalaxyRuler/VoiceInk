using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Text;
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
            Hotkey = "Ctrl+Shift+D",
            PasteLastTranscriptionHotkey = "Ctrl+Alt+V",
            PasteLastEnhancementHotkey = "Ctrl+Alt+E",
            RetryLastTranscriptionHotkey = "Ctrl+Alt+R",
            AudioInputDeviceNumber = 2,
            AudioInputDeviceName = "USB Microphone"
        };

        await store.SaveAsync(expected, CancellationToken.None);
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
