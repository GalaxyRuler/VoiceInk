namespace VoiceInk.Windows.Core.Dictionary;

public sealed record VocabularyWord(
    Guid Id,
    string Word,
    DateTimeOffset DateAdded);
