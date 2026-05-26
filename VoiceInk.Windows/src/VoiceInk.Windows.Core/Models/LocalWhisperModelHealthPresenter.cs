namespace VoiceInk.Windows.Core.Models;

public sealed record LocalWhisperModelHealthPresentation(
    string Title,
    string Guidance,
    string ActionLabel,
    bool CanWarmup,
    LocalWhisperModelRepairAction RepairAction);

public enum LocalWhisperModelRepairAction
{
    ImportReplacement,
    Warmup
}

public static class LocalWhisperModelHealthPresenter
{
    public static LocalWhisperModelHealthPresentation Present(LocalWhisperModelHealth health) =>
        health.Status switch
        {
            LocalWhisperModelHealthStatus.NotSelected => new(
                "Choose a local Whisper model",
                "Download a recommended GGML model or import an existing whisper.cpp .bin file.",
                "Import .bin Model",
                CanWarmup: false,
                LocalWhisperModelRepairAction.ImportReplacement),
            LocalWhisperModelHealthStatus.InvalidExtension => new(
                "Model file type is not supported",
                "Choose a whisper.cpp GGML .bin model file.",
                "Import .bin Model",
                CanWarmup: false,
                LocalWhisperModelRepairAction.ImportReplacement),
            LocalWhisperModelHealthStatus.Missing => new(
                "Model file is missing",
                "Re-import the model from its new location or download a fresh GGML model.",
                "Repair Model Path",
                CanWarmup: false,
                LocalWhisperModelRepairAction.ImportReplacement),
            LocalWhisperModelHealthStatus.Empty => new(
                "Model file is empty",
                "Delete this broken file and download or import a complete GGML model.",
                "Replace Model",
                CanWarmup: false,
                LocalWhisperModelRepairAction.ImportReplacement),
            LocalWhisperModelHealthStatus.SuspiciouslySmall => new(
                "Model file looks incomplete",
                "Replace it with a complete whisper.cpp GGML model before recording.",
                "Replace Model",
                CanWarmup: false,
                LocalWhisperModelRepairAction.ImportReplacement),
            LocalWhisperModelHealthStatus.Ready => new(
                "Local model is ready",
                "You can warm up this model to reduce first transcription latency.",
                "Warm Up Model",
                CanWarmup: true,
                LocalWhisperModelRepairAction.Warmup),
            _ => new(
                "Check local model",
                health.Message,
                "Review Model",
                health.CanUse,
                health.CanUse ? LocalWhisperModelRepairAction.Warmup : LocalWhisperModelRepairAction.ImportReplacement)
        };
}
