using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Shortcuts;
using VoiceInk.Windows.Core.PowerMode;
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

    [Theory]
    [InlineData("Ctrl", 0x11, "Ctrl")]
    [InlineData("Alt", 0x12, "Alt")]
    [InlineData("Shift", 0x10, "Shift")]
    [InlineData("Ctrl+Alt", 0, "Ctrl+Alt")]
    public void TryParse_SupportsModifierOnlyRecordingShortcuts(
        string value,
        int expectedVirtualKey,
        string expectedDisplay)
    {
        var parsed = GlobalShortcut.TryParse(value, out var shortcut, out var error);

        Assert.True(parsed);
        Assert.Null(error);
        Assert.NotNull(shortcut);
        Assert.True(shortcut.IsModifierOnly);
        Assert.Equal(expectedVirtualKey, shortcut.VirtualKey);
        Assert.Equal(expectedDisplay, shortcut.DisplayText);
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
    public void TryCreateFromKeyCapture_FormatsCapturedShortcut()
    {
        var created = GlobalShortcut.TryCreateFromKeyCapture(
            control: true,
            alt: true,
            shift: false,
            virtualKey: 0x20,
            out var shortcut,
            out var error);

        Assert.True(created);
        Assert.Null(error);
        Assert.NotNull(shortcut);
        Assert.Equal("Ctrl+Alt+Space", shortcut.DisplayText);
    }

    [Theory]
    [InlineData(0x11)]
    [InlineData(0x12)]
    [InlineData(0x10)]
    [InlineData(0x5B)]
    [InlineData(0x5C)]
    public void TryCreateFromKeyCapture_RejectsModifierAndWindowsKeys(int virtualKey)
    {
        var created = GlobalShortcut.TryCreateFromKeyCapture(
            control: true,
            alt: true,
            shift: false,
            virtualKey,
            out var shortcut,
            out var error);

        Assert.False(created);
        Assert.Null(shortcut);
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    [Theory]
    [InlineData(0x11, "Ctrl")]
    [InlineData(0x12, "Alt")]
    [InlineData(0x10, "Shift")]
    public void TryCreateModifierOnlyFromKeyCapture_FormatsCapturedModifierOnlyShortcut(
        int virtualKey,
        string expectedDisplay)
    {
        var created = GlobalShortcut.TryCreateModifierOnlyFromKeyCapture(
            control: virtualKey == 0x11,
            alt: virtualKey == 0x12,
            shift: virtualKey == 0x10,
            virtualKey,
            out var shortcut,
            out var error);

        Assert.True(created);
        Assert.Null(error);
        Assert.NotNull(shortcut);
        Assert.True(shortcut.IsModifierOnly);
        Assert.Equal(expectedDisplay, shortcut.DisplayText);
    }

    [Fact]
    public void BuildRegistrations_AllowsModifierOnlyRecordingShortcut()
    {
        var result = GlobalShortcutSettings.BuildRegistrations(new AppSettings
        {
            Hotkey = "Ctrl",
            SecondaryRecordingHotkey = "Alt"
        });

        Assert.Empty(result.Errors);
        Assert.Collection(
            result.Registrations,
            item =>
            {
                Assert.Equal(GlobalShortcutAction.ToggleRecording, item.Action);
                Assert.True(item.Shortcut.IsModifierOnly);
                Assert.Equal("Ctrl", item.Shortcut.DisplayText);
            },
            item =>
            {
                Assert.Equal(GlobalShortcutAction.ToggleRecording, item.Action);
                Assert.True(item.Shortcut.IsModifierOnly);
                Assert.Equal("Alt", item.Shortcut.DisplayText);
            });
    }

    [Fact]
    public void BuildRegistrations_RejectsModifierOnlyUtilityShortcut()
    {
        var result = GlobalShortcutSettings.BuildRegistrations(new AppSettings
        {
            Hotkey = "Ctrl+Alt+Space",
            PasteLastTranscriptionHotkey = "Ctrl"
        });

        Assert.Contains(
            result.Errors,
            item => item == "Paste Last Transcription: Modifier-only shortcuts are only supported for recording.");
    }

    [Fact]
    public void BuildRegistrations_UsesPrimaryAndOptionalHotkeys()
    {
        var settings = new AppSettings
        {
            Hotkey = "Ctrl+Alt+Space",
            SecondaryRecordingHotkey = "Ctrl+Alt+S",
            PasteLastTranscriptionHotkey = "Ctrl+Alt+V",
            PasteLastEnhancementHotkey = "Ctrl+Alt+E",
            RetryLastTranscriptionHotkey = "Ctrl+Alt+R",
            CancelRecordingHotkey = "Ctrl+Alt+C",
            OpenHistoryHotkey = "Ctrl+Alt+H",
            QuickAddDictionaryHotkey = "Ctrl+Alt+D",
            ToggleEnhancementHotkey = "Ctrl+Alt+X",
            CyclePowerModeHotkey = "Ctrl+Alt+P"
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
                Assert.Equal(GlobalShortcutAction.ToggleRecording, item.Action);
                Assert.Equal("Ctrl+Alt+S", item.Shortcut.DisplayText);
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
            },
            item =>
            {
                Assert.Equal(GlobalShortcutAction.CancelRecording, item.Action);
                Assert.Equal("Ctrl+Alt+C", item.Shortcut.DisplayText);
            },
            item =>
            {
                Assert.Equal(GlobalShortcutAction.OpenHistoryWindow, item.Action);
                Assert.Equal("Ctrl+Alt+H", item.Shortcut.DisplayText);
            },
            item =>
            {
                Assert.Equal(GlobalShortcutAction.QuickAddToDictionary, item.Action);
                Assert.Equal("Ctrl+Alt+D", item.Shortcut.DisplayText);
            },
            item =>
            {
                Assert.Equal(GlobalShortcutAction.ToggleEnhancement, item.Action);
                Assert.Equal("Ctrl+Alt+X", item.Shortcut.DisplayText);
            },
            item =>
            {
                Assert.Equal(GlobalShortcutAction.CyclePowerMode, item.Action);
                Assert.Equal("Ctrl+Alt+P", item.Shortcut.DisplayText);
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
    public void BuildRegistrations_ReportsDuplicateSecondaryRecordingAssignment()
    {
        var result = GlobalShortcutSettings.BuildRegistrations(new AppSettings
        {
            Hotkey = "Ctrl+Alt+Space",
            SecondaryRecordingHotkey = "Ctrl+Alt+Space"
        });

        Assert.Contains(
            result.Errors,
            item => item == "Secondary Shortcut already uses Ctrl+Alt+Space.");
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

    [Fact]
    public void BuildRegistrations_ReportsDuplicateCancelRecordingAssignment()
    {
        var result = GlobalShortcutSettings.BuildRegistrations(new AppSettings
        {
            Hotkey = "Ctrl+Alt+Space",
            CancelRecordingHotkey = "Ctrl+Alt+Space"
        });

        Assert.Contains(
            result.Errors,
            item => item == "Cancel Recording already uses Ctrl+Alt+Space.");
    }

    [Fact]
    public void BuildRegistrations_ReportsDuplicateOpenHistoryAssignment()
    {
        var result = GlobalShortcutSettings.BuildRegistrations(new AppSettings
        {
            Hotkey = "Ctrl+Alt+Space",
            OpenHistoryHotkey = "Ctrl+Alt+Space"
        });

        Assert.Contains(
            result.Errors,
            item => item == "Open History Window already uses Ctrl+Alt+Space.");
    }

    [Fact]
    public void BuildRegistrations_ReportsDuplicateQuickAddAssignment()
    {
        var result = GlobalShortcutSettings.BuildRegistrations(new AppSettings
        {
            Hotkey = "Ctrl+Alt+Space",
            QuickAddDictionaryHotkey = "Ctrl+Alt+Space"
        });

        Assert.Contains(
            result.Errors,
            item => item == "Quick Add to Dictionary already uses Ctrl+Alt+Space.");
    }

    [Fact]
    public void BuildRegistrations_ReportsDuplicateToggleEnhancementAssignment()
    {
        var result = GlobalShortcutSettings.BuildRegistrations(new AppSettings
        {
            Hotkey = "Ctrl+Alt+Space",
            ToggleEnhancementHotkey = "Ctrl+Alt+Space"
        });

        Assert.Contains(
            result.Errors,
            item => item == "Toggle Enhancement already uses Ctrl+Alt+Space.");
    }

    [Fact]
    public void BuildRegistrations_ReportsDuplicateCyclePowerModeAssignment()
    {
        var result = GlobalShortcutSettings.BuildRegistrations(new AppSettings
        {
            Hotkey = "Ctrl+Alt+Space",
            CyclePowerModeHotkey = "Ctrl+Alt+Space"
        });

        Assert.Contains(
            result.Errors,
            item => item == "Cycle Power Mode already uses Ctrl+Alt+Space.");
    }

    [Fact]
    public void BuildRegistrations_AddsEnabledPowerModeRuleShortcutsWithRulePayload()
    {
        var ruleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var result = GlobalShortcutSettings.BuildRegistrations(new AppSettings
        {
            Hotkey = "Ctrl+Alt+Space",
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Id = ruleId,
                    Name = "Chat",
                    IsEnabled = true,
                    Shortcut = "Ctrl+Alt+1"
                },
                new PowerModeRule
                {
                    Name = "Disabled",
                    IsEnabled = false,
                    Shortcut = "Ctrl+Alt+2"
                }
            ]
        });

        Assert.Empty(result.Errors);
        Assert.Collection(
            result.Registrations,
            item => Assert.Equal(GlobalShortcutAction.ToggleRecording, item.Action),
            item =>
            {
                Assert.Equal(GlobalShortcutAction.SelectPowerModeRule, item.Action);
                Assert.Equal(ruleId, item.PowerModeRuleId);
                Assert.Equal("Ctrl+Alt+1", item.Shortcut.DisplayText);
            });
    }

    [Fact]
    public void BuildRegistrations_ReportsDuplicatePowerModeRuleShortcutAssignment()
    {
        var result = GlobalShortcutSettings.BuildRegistrations(new AppSettings
        {
            Hotkey = "Ctrl+Alt+Space",
            PasteLastTranscriptionHotkey = "Ctrl+Alt+1",
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Chat",
                    IsEnabled = true,
                    Shortcut = "Ctrl+Alt+1"
                }
            ]
        });

        Assert.Contains(
            result.Errors,
            item => item == "Power Mode: Chat already uses Ctrl+Alt+1.");
    }
}
