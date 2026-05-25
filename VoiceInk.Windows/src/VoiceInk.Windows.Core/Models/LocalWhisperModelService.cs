using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Models;

public static class LocalWhisperModelService
{
    private const string ModelExtension = ".bin";

    public static LocalWhisperModel[] Import(
        string path,
        IEnumerable<LocalWhisperModel> existing,
        DateTimeOffset importedAt,
        out string? error)
    {
        var trimmedPath = path.Trim();
        var existingModels = existing.ToArray();

        if (string.IsNullOrWhiteSpace(trimmedPath) ||
            !string.Equals(System.IO.Path.GetExtension(trimmedPath), ModelExtension, StringComparison.OrdinalIgnoreCase))
        {
            error = "Choose a whisper.cpp .bin model file.";
            return existingModels;
        }

        if (existingModels.Any(model => string.Equals(model.Path, trimmedPath, StringComparison.OrdinalIgnoreCase)))
        {
            error = "Model is already imported.";
            return existingModels;
        }

        var imported = new LocalWhisperModel(
            trimmedPath,
            System.IO.Path.GetFileNameWithoutExtension(trimmedPath),
            importedAt);

        error = null;
        return [.. existingModels, imported];
    }

    public static LocalWhisperModel[] BuildChoices(AppSettings settings)
    {
        var importedModels = settings.ImportedWhisperModels;
        var modelPath = settings.ModelPath.Trim();

        if (string.IsNullOrWhiteSpace(modelPath) ||
            importedModels.Any(model => string.Equals(model.Path, modelPath, StringComparison.OrdinalIgnoreCase)))
        {
            return importedModels;
        }

        var currentModel = new LocalWhisperModel(
            modelPath,
            System.IO.Path.GetFileNameWithoutExtension(modelPath),
            DateTimeOffset.MinValue);

        return [.. importedModels, currentModel];
    }

    public static WhisperModelCatalogItem[] BuildCatalogItems(
        IEnumerable<LocalWhisperModel> localModels,
        string currentModelPath)
    {
        var modelsByName = localModels
            .GroupBy(model => model.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var recommendedNames = WhisperModelCatalog.Recommended
            .Select(model => model.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return WhisperModelCatalog.All
            .Select(model =>
            {
                modelsByName.TryGetValue(model.Name, out var localModel);
                return new WhisperModelCatalogItem(
                    model,
                    localModel?.Path,
                    recommendedNames.Contains(model.Name),
                    localModel is not null
                        && string.Equals(
                            localModel.Path,
                            currentModelPath.Trim(),
                            StringComparison.OrdinalIgnoreCase));
            })
            .ToArray();
    }

    public static LocalWhisperModel[] AddOrReplaceCatalogModel(
        IEnumerable<LocalWhisperModel> existing,
        LocalWhisperModel downloadedModel)
    {
        var models = existing.ToList();
        var existingIndex = models.FindIndex(model =>
            string.Equals(model.DisplayName, downloadedModel.DisplayName, StringComparison.OrdinalIgnoreCase));

        if (existingIndex >= 0)
        {
            models[existingIndex] = downloadedModel;
            return [.. models];
        }

        models.Add(downloadedModel);
        return [.. models];
    }
}
