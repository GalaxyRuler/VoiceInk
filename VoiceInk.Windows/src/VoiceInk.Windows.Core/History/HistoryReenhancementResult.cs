namespace VoiceInk.Windows.Core.History;

public sealed record HistoryReenhancementResult(
    bool Success,
    string Message,
    TranscriptionHistoryItem? Item = null);
