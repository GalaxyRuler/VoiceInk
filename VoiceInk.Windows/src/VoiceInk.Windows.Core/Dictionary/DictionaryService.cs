using System.Text.RegularExpressions;

namespace VoiceInk.Windows.Core.Dictionary;

public static class DictionaryService
{
    public static IReadOnlyList<VocabularyWord> AddVocabularyWords(
        string input,
        IEnumerable<VocabularyWord> existing,
        DateTimeOffset now,
        out string? error)
    {
        error = null;

        var parts = SplitCommaSeparated(input);
        if (parts.Count == 0)
        {
            return [];
        }

        var existingWords = existing
            .Select(word => word.Word)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (parts.Count == 1 && existingWords.Contains(parts[0]))
        {
            error = $"'{parts[0]}' is already in the vocabulary";
            return [];
        }

        var added = new List<VocabularyWord>();
        var seen = new HashSet<string>(existingWords, StringComparer.OrdinalIgnoreCase);
        foreach (var word in parts)
        {
            if (!seen.Add(word))
            {
                continue;
            }

            added.Add(new VocabularyWord(Guid.NewGuid(), word, now));
        }

        return added;
    }

    public static WordReplacement? AddWordReplacement(
        string original,
        string replacement,
        IEnumerable<WordReplacement> existing,
        DateTimeOffset now,
        out string? error)
    {
        error = null;

        var originals = SplitCommaSeparated(original);
        var trimmedReplacement = replacement.Trim();
        if (originals.Count == 0 || trimmedReplacement.Length == 0)
        {
            return null;
        }

        var existingTokens = existing
            .SelectMany(entry => SplitCommaSeparated(entry.OriginalText))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var token in originals)
        {
            if (existingTokens.Contains(token))
            {
                error = $"'{token}' already exists in word replacements";
                return null;
            }
        }

        return new WordReplacement(
            Guid.NewGuid(),
            original.Trim(),
            trimmedReplacement,
            now,
            IsEnabled: true);
    }

    public static WordReplacement? UpdateWordReplacement(
        WordReplacement current,
        string original,
        string replacement,
        bool isEnabled,
        IEnumerable<WordReplacement> existing,
        out string? error)
    {
        error = null;

        var originals = SplitCommaSeparated(original);
        var trimmedReplacement = replacement.Trim();
        if (originals.Count == 0)
        {
            error = "Original text is required.";
            return null;
        }

        if (trimmedReplacement.Length == 0)
        {
            error = "Replacement text is required.";
            return null;
        }

        var existingTokens = existing
            .Where(entry => entry.Id != current.Id)
            .SelectMany(entry => SplitCommaSeparated(entry.OriginalText))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var token in originals)
        {
            if (existingTokens.Contains(token))
            {
                error = $"'{token}' already exists in word replacements";
                return null;
            }
        }

        return current with
        {
            OriginalText = string.Join(", ", originals),
            ReplacementText = trimmedReplacement,
            IsEnabled = isEnabled
        };
    }

    public static string RenderVocabularyPrompt(IEnumerable<VocabularyWord> words)
    {
        var vocabulary = words
            .Select(word => word.Word.Trim())
            .Where(word => word.Length > 0)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return vocabulary.Length == 0
            ? string.Empty
            : $"Important Vocabulary: {string.Join(", ", vocabulary)}";
    }

    public static string ApplyReplacements(string text, IEnumerable<WordReplacement> replacements)
    {
        var result = text;
        var replacementVariants = replacements
            .Where(replacement => replacement.IsEnabled)
            .SelectMany(
                (replacement, replacementIndex) => SplitCommaSeparated(replacement.OriginalText)
                    .Select((variant, variantIndex) => new
                    {
                        Variant = variant,
                        replacement.ReplacementText,
                        ReplacementIndex = replacementIndex,
                        VariantIndex = variantIndex
                    }))
            .OrderByDescending(replacement => replacement.Variant.Length)
            .ThenBy(replacement => replacement.ReplacementIndex)
            .ThenBy(replacement => replacement.VariantIndex)
            .ToArray();

        foreach (var replacement in replacementVariants)
        {
            result = ReplaceVariant(result, replacement.Variant, replacement.ReplacementText);
        }

        return result;
    }

    private static string ReplaceVariant(string text, string original, string replacement)
    {
        if (UsesWordBoundaries(original))
        {
            var pattern = $@"(?<![a-zA-Z0-9]){Regex.Escape(original)}(?![a-zA-Z0-9])";
            return Regex.Replace(
                text,
                pattern,
                _ => replacement,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        return Regex.Replace(
            text,
            Regex.Escape(original),
            _ => replacement,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static IReadOnlyList<string> SplitCommaSeparated(string value) =>
        value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(part => part.Length > 0)
            .ToArray();

    private static bool UsesWordBoundaries(string text)
    {
        foreach (var rune in text.EnumerateRunes())
        {
            var value = (uint)rune.Value;
            if (IsInRange(value, 0x3040, 0x309F) ||
                IsInRange(value, 0x30A0, 0x30FF) ||
                IsInRange(value, 0x4E00, 0x9FFF) ||
                IsInRange(value, 0xAC00, 0xD7AF) ||
                IsInRange(value, 0x0E00, 0x0E7F))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsInRange(uint value, uint start, uint end) =>
        value >= start && value <= end;
}
