namespace VoiceInk.Windows.Core.History;

public sealed record HistoryPage(
    IReadOnlyList<TranscriptionHistoryItem> Items,
    HistoryPageCursor? NextCursor,
    bool HasMore);
