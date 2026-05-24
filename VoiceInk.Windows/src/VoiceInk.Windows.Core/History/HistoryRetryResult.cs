namespace VoiceInk.Windows.Core.History;

public sealed record HistoryRetryResult(
    bool Success,
    string Message,
    TranscriptionHistoryItem? Item = null);
