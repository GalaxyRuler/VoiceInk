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
}
