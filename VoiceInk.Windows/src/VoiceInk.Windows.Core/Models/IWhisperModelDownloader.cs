namespace VoiceInk.Windows.Core.Models;

public interface IWhisperModelDownloader
{
    Task<LocalWhisperModel> DownloadAsync(
        WhisperModelCatalogEntry model,
        string modelsDirectory,
        IProgress<WhisperModelDownloadProgress>? progress,
        CancellationToken cancellationToken);
}
