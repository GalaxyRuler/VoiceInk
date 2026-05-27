using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using VoiceInk.Windows.Core.Dictionary;

namespace VoiceInk.Windows.Core.Text;

public static class TextPostProcessor
{
    private const int FormattingTargetWordCount = 50;
    private const int FormattingMaxSignificantSentencesPerChunk = 4;
    private const int FormattingMinWordsForSignificantSentence = 4;

    public static readonly IReadOnlyList<string> DefaultFillerWords =
    [
        "uh",
        "um",
        "uhm",
        "umm",
        "uhh",
        "uhhh",
        "hmm",
        "hm",
        "mmm",
        "mm",
        "mh",
        "ehh"
    ];

    private static readonly Regex TagBlockRegex = new(
        @"<([A-Za-z][A-Za-z0-9:_-]*)[^>]*>[\s\S]*?</\1>",
        RegexOptions.Compiled);

    private static readonly Regex WhitespaceRegex = new(@"\s{2,}", RegexOptions.Compiled);
    private static readonly Regex HorizontalWhitespaceRegex = new(@"[^\S\r\n]{2,}", RegexOptions.Compiled);
    private static readonly Regex SpaceBeforeNewlineRegex = new(@"[ \t]+(\r?\n)", RegexOptions.Compiled);
    private static readonly Regex SpaceAfterNewlineRegex = new(@"(\r?\n)[ \t]+", RegexOptions.Compiled);
    private static readonly Regex WordRegex = new(@"[\p{L}\p{N}]+(?:['’][\p{L}\p{N}]+)?", RegexOptions.Compiled);

    private static readonly Regex[] HallucinationRegexes =
    [
        new(@"\[.*?\]", RegexOptions.Compiled),
        new(@"\(.*?\)", RegexOptions.Compiled),
        new(@"\{.*?\}", RegexOptions.Compiled)
    ];

    private static readonly HashSet<char> ApostropheLikeCharacters =
    [
        '\'',
        '\u2019',
        '\u2018',
        '\u02BC',
        '\uFF07'
    ];

    public static string Process(string text, TextPostProcessingOptions options)
    {
        var processed = text.Normalize(NormalizationForm.FormC);
        processed = RemoveHallucinations(processed);
        processed = RemoveFillerWords(processed, options);
        processed = NormalizeWhitespace(processed);

        if (processed.Length == 0)
        {
            return string.Empty;
        }

        if (options.WordReplacements is { Count: > 0 })
        {
            processed = DictionaryService.ApplyReplacements(processed, options.WordReplacements);
        }

        if (options.ApplyTextFormatting)
        {
            processed = ApplyTextFormatting(processed);
        }

        processed = ApplyPunctuationCleanup(processed, options.PunctuationCleanupMode);

        if (options.LowercaseTranscription)
        {
            processed = processed.ToLowerInvariant();
        }

        var trimmed = processed.Trim();

        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        return options.AppendTrailingSpace ? $"{trimmed} " : trimmed;
    }

    public static string ApplyTrailingSpace(string text, bool appendTrailingSpace)
    {
        var trimmed = text.TrimEnd();
        return appendTrailingSpace && trimmed.Length > 0
            ? $"{trimmed} "
            : trimmed;
    }

    private static string RemoveHallucinations(string text)
    {
        var filtered = TagBlockRegex.Replace(text, string.Empty);
        foreach (var regex in HallucinationRegexes)
        {
            filtered = regex.Replace(filtered, string.Empty);
        }

        return filtered;
    }

