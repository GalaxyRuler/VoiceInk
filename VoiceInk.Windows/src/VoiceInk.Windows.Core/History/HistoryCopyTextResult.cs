namespace VoiceInk.Windows.Core.History;

public sealed record HistoryCopyTextResult(
    bool Success,
    string Message,
    string Text);
