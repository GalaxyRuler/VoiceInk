namespace VoiceInk.Windows.Core.Models;

public sealed record ModelLibraryOverviewPresentation(
    string Title,
    string Summary,
    string DefaultModelLabel,
    string CleanupHint,
    IReadOnlyList<ModelLibraryStorageGuidanceRow> StorageGuidanceRows,
    IReadOnlyList<ModelLibraryActionRow> ActionRows);

public sealed record ModelLibraryStorageGuidanceRow(
    string Title,
    string Value,
    string Detail,
    string StatusBadge);

public sealed record ModelLibraryActionRow(
    string Title,
    string Value,
    string Detail,
    string StatusBadge);

public static class ModelLibraryOverviewPresenter
{
    public static ModelLibraryOverviewPresentation Present(
        IEnumerable<WhisperModelCatalogItem> catalogItems,
        IEnumerable<LocalWhisperModel> localModels,
        string selectedModelPath,
        int unavailableImportedModelCount)
    {
        var items = catalogItems.ToArray();
        var localModelList = localModels.ToArray();
        var catalogModelNames = WhisperModelCatalog.All
            .Select(model => model.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var customModelCount = localModelList.Count(model => !catalogModelNames.Contains(model.DisplayName));
        var availableCount = Math.Max(0, items.Count(item => item.IsDownloaded) - unavailableImportedModelCount);
        var recommendedCount = items.Count(item => item.IsRecommended);
        var defaultModel = items.FirstOrDefault(item => item.IsDefault);
        var customDefaultModel = localModelList.FirstOrDefault(model =>
            string.Equals(model.Path, selectedModelPath.Trim(), StringComparison.OrdinalIgnoreCase));
        var customDefaultDisplayName = customDefaultModel?.DisplayName ?? string.Empty;
        var defaultDisplayName = defaultModel?.DisplayName ?? customDefaultDisplayName;

        return new ModelLibraryOverviewPresentation(
            "Local Whisper Library",
            $"{availableCount} of {items.Length} catalog models available on this device. {customModelCount} {ImportedModelText(customModelCount)} ready. {recommendedCount} recommended starter models are highlighted.",
            DefaultModelLabel(defaultModel, customDefaultModel),
            CleanupHint(availableCount + customModelCount, unavailableImportedModelCount),
            StorageGuidanceRows(),
            ActionRows(
                availableCount,
                items.Length,
                customModelCount,
                defaultDisplayName,
                unavailableImportedModelCount));
    }

    private static IReadOnlyList<ModelLibraryStorageGuidanceRow> StorageGuidanceRows() =>
    [
        new(
            "Downloaded Models",
            "App-local",
            "Catalog downloads are stored under this Windows profile's VoiceInk Models folder.",
            "Local"),
        new(
            "Imported Models",
            "References",
            "Imported .bin files stay where you selected them; VoiceInk stores only the local reference.",
            "User-owned"),
        new(
            "Settings Backup",
            "No binaries",
            "Backups include model references but do not copy large GGML model files.",
            "Paths only"),
        new(
            "Warmup",
            "After selection",
            "Prewarm loads the selected local model when available to reduce first-use delay.",
            "Optional"),
        new(
            "Expected Filename",
            "ggml-*.bin",
            "Whisper.cpp GGML models commonly use filenames such as ggml-base.en.bin.",
            "Check import"),
        new(
            "Download Source",
            "whisper.cpp GGML",
            "Catalog downloads use open-source whisper.cpp GGML model files hosted on Hugging Face.",
            "Open source"),
        new(
            "Compatibility Check",
            "GGML header",
            "VoiceInk validates selected .bin files before warmup and rejects files that do not look like whisper.cpp GGML models.",
            "Preflight")
    ];

    private static IReadOnlyList<ModelLibraryActionRow> ActionRows(
        int availableCatalogCount,
        int totalCatalogCount,
        int customModelCount,
        string defaultDisplayName,
        int unavailableImportedModelCount) =>
    [
        new(
            "Download Models",
            $"{availableCatalogCount} of {totalCatalogCount} available",
            availableCatalogCount > 0
                ? "Downloaded catalog models are ready for private local transcription."
                : "Download a recommended GGML model for private local transcription.",
            availableCatalogCount > 0 ? "Available" : "Needed"),
        new(
            "Imported Models",
            $"{customModelCount} ready",
            customModelCount > 0
                ? "Imported whisper.cpp .bin models stay referenced locally."
                : "Import an existing whisper.cpp .bin model from disk.",
            customModelCount > 0 ? "Ready" : "Optional"),
        new(
            "Default Model",
            string.IsNullOrWhiteSpace(defaultDisplayName) ? "Not selected" : defaultDisplayName,
            string.IsNullOrWhiteSpace(defaultDisplayName)
                ? "Set a downloaded or imported model as the default before recording."
                : "This model is used for local dictation unless Power Mode overrides it.",
            string.IsNullOrWhiteSpace(defaultDisplayName) ? "Required" : "Ready"),
        RepairWarmupRow(defaultDisplayName, unavailableImportedModelCount)
    ];

    private static ModelLibraryActionRow RepairWarmupRow(
        string defaultDisplayName,
        int unavailableImportedModelCount)
    {
        if (unavailableImportedModelCount > 0)
        {
            return new(
                "Repair & Warmup",
                $"{unavailableImportedModelCount} stale references",
                "Remove unavailable imported references or choose a replacement model path.",
                "Repair");
        }

        if (!string.IsNullOrWhiteSpace(defaultDisplayName))
        {
            return new(
                "Repair & Warmup",
                "Warmup ready",
                "Warm up the selected model to reduce first-use delay.",
                "Warmup");
        }

        return new(
            "Repair & Warmup",
            "After model select",
            "Warmup becomes available once the default model path is usable.",
            "Waiting");
    }

    private static string ImportedModelText(int count) =>
        count == 1 ? "imported custom model" : "imported custom models";

    private static string DefaultModelLabel(
        WhisperModelCatalogItem? defaultModel,
        LocalWhisperModel? customDefaultModel)
    {
        if (defaultModel is not null)
        {
            return $"Default local model: {defaultModel.DisplayName}.";
        }

        if (customDefaultModel is not null)
        {
            return $"Default local model: {customDefaultModel.DisplayName}.";
        }

        return "No default local model selected.";
    }

    private static string CleanupHint(int availableCount, int unavailableImportedModelCount)
    {
        if (unavailableImportedModelCount > 0)
        {
            return $"{unavailableImportedModelCount} unavailable imported model references can be removed from settings without deleting model files.";
        }

        if (availableCount == 0)
        {
            return "Import a whisper.cpp .bin file or download a recommended model to start private local transcription.";
        }

        return "Model library is ready. Downloaded and imported model references stay local to this Windows profile.";
    }
}
