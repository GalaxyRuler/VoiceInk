namespace VoiceInk.Windows.Core.Models;

public sealed record LocalWhisperModelHealthPresentation(
    string Title,
    string Guidance,
    string ActionLabel,
    bool CanWarmup,
    LocalWhisperModelRepairAction RepairAction,
    IReadOnlyList<LocalWhisperModelHealthGuidanceRow> GuidanceRows);

public sealed record LocalWhisperModelHealthGuidanceRow(
    string Title,
    string Value,
    string Detail,
    string StatusBadge);

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
                LocalWhisperModelRepairAction.ImportReplacement,
                GuidanceRows(health)),
            LocalWhisperModelHealthStatus.InvalidExtension => new(
                "Model file type is not supported",
                "Choose a whisper.cpp GGML .bin model file.",
                "Import .bin Model",
                CanWarmup: false,
                LocalWhisperModelRepairAction.ImportReplacement,
                GuidanceRows(health)),
            LocalWhisperModelHealthStatus.Missing => new(
                "Model file is missing",
                "Re-import the model from its new location or download a fresh GGML model.",
                "Repair Model Path",
                CanWarmup: false,
                LocalWhisperModelRepairAction.ImportReplacement,
                GuidanceRows(health)),
            LocalWhisperModelHealthStatus.Empty => new(
                "Model file is empty",
                "Delete this broken file and download or import a complete GGML model.",
                "Replace Model",
                CanWarmup: false,
                LocalWhisperModelRepairAction.ImportReplacement,
                GuidanceRows(health)),
            LocalWhisperModelHealthStatus.SuspiciouslySmall => new(
                "Model file looks incomplete",
                "Replace it with a complete whisper.cpp GGML model before recording.",
                "Replace Model",
                CanWarmup: false,
                LocalWhisperModelRepairAction.ImportReplacement,
                GuidanceRows(health)),
            LocalWhisperModelHealthStatus.InvalidHeader => new(
                "Model file is not GGML",
                "Replace it with a whisper.cpp GGML .bin model before recording.",
                "Replace Model",
                CanWarmup: false,
                LocalWhisperModelRepairAction.ImportReplacement,
                GuidanceRows(health)),
            LocalWhisperModelHealthStatus.Ready => new(
                "Local model is ready",
                "You can warm up this model to reduce first transcription latency.",
                "Warm Up Model",
                CanWarmup: true,
                LocalWhisperModelRepairAction.Warmup,
                GuidanceRows(health)),
            _ => new(
                "Check local model",
                health.Message,
                "Review Model",
                health.CanUse,
                health.CanUse ? LocalWhisperModelRepairAction.Warmup : LocalWhisperModelRepairAction.ImportReplacement,
                GuidanceRows(health))
        };

    private static IReadOnlyList<LocalWhisperModelHealthGuidanceRow> GuidanceRows(LocalWhisperModelHealth health) =>
        health.CanUse
            ? ReadyRows()
            : RepairRows();

    private static IReadOnlyList<LocalWhisperModelHealthGuidanceRow> ReadyRows() =>
    [
        new(
            "Model File",
            "Ready",
            "The selected whisper.cpp .bin file can be used for local transcription.",
            "Usable"),
        new(
            "Filename",
            "ggml-*.bin",
            "Whisper.cpp models usually use names like ggml-base.en.bin.",
            "Expected"),
        new(
            "Warmup",
            "Available",
            "Prewarm can load this model before the first recording to reduce startup latency.",
            "Optional"),
        new(
            "Storage",
            "Local path",
            "VoiceInk keeps the model on this Windows profile and does not upload it for local transcription.",
            "Local")
    ];

    private static IReadOnlyList<LocalWhisperModelHealthGuidanceRow> RepairRows() =>
    [
        new(
            "Model File",
            "Unavailable",
            "VoiceInk cannot use this model until the path points to a complete .bin file.",
            "Repair"),
        new(
            "Filename",
            "ggml-*.bin",
            "Import a whisper.cpp GGML file such as ggml-base.en.bin.",
            "Expected"),
        new(
            "Repair Picker",
            "User selected",
            "Repair validates the .bin file you choose and does not scan folders automatically.",
            "Explicit"),
        new(
            "Warmup",
            "Blocked",
            "Warmup is disabled until the selected model path is usable.",
            "Waiting"),
        new(
            "Storage",
            "User-owned path",
            "Repairing the reference does not delete model files from disk.",
            "Safe repair")
    ];
}
