namespace VoiceInk.Windows.Core.History;

public sealed record HistoryPageCursor(
    long CreatedAtUtcTicks,
    string Id);
