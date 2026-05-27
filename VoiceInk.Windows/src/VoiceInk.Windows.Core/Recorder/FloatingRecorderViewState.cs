namespace VoiceInk.Windows.Core.Recorder;

public sealed record FloatingRecorderViewState(
    bool IsVisible,
    string Title,
    string Detail,
    string Elapsed,
    bool CanStop,
    bool CanCancel,
    bool ShowPulse,
    double InputLevel = 0,
    string LiveTranscript = "",
    bool HasLiveTranscript = false,
    string LiveTranscriptDetail = "",
    string RecorderStyle = RecorderStyleSettings.Mini,
    string FooterHint = "Shortcut, tray, or recorder controls")
{
    public string AccessibleName =>
        string.Join(
            ", ",
            new[]
            {
                "VoiceInk recorder",
                Title,
                Detail,
                Elapsed,
                FooterHint,
                HasLiveTranscript ? "Live preview available" : null
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
}
