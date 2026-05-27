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
}
