namespace VoiceInk.Windows.Core.History;

public sealed record HistoryWindowCommandState(
    bool CanCopyOriginal,
    bool CanCopyFinal,
    bool CanCopyEnhanced,
    bool CanCopyAiRequest,
    bool CanRetry,
    bool CanReenhance,
    bool CanOpenAudio,
    bool CanDelete,
    string SelectionStatus);
