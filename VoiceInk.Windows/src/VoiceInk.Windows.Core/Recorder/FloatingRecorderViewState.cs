namespace VoiceInk.Windows.Core.Recorder;

public sealed record FloatingRecorderViewState(
    bool IsVisible,
    string Title,
    string Detail,
    string Elapsed,
    bool CanStop,
    bool CanCancel,
    bool ShowPulse);
