namespace VoiceInk.Windows.Core.Models;

public sealed record LocalWhisperModelExplorerTarget(
    bool CanOpen,
    string FileName,
    string Arguments,
    string StatusMessage)
{
    public static LocalWhisperModelExplorerTarget Create(
        string? modelPath,
        Func<string, bool> fileExists)
    {
        var trimmedPath = modelPath?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedPath))
        {
            return new LocalWhisperModelExplorerTarget(
                CanOpen: false,
                string.Empty,
                string.Empty,
                "Select a local model to show in Explorer");
        }

        if (!fileExists(trimmedPath))
        {
            return new LocalWhisperModelExplorerTarget(
                CanOpen: false,
                string.Empty,
                string.Empty,
                $"Model file not found: {trimmedPath}");
        }

        return new LocalWhisperModelExplorerTarget(
            CanOpen: true,
            "explorer.exe",
            $"/select,\"{trimmedPath}\"",
            "Model file opened");
    }
}
