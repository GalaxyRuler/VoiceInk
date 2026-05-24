namespace VoiceInk.Windows.Core.Dictionary;

public static class DictionarySortService
{
    public static IReadOnlyList<VocabularyWord> SortVocabulary(
        IEnumerable<VocabularyWord> words,
        string sortMode) =>
        sortMode == DictionarySortModes.VocabularyWordDescending
            ? words
                .OrderByDescending(word => word.Word, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(word => word.DateAdded)
                .ThenByDescending(word => word.Id)
                .ToArray()
            : words
                .OrderBy(word => word.Word, StringComparer.OrdinalIgnoreCase)
                .ThenBy(word => word.DateAdded)
                .ThenBy(word => word.Id)
                .ToArray();

    public static IReadOnlyList<WordReplacement> SortReplacements(
        IEnumerable<WordReplacement> replacements,
        string sortMode) =>
        sortMode switch
        {
            DictionarySortModes.ReplacementOriginalDescending => replacements
                .OrderByDescending(replacement => replacement.OriginalText, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(replacement => replacement.ReplacementText, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(replacement => replacement.DateAdded)
                .ThenByDescending(replacement => replacement.Id)
                .ToArray(),
            DictionarySortModes.ReplacementTextAscending => replacements
                .OrderBy(replacement => replacement.ReplacementText, StringComparer.OrdinalIgnoreCase)
                .ThenBy(replacement => replacement.OriginalText, StringComparer.OrdinalIgnoreCase)
                .ThenBy(replacement => replacement.DateAdded)
                .ThenBy(replacement => replacement.Id)
                .ToArray(),
            DictionarySortModes.ReplacementTextDescending => replacements
                .OrderByDescending(replacement => replacement.ReplacementText, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(replacement => replacement.OriginalText, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(replacement => replacement.DateAdded)
                .ThenByDescending(replacement => replacement.Id)
                .ToArray(),
            _ => replacements
                .OrderBy(replacement => replacement.OriginalText, StringComparer.OrdinalIgnoreCase)
                .ThenBy(replacement => replacement.ReplacementText, StringComparer.OrdinalIgnoreCase)
                .ThenBy(replacement => replacement.DateAdded)
                .ThenBy(replacement => replacement.Id)
                .ToArray()
        };
}
