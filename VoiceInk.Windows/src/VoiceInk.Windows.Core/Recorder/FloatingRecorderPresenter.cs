using VoiceInk.Windows.Core.Dictation;

namespace VoiceInk.Windows.Core.Recorder;

public static class FloatingRecorderPresenter
{
    public static FloatingRecorderViewState FromState(
        DictationState state,
        TimeSpan elapsed,
        string? status,
        bool isOperationActive,
        double inputLevel = 0,
        string? partialTranscript = null,
        bool showLiveTranscriptPreview = false,
        string? recorderStyle = null)
    {
        var safeInputLevel = double.IsFinite(inputLevel) ? Math.Clamp(inputLevel, 0, 1) : 0;
        var liveTranscript = LiveTranscriptForState(state, partialTranscript, showLiveTranscriptPreview);
        var hasLiveTranscript = liveTranscript.Length > 0;
        var liveTranscriptDetail = hasLiveTranscript
            ? "Live preview is interim and stays in the recorder until final insertion."
            : string.Empty;
        var normalizedRecorderStyle = RecorderStyleSettings.Normalize(recorderStyle);
        return state switch
        {
            DictationState.Recording => new(
                true,
                "Recording",
                isOperationActive && !string.IsNullOrWhiteSpace(status) ? status : "Listening",
                FormatElapsed(elapsed),
                CanStop: !isOperationActive,
                CanCancel: !isOperationActive,
                ShowPulse: true,
                InputLevel: safeInputLevel,
                LiveTranscript: liveTranscript,
                HasLiveTranscript: hasLiveTranscript,
                LiveTranscriptDetail: liveTranscriptDetail,
                RecorderStyle: normalizedRecorderStyle),
            DictationState.Transcribing => new(
                true,
                "Transcribing",
                "Processing speech",
                FormatElapsed(elapsed),
                CanStop: false,
                CanCancel: false,
                ShowPulse: true,
                RecorderStyle: normalizedRecorderStyle),
            DictationState.Inserting => new(
                true,
                "Inserting",
                "Inserting text",
                FormatElapsed(elapsed),
                CanStop: false,
                CanCancel: false,
                ShowPulse: true,
                RecorderStyle: normalizedRecorderStyle),
            DictationState.Error => new(
                isOperationActive,
                "Error",
                status ?? "Recording failed",
                FormatElapsed(elapsed),
                CanStop: false,
                CanCancel: false,
                ShowPulse: false,
                RecorderStyle: normalizedRecorderStyle),
            _ when isOperationActive => new(
                true,
                string.IsNullOrWhiteSpace(status) ? "Working" : status,
                "VoiceInk is busy",
                FormatElapsed(elapsed),
                CanStop: false,
                CanCancel: false,
                ShowPulse: true,
                RecorderStyle: normalizedRecorderStyle),
            _ => new(
                false,
                "Idle",
                status ?? "Idle",
                FormatElapsed(TimeSpan.Zero),
                CanStop: false,
                CanCancel: false,
                ShowPulse: false,
                RecorderStyle: normalizedRecorderStyle)
        };
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        var safeElapsed = elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
        var totalMinutes = (int)safeElapsed.TotalMinutes;
        return $"{totalMinutes:00}:{safeElapsed.Seconds:00}";
    }

    private static string LiveTranscriptForState(
        DictationState state,
        string? partialTranscript,
        bool showLiveTranscriptPreview)
    {
        if (state != DictationState.Recording || !showLiveTranscriptPreview)
        {
            return string.Empty;
        }

        return partialTranscript?.Trim() ?? string.Empty;
    }
}
