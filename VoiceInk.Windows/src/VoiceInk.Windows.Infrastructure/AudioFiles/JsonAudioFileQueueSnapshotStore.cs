using System.Text.Json;
using VoiceInk.Windows.Core.AudioFiles;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Infrastructure.AudioFiles;

public sealed class JsonAudioFileQueueSnapshotStore(string filePath) : IAudioFileQueueSnapshotStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public async Task<AudioFileQueueSnapshot> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return AudioFileQueueSnapshot.Empty;
        }

        try
        {
            await using var stream = File.OpenRead(filePath);
            var snapshot = await JsonSerializer.DeserializeAsync<AudioFileQueueSnapshot>(
                stream,
                JsonOptions,
                cancellationToken);
            return snapshot ?? AudioFileQueueSnapshot.Empty;
        }
        catch (JsonException)
        {
            return AudioFileQueueSnapshot.Empty;
        }
        catch (IOException)
        {
            return AudioFileQueueSnapshot.Empty;
        }
    }

    public async Task SaveAsync(AudioFileQueueSnapshot snapshot, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (snapshot.Items.Count == 0)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return;
        }

        var tempDirectory = string.IsNullOrWhiteSpace(directory) ? "." : directory;
        var tempPath = Path.Combine(
            tempDirectory,
            $".{Path.GetFileName(filePath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var stream = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                options: FileOptions.Asynchronous))
            {
                await JsonSerializer.SerializeAsync(stream, snapshot, JsonOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            File.Move(tempPath, filePath, overwrite: true);
        }
        catch
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            throw;
        }
    }
}
