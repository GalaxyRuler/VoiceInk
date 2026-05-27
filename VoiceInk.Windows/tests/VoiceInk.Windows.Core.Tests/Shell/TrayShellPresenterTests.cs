using VoiceInk.Windows.Core.Dictation;
using VoiceInk.Windows.Core.Shell;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Shell;

public sealed class TrayShellPresenterTests
{
    [Fact]
    public void FromState_IdleWithLoadedSettings_EnablesStartRecording()
    {
        var state = TrayShellPresenter.FromState(
            settingsLoaded: true,
            dictationState: DictationState.Idle,
            operationActive: false,
            statusOverride: null);

        Assert.Equal("Idle", state.StatusText);
        Assert.Equal("Start Recording", state.ToggleRecordingLabel);
        Assert.True(state.CanToggleRecording);
        Assert.True(state.CanQuickAddDictionary);
        Assert.True(state.CanOpenHistory);
        Assert.True(state.CanUseQuickSettings);
        Assert.Equal(
            "If the tray icon is hidden, open Windows taskbar corner overflow and pin VoiceInk; the main window can always be opened from the tray menu.",
            state.VisibilityGuidance);
    }

    [Fact]
    public void FromState_RecordingWithLoadedSettings_EnablesStopRecording()
    {
        var state = TrayShellPresenter.FromState(
            settingsLoaded: true,
            dictationState: DictationState.Recording,
            operationActive: false,
            statusOverride: null);

        Assert.Equal("Recording", state.StatusText);
        Assert.Equal("Stop Recording", state.ToggleRecordingLabel);
        Assert.True(state.CanToggleRecording);
        Assert.False(state.CanQuickAddDictionary);
        Assert.True(state.CanOpenHistory);
        Assert.False(state.CanUseQuickSettings);
    }

    [Fact]
    public void FromState_BusyTranscribing_DisablesOperationalCommands()
    {
        var state = TrayShellPresenter.FromState(
            settingsLoaded: true,
            dictationState: DictationState.Transcribing,
            operationActive: true,
            statusOverride: "Stopping and inserting");

        Assert.Equal("Stopping and inserting", state.StatusText);
        Assert.Equal("Start Recording", state.ToggleRecordingLabel);
        Assert.False(state.CanToggleRecording);
        Assert.False(state.CanQuickAddDictionary);
        Assert.False(state.CanOpenHistory);
        Assert.False(state.CanUseQuickSettings);
    }

    [Fact]
    public void FromState_TranscribingWithoutOperationFlag_DisablesHistory()
    {
        var state = TrayShellPresenter.FromState(
            settingsLoaded: true,
            dictationState: DictationState.Transcribing,
            operationActive: false,
            statusOverride: null);

        Assert.Equal("Transcribing", state.StatusText);
        Assert.False(state.CanOpenHistory);
    }

    [Fact]
    public void FromState_SettingsNotLoaded_DisablesOperationalCommands()
    {
        var state = TrayShellPresenter.FromState(
            settingsLoaded: false,
            dictationState: DictationState.Idle,
            operationActive: false,
            statusOverride: null);

        Assert.Equal("Loading settings", state.StatusText);
        Assert.Equal("Start Recording", state.ToggleRecordingLabel);
        Assert.False(state.CanToggleRecording);
        Assert.False(state.CanQuickAddDictionary);
        Assert.False(state.CanOpenHistory);
        Assert.False(state.CanUseQuickSettings);
    }
}
