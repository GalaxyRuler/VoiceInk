using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Shortcuts;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Shortcuts;

public sealed class RecordingShortcutModeSettingsTests
{
    [Theory]
    [InlineData(null, RecordingShortcutModeSettings.Toggle)]
    [InlineData("", RecordingShortcutModeSettings.Toggle)]
    [InlineData("toggle", RecordingShortcutModeSettings.Toggle)]
    [InlineData("pushToTalk", RecordingShortcutModeSettings.PushToTalk)]
    [InlineData("hybrid", RecordingShortcutModeSettings.Hybrid)]
    [InlineData("unexpected", RecordingShortcutModeSettings.Toggle)]
    public void Normalize_ReturnsSupportedRecordingShortcutModes(string? value, string expected)
    {
        Assert.Equal(expected, RecordingShortcutModeSettings.Normalize(value));
    }

    [Fact]
    public void BuildRegistrations_CarriesPrimaryAndSecondaryRecordingModes()
    {
        var settings = new AppSettings
        {
            Hotkey = "Ctrl+Alt+Space",
            PrimaryRecordingShortcutMode = RecordingShortcutModeSettings.PushToTalk,
            SecondaryRecordingHotkey = "Ctrl+Alt+S",
            SecondaryRecordingShortcutMode = RecordingShortcutModeSettings.Hybrid,
            PasteLastTranscriptionHotkey = "Ctrl+Alt+V"
        };

        var result = GlobalShortcutSettings.BuildRegistrations(settings);

        Assert.Empty(result.Errors);
        Assert.Contains(
            result.Registrations,
            registration => registration.Action == GlobalShortcutAction.ToggleRecording
                && registration.Shortcut.DisplayText == "Ctrl+Alt+Space"
                && registration.RecordingShortcutMode == RecordingShortcutModeSettings.PushToTalk);
        Assert.Contains(
            result.Registrations,
            registration => registration.Action == GlobalShortcutAction.ToggleRecording
                && registration.Shortcut.DisplayText == "Ctrl+Alt+S"
                && registration.RecordingShortcutMode == RecordingShortcutModeSettings.Hybrid);
        Assert.Contains(
            result.Registrations,
            registration => registration.Action == GlobalShortcutAction.PasteLastTranscription
                && registration.RecordingShortcutMode is null);
    }
}
