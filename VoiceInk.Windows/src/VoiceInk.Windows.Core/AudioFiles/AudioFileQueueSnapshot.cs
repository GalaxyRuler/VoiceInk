namespace VoiceInk.Windows.Core.AudioFiles;

public sealed record AudioFileQueueSnapshot(IReadOnlyList<AudioFileQueueSnapshotItem> Items)
{
    public static AudioFileQueueSnapshot Empty { get; } = new([]);
}

public sealed record AudioFileQueueSnapshotItem(
    Guid Id,
    string FilePath,
    AudioFileQueueStatus Status,
    string StatusDetail,
    string? ErrorMessage);
