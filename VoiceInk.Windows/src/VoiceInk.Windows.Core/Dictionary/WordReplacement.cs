namespace VoiceInk.Windows.Core.Dictionary;

public sealed record WordReplacement(
    Guid Id,
    string OriginalText,
    string ReplacementText,
    DateTimeOffset DateAdded,
    bool IsEnabled = true);
