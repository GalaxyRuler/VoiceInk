namespace VoiceInk.Windows.Core.Dictionary;

public sealed record DictionaryPagePresentation(
    string HeroTitle,
    string HeroDescription,
    string VocabularySectionTitle,
    string VocabularySectionDescription,
    string VocabularyCountLabel,
    string VocabularyEmptyText,
    IReadOnlyList<DictionaryVocabularyRow> VocabularyRows,
    string ReplacementSectionTitle,
    string ReplacementSectionDescription,
    string ReplacementCountLabel,
    string ReplacementEmptyText,
    IReadOnlyList<DictionaryReplacementRow> ReplacementRows);

public sealed record DictionaryVocabularyRow(
    Guid Id,
    string Word,
    string DisplayText);

public sealed record DictionaryReplacementRow(
    Guid Id,
    string OriginalText,
    string ReplacementText,
    bool IsEnabled,
    string DisplayText);

public static class DictionaryPagePresenter
{
    public static DictionaryPagePresentation Present(
        IEnumerable<VocabularyWord> vocabulary,
        IEnumerable<WordReplacement> replacements)
    {
        ArgumentNullException.ThrowIfNull(vocabulary);
        ArgumentNullException.ThrowIfNull(replacements);

        var vocabularyRows = vocabulary
            .Select(item => new DictionaryVocabularyRow(item.Id, item.Word, item.Word))
            .ToArray();
        var replacementRows = replacements
            .Select(item => new DictionaryReplacementRow(
                item.Id,
                item.OriginalText,
                item.ReplacementText,
                item.IsEnabled,
                ReplacementDisplayText(item)))
            .ToArray();

        return new DictionaryPagePresentation(
            "Dictionary Settings",
            "Enhance VoiceInk's transcription accuracy by teaching it your vocabulary",
            "Vocabulary",
            "Add words to help VoiceInk recognize them properly. (Requires AI enhancement)",
            $"Vocabulary Words ({vocabularyRows.Length})",
            vocabularyRows.Length == 0 ? "Add words to help VoiceInk recognize them properly." : string.Empty,
            vocabularyRows,
            "Word Replacements",
            "Automatically replace specific words/phrases with custom formatted text",
            $"Word Replacements ({replacementRows.Length})",
            replacementRows.Length == 0
                ? "Define word replacements to automatically replace specific words or phrases."
                : string.Empty,
            replacementRows);
    }

    private static string ReplacementDisplayText(WordReplacement replacement)
    {
        var display = $"{replacement.OriginalText} -> {replacement.ReplacementText}";
        return replacement.IsEnabled ? display : $"{display} (disabled)";
    }
}
