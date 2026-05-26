namespace VoiceInk.Windows.Core.AudioFiles;

public sealed class AudioFileQueueService(Func<Guid>? idFactory = null)
{
    private static readonly HashSet<string> SupportedExtensionsSet = new(
        [
            ".wav",
            ".mp3",
            ".m4a",
            ".mp4",
            ".mov",
            ".aac",
            ".wma",
            ".wmv",
            ".avi",
            ".3gp",
            ".3g2",
            ".flac"
        ],
        StringComparer.OrdinalIgnoreCase);

    private readonly Func<Guid> idFactory = idFactory ?? Guid.NewGuid;

    public static IReadOnlyList<string> SupportedExtensions => SupportedExtensionsSet.Order(StringComparer.OrdinalIgnoreCase).ToArray();

    public AudioFileQueueUpdate AddFiles(
        IReadOnlyList<AudioFileQueueItem> currentItems,
        IEnumerable<string> filePaths)
    {
        var items = currentItems.ToList();
        var activePaths = items
            .Where(item => !item.IsTerminal)
            .Select(item => NormalizePath(item.FilePath))
            .Where(path => path is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var added = 0;
        var skipped = 0;

        foreach (var filePath in filePaths)
        {
            var normalizedPath = NormalizePath(filePath);
            if (normalizedPath is null
                || !File.Exists(normalizedPath)
                || !IsSupportedFilePath(normalizedPath)
                || !activePaths.Add(normalizedPath))
            {
                skipped++;
                continue;
            }

            items.Add(new AudioFileQueueItem(
                idFactory(),
                normalizedPath,
                Path.GetFileName(normalizedPath),
                AudioFileQueueStatus.Pending,
                "Waiting"));
            added++;
        }

        return new AudioFileQueueUpdate(items, AddedCount: added, SkippedCount: skipped);
    }

    public AudioFileQueueUpdate RemovePending(
        IReadOnlyList<AudioFileQueueItem> currentItems,
        Guid id)
    {
        var items = currentItems.ToList();
        var removed = items.RemoveAll(item => item.Id == id && item.Status == AudioFileQueueStatus.Pending);
        return new AudioFileQueueUpdate(items, RemovedCount: removed);
    }

    public AudioFileQueueUpdate RetryFailed(
        IReadOnlyList<AudioFileQueueItem> currentItems,
        Guid id)
    {
        var items = currentItems
            .Select(item => item.Id == id && item.Status == AudioFileQueueStatus.Failed
                ? item.MarkPending()
                : item)
            .ToArray();
        return new AudioFileQueueUpdate(items);
    }

    public AudioFileQueueUpdate Clear(IReadOnlyList<AudioFileQueueItem> currentItems) =>
        new([], RemovedCount: currentItems.Count);

    public static bool IsSupportedFilePath(string path) =>
        SupportedExtensionsSet.Contains(Path.GetExtension(path));

    public static string? NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        try
        {
            return Path.GetFullPath(path);
        }
        catch
        {
            return null;
        }
    }
}
