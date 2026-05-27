using VoiceInk.Windows.Core.Dictation;

namespace VoiceInk.Windows.Core.Shell;

public static class TrayShellPresenter
{
    public static TrayShellState FromState(
        bool settingsLoaded,
        DictationState dictationState,
        bool operationActive,
        string? statusOverride)
    {
        var controllerBusy = dictationState is DictationState.Transcribing or DictationState.Inserting;
        var canUseOperationalCommands = settingsLoaded && !operationActive && !controllerBusy;
        var canToggleRecording = canUseOperationalCommands
            && dictationState is not DictationState.Transcribing and not DictationState.Inserting;

        return new TrayShellState(
            StatusText(settingsLoaded, dictationState, statusOverride),
            ToggleRecordingLabel(dictationState),
            canToggleRecording,
            CanQuickAddDictionary: canUseOperationalCommands && dictationState == DictationState.Idle,
            CanOpenHistory: canUseOperationalCommands,
            CanUseQuickSettings: canUseOperationalCommands && dictationState == DictationState.Idle,
            VisibilityGuidance: "If the VoiceInk notification-area icon is hidden, open Windows taskbar corner overflow and pin it; the main window can always be opened from the tray menu.",
            VisibilityMenuText: "Taskbar settings: Other system tray icons");
    }

    private static string StatusText(
        bool settingsLoaded,
        DictationState dictationState,
        string? statusOverride)
    {
        if (!settingsLoaded)
        {
            return "Loading settings";
        }

        return string.IsNullOrWhiteSpace(statusOverride)
            ? StateToStatusText(dictationState)
            : statusOverride;
    }

    private static string ToggleRecordingLabel(DictationState dictationState) =>
        dictationState == DictationState.Recording
            ? "Stop Recording"
            : "Start Recording";

    private static string StateToStatusText(DictationState state) =>
        state switch
        {
            DictationState.Idle => "Idle",
            DictationState.Recording => "Recording",
            DictationState.Transcribing => "Transcribing",
            DictationState.Inserting => "Inserting",
            DictationState.Error => "Error",
            _ => state.ToString()
        };
}
