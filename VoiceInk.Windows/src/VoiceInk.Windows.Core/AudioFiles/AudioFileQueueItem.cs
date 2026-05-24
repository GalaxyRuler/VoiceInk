using VoiceInk.Windows.Core.History;

namespace VoiceInk.Windows.Core.AudioFiles;

public sealed record AudioFileQueueItem(
    Guid Id,
    string FilePath,
    string FileName,
    AudioFileQueueStatus Status,
    string StatusDetail,
    TranscriptionHistoryItem? HistoryItem = null,
    string? ErrorMessage = null)
{
    public bool IsTerminal => Status is AudioFileQueueStatus.Completed or AudioFileQueueStatus.Failed;

    public AudioFileQueueItem MarkPending() =>
        this with
        {
            Status = AudioFileQueueStatus.Pending,
            StatusDetail = "Waiting",
            HistoryItem = null,
            ErrorMessage = null
        };

    public AudioFileQueueItem MarkProcessing(string statusDetail) =>
        this with
        {
            Status = AudioFileQueueStatus.Processing,
            StatusDetail = string.IsNullOrWhiteSpace(statusDetail) ? "Processing" : statusDetail,
            ErrorMessage = null
        };

    public AudioFileQueueItem MarkCompleted(TranscriptionHistoryItem historyItem) =>
        this with
        {
            Status = AudioFileQueueStatus.Completed,
            StatusDetail = "Completed",
            HistoryItem = historyItem,
            ErrorMessage = null
        };

    public AudioFileQueueItem MarkFailed(string message) =>
        this with
        {
            Status = AudioFileQueueStatus.Failed,
            StatusDetail = "Failed",
            ErrorMessage = string.IsNullOrWhiteSpace(message) ? "Transcription failed" : message
        };
}
