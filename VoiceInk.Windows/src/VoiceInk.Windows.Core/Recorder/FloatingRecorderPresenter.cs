using VoiceInk.Windows.Core.Dictation;

namespace VoiceInk.Windows.Core.Recorder;

public static class FloatingRecorderPresenter
{
    public static FloatingRecorderViewState FromState(
        DictationState state,
        TimeSpan elapsed,
        string? status,
        bool isOperationActive)
    {
        return state switch
        {
            DictationState.Recording => new(
                true,
                "Recording",
                isOperationActive && !string.IsNullOrWhiteSpace(status) ? status : "Listening",
                FormatElapsed(elapsed),
                CanStop: !isOperationActive,
                CanCancel: !isOperationActive,
                ShowPulse: true),
            DictationState.Transcribing => new(
                true,
                "Transcribing",
                "Processing speech",
                FormatElapsed(elapsed),
                CanStop: false,
                CanCancel: false,
                ShowPulse: true),
            DictationState.Inserting => new(
                true,
                "Inserting",
                "Inserting text",
                FormatElapsed(elapsed),
                CanStop: false,
                CanCancel: false,
                ShowPulse: true),
            DictationState.Error => new(
                isOperationActive,
                "Error",
                status ?? "Recording failed",
                FormatElapsed(elapsed),
                CanStop: false,
                CanCancel: false,
                ShowPulse: false),
            _ when isOperationActive => new(
                true,
                string.IsNullOrWhiteSpace(status) ? "Working" : status,
                "VoiceInk is busy",
                FormatElapsed(elapsed),
                CanStop: false,
                CanCancel: false,
                ShowPulse: true),
            _ => new(
                false,
                "Idle",
                status ?? "Idle",
                FormatElapsed(TimeSpan.Zero),
                CanStop: false,
                CanCancel: false,
                ShowPulse: false)
        };
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        var safeElapsed = elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
        var totalMinutes = (int)safeElapsed.TotalMinutes;
        return $"{totalMinutes:00}:{safeElapsed.Seconds:00}";
    }
}
