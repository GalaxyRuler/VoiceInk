namespace VoiceInk.Windows.Core.History;

public static class HistoryWindowCommandPresenter
{
    public static HistoryWindowCommandState Present(TranscriptionHistoryItem? item)
    {
        if (item is null)
        {
            return new HistoryWindowCommandState(
                CanCopyOriginal: false,
                CanCopyFinal: false,
                CanCopyEnhanced: false,
                CanCopyAiRequest: false,
                CanRetry: false,
                CanReenhance: false,
                CanOpenAudio: false,
                CanDelete: false,
                SelectionStatus: "Select a transcription");
        }

        var completed = item.Status == TranscriptionHistoryStatus.Completed;
        return new HistoryWindowCommandState(
            CanCopyOriginal: HistoryCopyTextSelector.HasText(item, HistoryCopyTextKind.Original),
            CanCopyFinal: HistoryCopyTextSelector.HasText(item, HistoryCopyTextKind.Final),
            CanCopyEnhanced: HistoryCopyTextSelector.HasText(item, HistoryCopyTextKind.Enhanced),
            CanCopyAiRequest: HistoryCopyTextSelector.HasText(item, HistoryCopyTextKind.AiRequest),
            CanRetry: completed && !string.IsNullOrWhiteSpace(item.AudioFilePath),
            CanReenhance: completed,
            CanOpenAudio: !string.IsNullOrWhiteSpace(item.AudioFilePath),
            CanDelete: true,
            SelectionStatus: $"{item.Status} transcription selected");
    }
}
