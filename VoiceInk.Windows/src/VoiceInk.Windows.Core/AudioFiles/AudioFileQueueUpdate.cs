namespace VoiceInk.Windows.Core.AudioFiles;

public sealed record AudioFileQueueUpdate(
    IReadOnlyList<AudioFileQueueItem> Items,
    int AddedCount = 0,
    int SkippedCount = 0,
    int RemovedCount = 0);
