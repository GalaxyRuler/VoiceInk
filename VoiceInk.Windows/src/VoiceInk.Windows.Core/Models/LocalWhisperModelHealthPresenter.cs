namespace VoiceInk.Windows.Core.Models;

public sealed record LocalWhisperModelHealthPresentation(
    string Title,
    string Guidance,
    string ActionLabel,
    bool CanWarmup);

public static class LocalWhisperModelHealthPresenter
{
    public static LocalWhisperModelHealthPresentation Present(LocalWhisperModelHealth health) =>
        health.Status switch
        {
            LocalWhisperModelHealthStatus.NotSelected => new(
                "Choose a local Whisper model",
                "Download a recommended GGML model or import an existing whisper.cpp .bin file.",
                "Download or Import",
                CanWarmup: false),
            LocalWhisperModelHealthStatus.InvalidExtension => new(
                "Model file type is not supported",
                "Choose a whisper.cpp GGML .bin model file.",
                "Import .bin Model",
                CanWarmup: false),
            LocalWhisperModelHealthStatus.Missing => new(
                "Model file is missing",
                "Re-import the model from its new location or download a fresh GGML model.",
                "Repair Model Path",
                CanWarmup: false),
            LocalWhisperModelHealthStatus.Empty => new(
                "Model file is empty",
                "Delete this broken file and download or import a complete GGML model.",
                "Replace Model",
                CanWarmup: false),
            LocalWhisperModelHealthStatus.SuspiciouslySmall => new(
                "Model file looks incomplete",
                "Replace it with a complete whisper.cpp GGML model before recording.",
                "Replace Model",
                CanWarmup: false),
            LocalWhisperModelHealthStatus.Ready => new(
                "Local model is ready",
                "You can warm up this model to reduce first transcription latency.",
                "Warm Up Model",
                CanWarmup: true),
            _ => new(
                "Check local model",
                health.Message,
                "Review Model",
                health.CanUse)
        };
}
