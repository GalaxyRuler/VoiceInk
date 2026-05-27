using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Models;

public sealed record LocalWhisperModelRemovalResult(
    LocalWhisperModel[] ImportedModels,
    string ModelPath,
    string? FilePathToDelete,
    bool Removed);

public static class LocalWhisperModelService
{
    private const string ModelExtension = ".bin";
    private const long MinimumPlausibleModelBytes = 1024 * 1024;
    private const int GgmlMagicByteCount = 4;
    private const uint GgmlMagic = 0x67676d6c;

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

    public static LocalWhisperModelHealth CheckPathHealth(
        string? modelPath,
        Func<string, bool> fileExists,
        Func<string, long> fileLength,
        Func<string, byte[]>? readHeader = null)
    {
        var trimmedPath = modelPath?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedPath))
        {
            return new LocalWhisperModelHealth(
                LocalWhisperModelHealthStatus.NotSelected,
                "No local model selected.",
                CanUse: false);
        }

        if (!string.Equals(System.IO.Path.GetExtension(trimmedPath), ModelExtension, StringComparison.OrdinalIgnoreCase))
        {
            return new LocalWhisperModelHealth(
                LocalWhisperModelHealthStatus.InvalidExtension,
                "Choose a whisper.cpp .bin model file.",
                CanUse: false);
        }

        if (!fileExists(trimmedPath))
        {
            return new LocalWhisperModelHealth(
                LocalWhisperModelHealthStatus.Missing,
                $"Model file not found: {trimmedPath}",
                CanUse: false);
        }

        var length = fileLength(trimmedPath);
        if (length <= 0)
        {
            return new LocalWhisperModelHealth(
                LocalWhisperModelHealthStatus.Empty,
                $"Model file is empty: {trimmedPath}",
                CanUse: false);
        }

        if (length < MinimumPlausibleModelBytes)
        {
            return new LocalWhisperModelHealth(
                LocalWhisperModelHealthStatus.SuspiciouslySmall,
                $"Model file looks too small for a whisper.cpp model: {trimmedPath}",
                CanUse: false);
        }

        if (readHeader is not null && !HasGgmlMagicHeader(readHeader(trimmedPath)))
        {
            return new LocalWhisperModelHealth(
                LocalWhisperModelHealthStatus.InvalidHeader,
                $"Model file is not a whisper.cpp GGML model: {trimmedPath}",
                CanUse: false);
        }

        return new LocalWhisperModelHealth(
            LocalWhisperModelHealthStatus.Ready,
            $"Default Model: {System.IO.Path.GetFileNameWithoutExtension(trimmedPath)}",
            CanUse: true);
    }

    public static LocalWhisperModel[] RemoveUnavailableImportedModels(
        IEnumerable<LocalWhisperModel> importedModels,
        Func<string, bool> fileExists,
        Func<string, long> fileLength,
        out int removedCount)
    {
        var remaining = importedModels
            .Where(model => CheckPathHealth(model.Path, fileExists, fileLength).CanUse)
            .ToArray();
        removedCount = importedModels.Count() - remaining.Length;
        return remaining;
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

    public static LocalWhisperModelRemovalResult RemoveModel(
        IEnumerable<LocalWhisperModel> existing,
        string pathToRemove,
        string currentModelPath,
        string appModelsDirectory)
    {
        var models = existing.ToList();
        var trimmedPath = pathToRemove.Trim();
        var index = models.FindIndex(model =>
            string.Equals(model.Path, trimmedPath, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            if (string.Equals(currentModelPath.Trim(), trimmedPath, StringComparison.OrdinalIgnoreCase))
            {
                return new LocalWhisperModelRemovalResult(
                    [.. models],
                    string.Empty,
                    AppOwnsModelPath(trimmedPath, appModelsDirectory) ? trimmedPath : null,
                    Removed: true);
            }

            return new LocalWhisperModelRemovalResult(
                [.. models],
                currentModelPath,
                FilePathToDelete: null,
                Removed: false);
        }

        var removedModel = models[index];
        models.RemoveAt(index);
        var nextModelPath = string.Equals(currentModelPath.Trim(), removedModel.Path, StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : currentModelPath;

        return new LocalWhisperModelRemovalResult(
            [.. models],
            nextModelPath,
            AppOwnsModelPath(removedModel.Path, appModelsDirectory) ? removedModel.Path : null,
            Removed: true);
    }

    private static bool AppOwnsModelPath(string modelPath, string appModelsDirectory)
    {
        if (string.IsNullOrWhiteSpace(modelPath) || string.IsNullOrWhiteSpace(appModelsDirectory))
        {
            return false;
        }

        try
        {
            var normalizedModelPath = System.IO.Path.GetFullPath(modelPath);
            var normalizedModelsDirectory = System.IO.Path.GetFullPath(appModelsDirectory)
                .TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            return normalizedModelPath.StartsWith(
                    normalizedModelsDirectory + System.IO.Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase)
                && string.Equals(
                    System.IO.Path.GetExtension(normalizedModelPath),
                    ModelExtension,
                    StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool HasGgmlMagicHeader(byte[] header) =>
        header.Length >= GgmlMagicByteCount
        && System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(0, GgmlMagicByteCount)) == GgmlMagic;
}
