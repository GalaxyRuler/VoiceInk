namespace VoiceInk.Windows.Core.Shell;

public sealed record TrayShellState(
    string StatusText,
    string ToggleRecordingLabel,
    bool CanToggleRecording,
    bool CanQuickAddDictionary,
    bool CanOpenHistory,
    bool CanUseQuickSettings,
    string VisibilityGuidance,
    string VisibilityMenuText,
    string TooltipText,
    string StatusMenuText);
