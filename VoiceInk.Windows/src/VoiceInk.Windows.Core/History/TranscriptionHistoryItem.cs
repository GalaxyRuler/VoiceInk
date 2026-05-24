namespace VoiceInk.Windows.Core.History;

public sealed record TranscriptionHistoryItem(
    Guid Id,
    DateTimeOffset CreatedAt,
    string Text,
    string ProviderName,
    TimeSpan AudioDuration,
    TimeSpan TranscriptionDuration);
