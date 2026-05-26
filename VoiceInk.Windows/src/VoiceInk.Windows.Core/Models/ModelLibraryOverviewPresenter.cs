namespace VoiceInk.Windows.Core.Models;

public sealed record ModelLibraryOverviewPresentation(
    string Title,
    string Summary,
    string DefaultModelLabel,
    string CleanupHint);

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

        return new ModelLibraryOverviewPresentation(
            "Local Whisper Library",
            $"{availableCount} of {items.Length} catalog models available on this device. {customModelCount} {ImportedModelText(customModelCount)} ready. {recommendedCount} recommended starter models are highlighted.",
            DefaultModelLabel(defaultModel, customDefaultModel),
            CleanupHint(availableCount + customModelCount, unavailableImportedModelCount));
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
