using VoiceInk.Windows.Core.Models;

namespace VoiceInk.Windows.Infrastructure.Models;

public sealed class HttpWhisperModelDownloader(HttpClient httpClient) : IWhisperModelDownloader
{
    private const int BufferSize = 128 * 1024;

    public async Task<LocalWhisperModel> DownloadAsync(
        WhisperModelCatalogEntry model,
        string modelsDirectory,
        IProgress<WhisperModelDownloadProgress>? progress,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        Directory.CreateDirectory(modelsDirectory);

        var destinationPath = Path.Combine(modelsDirectory, model.FileName);
        var temporaryPath = destinationPath + ".download";

        try
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }

            using var response = await httpClient.GetAsync(
                model.DownloadUri,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;
            await using var remoteStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var localStream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                BufferSize,
                useAsync: true);

            var buffer = new byte[BufferSize];
            long bytesReceived = 0;

            while (true)
            {
                var bytesRead = await remoteStream.ReadAsync(buffer, cancellationToken);
                if (bytesRead == 0)
                {
                    break;
                }

                await localStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                bytesReceived += bytesRead;
                progress?.Report(new WhisperModelDownloadProgress(model.Name, bytesReceived, totalBytes));
            }

            await localStream.FlushAsync(cancellationToken);
            localStream.Close();

            File.Move(temporaryPath, destinationPath, overwrite: true);
            progress?.Report(new WhisperModelDownloadProgress(model.Name, bytesReceived, totalBytes ?? bytesReceived));

            return new LocalWhisperModel(
                destinationPath,
                Path.GetFileNameWithoutExtension(destinationPath),
                DateTimeOffset.UtcNow);
        }
        catch
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }

            throw;
        }
    }
}
