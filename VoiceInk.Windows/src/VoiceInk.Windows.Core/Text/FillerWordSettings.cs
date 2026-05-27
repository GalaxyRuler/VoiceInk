namespace VoiceInk.Windows.Core.Text;

public static class FillerWordSettings
{
    public static string[] ParseList(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return text.Split([',', '\r', '\n'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static string ToEditableText(IReadOnlyList<string> words) =>
        string.Join(Environment.NewLine, words);

    public static IReadOnlyList<string>? EffectiveList(IReadOnlyList<string> words) =>
        words.Count == 0 ? null : words;

    public static bool TryAdd(IReadOnlyList<string> currentWords, string word, out string[] words)
    {
        var normalized = NormalizeWord(word);
        if (normalized.Length == 0
            || currentWords.Any(existing => string.Equals(existing, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            words = currentWords.ToArray();
            return false;
        }

        words = [.. currentWords, normalized];
        return true;
    }

    public static string[] Remove(IReadOnlyList<string> currentWords, string word) =>
        currentWords
            .Where(existing => !string.Equals(existing, word, StringComparison.OrdinalIgnoreCase))
            .ToArray();

    private static string NormalizeWord(string word) =>
        word.Trim().ToLowerInvariant();
}
