namespace VoiceInk.Windows.Core.Dictionary;

public sealed record DictionaryImportResult(
    int ImportedVocabularyCount,
    int ImportedReplacementCount,
    int SkippedDuplicateCount);