    private static string RemoveFillerWords(string text, TextPostProcessingOptions options)
    {
        if (!options.RemoveFillerWords)
        {
            return text;
        }

        var filtered = text;
        var fillerWords = options.FillerWords ?? DefaultFillerWords;
        foreach (var fillerWord in fillerWords)
        {
            var trimmed = fillerWord.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            var pattern = $@"\b{Regex.Escape(trimmed)}\b[,.]?";
            filtered = Regex.Replace(
                filtered,
                pattern,
                string.Empty,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        return filtered;
    }

    private static string ApplyPunctuationCleanup(string text, PunctuationCleanupMode punctuationCleanupMode) =>
        punctuationCleanupMode switch
        {
            PunctuationCleanupMode.Keep => text,
            PunctuationCleanupMode.RemoveAll => RemovePunctuation(text),
            PunctuationCleanupMode.RemoveTrailingPeriod => RemoveTrailingPeriod(text),
            _ => text
        };

    private static string ApplyTextFormatting(string text)
    {
        var sentences = SplitSentences(text);
        if (sentences.Count == 0)
        {
            return string.Empty;
        }

        if (sentences.Count == 1)
        {
            return sentences[0];
        }

        var chunks = new List<string>();
        var sentenceIndex = 0;
        while (sentenceIndex < sentences.Count)
        {
            var tentativeSentences = new List<string>();
            var tentativeWordCount = 0;
            var tentativeSignificantSentenceCount = 0;

            for (var index = sentenceIndex; index < sentences.Count; index++)
            {
                var sentence = sentences[index];
                var wordCount = CountWords(sentence);
                tentativeSentences.Add(sentence);
                tentativeWordCount += wordCount;

                if (wordCount >= FormattingMinWordsForSignificantSentence)
                {
                    tentativeSignificantSentenceCount++;
                }

                if (tentativeWordCount >= FormattingTargetWordCount)
                {
                    break;
                }
            }

            var chunkSentences = tentativeSentences;
            if (tentativeSignificantSentenceCount > FormattingMaxSignificantSentencesPerChunk)
            {
                chunkSentences = [];
                var significantSentences = 0;
                foreach (var sentence in tentativeSentences)
                {
                    chunkSentences.Add(sentence);
                    if (CountWords(sentence) < FormattingMinWordsForSignificantSentence)
                    {
                        continue;
                    }

                    significantSentences++;
                    if (significantSentences >= FormattingMaxSignificantSentencesPerChunk)
                    {
                        break;
                    }
                }
            }

            if (chunkSentences.Count == 0)
            {
                break;
            }

            chunks.Add(string.Join(" ", chunkSentences));
            sentenceIndex += chunkSentences.Count;
        }

        return string.Join("\n\n", chunks).Trim();
    }

    private static IReadOnlyList<string> SplitSentences(string text)
    {
        var sentences = new List<string>();
        var startIndex = 0;
        for (var index = 0; index < text.Length; index++)
        {
            if (!IsSentenceTerminal(text[index]))
            {
                continue;
            }

            if (index + 1 < text.Length && !char.IsWhiteSpace(text[index + 1]))
            {
                continue;
            }

            AddSentence(text, startIndex, index + 1, sentences);
            startIndex = index + 1;
            while (startIndex < text.Length && char.IsWhiteSpace(text[startIndex]))
            {
                startIndex++;
            }

            index = startIndex - 1;
        }

        AddSentence(text, startIndex, text.Length, sentences);
        return sentences;
    }

    private static void AddSentence(string text, int startIndex, int endIndex, ICollection<string> sentences)
    {
        if (endIndex <= startIndex)
        {
            return;
        }

        var sentence = text[startIndex..endIndex].Trim();
        if (sentence.Length > 0)
        {
            sentences.Add(sentence);
        }
    }

    private static bool IsSentenceTerminal(char character) =>
        character is '.' or '!' or '?';

    private static int CountWords(string text) =>
        WordRegex.Matches(text).Count;

    private static string RemoveTrailingPeriod(string text)
    {
        if (text.Length == 0)
        {
            return text;
        }

        var endIndex = text.Length - 1;
        while (endIndex >= 0 && char.IsWhiteSpace(text[endIndex]))
        {
            endIndex--;
        }

        if (endIndex < 0 || text[endIndex] != '.')
        {
            return text;
        }

        if (endIndex > 0 && text[endIndex - 1] == '.')
        {
            return text;
        }

        return text.Remove(endIndex, 1);
    }

    private static string RemovePunctuation(string text)
    {
        var output = new char[text.Length];
        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];
            if (ApostropheLikeCharacters.Contains(character))
            {
                output[index] = '\0';
                continue;
            }

            output[index] = IsPunctuation(character) ? ' ' : character;
        }

        return NormalizePunctuationWhitespace(new string(output).Replace("\0", string.Empty, StringComparison.Ordinal));
    }

    private static bool IsPunctuation(char character) =>
        CharUnicodeInfo.GetUnicodeCategory(character) switch
        {
            UnicodeCategory.ConnectorPunctuation => true,
            UnicodeCategory.DashPunctuation => true,
            UnicodeCategory.OpenPunctuation => true,
            UnicodeCategory.ClosePunctuation => true,
            UnicodeCategory.InitialQuotePunctuation => true,
            UnicodeCategory.FinalQuotePunctuation => true,
            UnicodeCategory.OtherPunctuation => true,
            _ => false
        };

    private static string NormalizeWhitespace(string text) =>
        WhitespaceRegex
            .Replace(text, " ")
            .Trim();

    private static string NormalizePunctuationWhitespace(string text)
    {
        var normalized = HorizontalWhitespaceRegex.Replace(text, " ");
        normalized = SpaceBeforeNewlineRegex.Replace(normalized, "$1");
        normalized = SpaceAfterNewlineRegex.Replace(normalized, "$1");
        return normalized.Trim();
    }
}
