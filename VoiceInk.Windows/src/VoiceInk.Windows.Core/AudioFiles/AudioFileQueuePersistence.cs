namespace VoiceInk.Windows.Core.AudioFiles;

public static class AudioFileQueuePersistence
{
    public static AudioFileQueueSnapshot CreateSnapshot(IReadOnlyList<AudioFileQueueItem> items) =>
        new(
            items
                .Where(item => item.Status != AudioFileQueueStatus.Completed)
                .Select(item => new AudioFileQueueSnapshotItem(
                    item.Id,
                    item.FilePath,
                    item.Status,
                    item.StatusDetail,
                    item.ErrorMessage))
                .ToArray());

    public static IReadOnlyList<AudioFileQueueItem> Restore(AudioFileQueueSnapshot? snapshot)
    {
        if (snapshot is null || snapshot.Items.Count == 0)
        {
            return [];
        }

        var restored = new List<AudioFileQueueItem>();
        var activePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var snapshotItem in snapshot.Items)
        {
            var normalizedPath = AudioFileQueueService.NormalizePath(snapshotItem.FilePath);
            if (normalizedPath is null
                || !File.Exists(normalizedPath)
                || !AudioFileQueueService.IsSupportedFilePath(normalizedPath)
                || !activePaths.Add(normalizedPath))
            {
                continue;
            }

            var baseItem = new AudioFileQueueItem(
                snapshotItem.Id,
                normalizedPath,
                Path.GetFileName(normalizedPath),
                AudioFileQueueStatus.Pending,
                "Waiting");

            restored.Add(snapshotItem.Status == AudioFileQueueStatus.Failed
                ? baseItem.MarkFailed(snapshotItem.ErrorMessage ?? snapshotItem.StatusDetail)
                : baseItem);
        }

        return restored;
    }
}
