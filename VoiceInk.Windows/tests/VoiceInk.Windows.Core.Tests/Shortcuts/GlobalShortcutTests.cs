using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Shortcuts;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Shortcuts;

public sealed class GlobalShortcutTests
{
    [Fact]
    public void TryParse_NormalizesCtrlAltSpace()
    {
        var parsed = GlobalShortcut.TryParse("Ctrl+Alt+Space", out var shortcut, out var error);

        Assert.True(parsed);
        Assert.Null(error);
        Assert.NotNull(shortcut);
        Assert.Equal("Ctrl+Alt+Space", shortcut.DisplayText);
        Assert.Equal(0x20, shortcut.VirtualKey);
        Assert.True(shortcut.Control);
        Assert.True(shortcut.Alt);
        Assert.False(shortcut.Shift);
    }

    [Fact]
    public void TryParse_NormalizesAliasesAndLetterKeys()
    {
        var parsed = GlobalShortcut.TryParse("control + shift + o", out var shortcut, out var error);

        Assert.True(parsed);
        Assert.Null(error);
        Assert.NotNull(shortcut);
        Assert.Equal("Ctrl+Shift+O", shortcut.DisplayText);
        Assert.Equal(0x4F, shortcut.VirtualKey);
        Assert.True(shortcut.Control);
        Assert.True(shortcut.Shift);
        Assert.False(shortcut.Alt);
    }

    [Fact]
    public void TryParse_RejectsWindowsKeyShortcuts()
    {
        var parsed = GlobalShortcut.TryParse("Win+Space", out var shortcut, out var error);

        Assert.False(parsed);
        Assert.Null(shortcut);
        Assert.Equal("Windows-key shortcuts are reserved by Windows.", error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Space")]
    [InlineData("Ctrl+Alt")]
    [InlineData("Ctrl+Alt+Space+V")]
    [InlineData("Ctrl+Alt+Unknown")]
    [InlineData("Ctrl++A")]
    [InlineData("+Ctrl+A")]
    [InlineData("Ctrl+A+")]
    public void TryParse_RejectsInvalidShortcuts(string value)
    {
        var parsed = GlobalShortcut.TryParse(value, out var shortcut, out var error);

        Assert.False(parsed);
        Assert.Null(shortcut);
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    [Theory]
    [InlineData("Ctrl+F12", 0x7B, "Ctrl+F12")]
    [InlineData("Alt+Escape", 0x1B, "Alt+Escape")]
    [InlineData("Shift+0", 0x30, "Shift+0")]
    public void TryParse_SupportsExpectedKeys(string value, int expectedVirtualKey, string expectedDisplay)
    {
        var parsed = GlobalShortcut.TryParse(value, out var shortcut, out var error);

        Assert.True(parsed);
        Assert.Null(error);
        Assert.NotNull(shortcut);
        Assert.Equal(expectedVirtualKey, shortcut.VirtualKey);
        Assert.Equal(expectedDisplay, shortcut.DisplayText);
    }

    [Fact]
    public void BuildRegistrations_UsesPrimaryAndOptionalHotkeys()
    {
        var settings = new AppSettings
        {
            Hotkey = "Ctrl+Alt+Space",
            PasteLastTranscriptionHotkey = "Ctrl+Alt+V",
            PasteLastEnhancementHotkey = "Ctrl+Alt+E",
            RetryLastTranscriptionHotkey = "Ctrl+Alt+R"
        };

        var result = GlobalShortcutSettings.BuildRegistrations(settings);

        Assert.Empty(result.Errors);
        Assert.Collection(
            result.Registrations,
            item =>
            {
                Assert.Equal(GlobalShortcutAction.ToggleRecording, item.Action);
                Assert.Equal("Ctrl+Alt+Space", item.Shortcut.DisplayText);
            },
            item =>
            {
                Assert.Equal(GlobalShortcutAction.PasteLastTranscription, item.Action);
                Assert.Equal("Ctrl+Alt+V", item.Shortcut.DisplayText);
            },
            item =>
            {
                Assert.Equal(GlobalShortcutAction.PasteLastEnhancedTranscription, item.Action);
                Assert.Equal("Ctrl+Alt+E", item.Shortcut.DisplayText);
            },
            item =>
            {
                Assert.Equal(GlobalShortcutAction.RetryLastTranscription, item.Action);
                Assert.Equal("Ctrl+Alt+R", item.Shortcut.DisplayText);
            });
    }

    [Fact]
    public void BuildRegistrations_AllowsMissingOptionalHotkeys()
    {
        var result = GlobalShortcutSettings.BuildRegistrations(new AppSettings
        {
            Hotkey = "Ctrl+Alt+Space"
        });

        Assert.Empty(result.Errors);
        var registration = Assert.Single(result.Registrations);
        Assert.Equal(GlobalShortcutAction.ToggleRecording, registration.Action);
    }

    [Fact]
    public void BuildRegistrations_ReportsInvalidPrimaryHotkey()
    {
        var result = GlobalShortcutSettings.BuildRegistrations(new AppSettings
        {
            Hotkey = "Space"
        });

        Assert.Empty(result.Registrations);
        Assert.Contains(
            result.Errors,
            item => item == "Primary Shortcut: A global shortcut must include at least one modifier.");
    }

    [Fact]
    public void BuildRegistrations_ReportsDuplicateAssignments()
    {
        var result = GlobalShortcutSettings.BuildRegistrations(new AppSettings
        {
            Hotkey = "Ctrl+Alt+Space",
            PasteLastTranscriptionHotkey = "Ctrl+Alt+Space"
        });

        Assert.Contains(
            result.Errors,
            item => item == "Paste Last Transcription already uses Ctrl+Alt+Space.");
    }

    [Fact]
    public void BuildRegistrations_ReportsDuplicateRetryLastAssignment()
    {
        var result = GlobalShortcutSettings.BuildRegistrations(new AppSettings
        {
            Hotkey = "Ctrl+Alt+Space",
            RetryLastTranscriptionHotkey = "Ctrl+Alt+Space"
        });

        Assert.Contains(
            result.Errors,
            item => item == "Retry Last Transcription already uses Ctrl+Alt+Space.");
    }
}
