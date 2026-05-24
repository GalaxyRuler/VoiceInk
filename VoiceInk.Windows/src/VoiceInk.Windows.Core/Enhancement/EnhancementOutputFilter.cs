using System.Text.RegularExpressions;

namespace VoiceInk.Windows.Core.Enhancement;

public static partial class EnhancementOutputFilter
{
    public static string Filter(string text)
    {
        var processed = text;
        foreach (var regex in ThinkingBlockRegexes())
        {
            processed = regex.Replace(processed, string.Empty);
        }

        return processed.Trim();
    }

    private static Regex[] ThinkingBlockRegexes() =>
    [
        ThinkingRegex(),
        ThinkRegex(),
        ReasoningRegex()
    ];

    [GeneratedRegex("<thinking>.*?</thinking>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex ThinkingRegex();

    [GeneratedRegex("<think>.*?</think>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex ThinkRegex();

    [GeneratedRegex("<reasoning>.*?</reasoning>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex ReasoningRegex();
}
