using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Text;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Text;

public sealed class TextPostProcessorTests
{
    [Fact]
    public void Process_TrimsWhitespace()
    {
        var result = TextPostProcessor.Process("  hello from voice ink \r\n", new TextPostProcessingOptions());

        Assert.Equal("hello from voice ink", result);
    }

    [Fact]
    public void Process_AppendsTrailingSpaceWhenEnabled()
    {
        var result = TextPostProcessor.Process("hello", new TextPostProcessingOptions(AppendTrailingSpace: true));

        Assert.Equal("hello ", result);
    }

    [Fact]
    public void Process_ReturnsEmptyStringForWhitespaceOnlyInput()
    {
        var result = TextPostProcessor.Process(" \r\n\t ", new TextPostProcessingOptions(AppendTrailingSpace: true));

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Process_RemovesTagBlocksAndBracketedHallucinations()
    {
        var result = TextPostProcessor.Process(
            "hello <NOISE>ignore me</NOISE> [music] (laughs) {noise} world",
            new TextPostProcessingOptions());

        Assert.Equal("hello world", result);
    }

    [Fact]
    public void Process_RemovesConfiguredFillerWordsWithTrailingPunctuation()
    {
        var result = TextPostProcessor.Process(
            "Um, hello uh. world",
            new TextPostProcessingOptions(RemoveFillerWords: true, FillerWords: ["um", "uh"]));

        Assert.Equal("hello world", result);
    }

    [Fact]
    public void Process_CanKeepConfiguredFillerWords()
    {
        var result = TextPostProcessor.Process(
            "Um, hello",
            new TextPostProcessingOptions(RemoveFillerWords: false, FillerWords: ["um"]));

        Assert.Equal("Um, hello", result);
    }

    [Fact]
    public void Process_RemoveAllPunctuationRemovesApostrophesAndSeparatesOtherPunctuation()
    {
        var result = TextPostProcessor.Process(
            "Don't stop, VoiceInk!",
            new TextPostProcessingOptions(PunctuationCleanupMode: PunctuationCleanupMode.RemoveAll));

        Assert.Equal("Dont stop VoiceInk", result);
    }

    [Fact]
    public void Process_RemoveAllPunctuationPreservesLineBreaks()
    {
        var result = TextPostProcessor.Process(
            "first,\nsecond",
            new TextPostProcessingOptions(PunctuationCleanupMode: PunctuationCleanupMode.RemoveAll));

        Assert.Equal("first\nsecond", result);
    }

    [Fact]
    public void Process_RemoveTrailingPeriodRemovesSingleTrailingPeriod()
    {
        var result = TextPostProcessor.Process(
            "Hello there.  ",
            new TextPostProcessingOptions(PunctuationCleanupMode: PunctuationCleanupMode.RemoveTrailingPeriod));

        Assert.Equal("Hello there", result);
    }

    [Fact]
    public void Process_RemoveTrailingPeriodPreservesEllipsis()
    {
        var result = TextPostProcessor.Process(
            "Wait...",
            new TextPostProcessingOptions(PunctuationCleanupMode: PunctuationCleanupMode.RemoveTrailingPeriod));

        Assert.Equal("Wait...", result);
    }

    [Fact]
    public void Process_LowercasesAfterPunctuationCleanup()
    {
        var result = TextPostProcessor.Process(
            "HELLO, WORLD!",
            new TextPostProcessingOptions(
                PunctuationCleanupMode: PunctuationCleanupMode.RemoveAll,
                LowercaseTranscription: true));

        Assert.Equal("hello world", result);
    }

    [Fact]
    public void Process_AppliesWordReplacementsBeforeCleanupPreferences()
    {
        var replacements = new[]
        {
            new WordReplacement(Guid.NewGuid(), "voice ink", "VoiceInk!", DateTimeOffset.UtcNow)
        };

        var result = TextPostProcessor.Process(
            "voice ink",
            new TextPostProcessingOptions(
                WordReplacements: replacements,
                PunctuationCleanupMode: PunctuationCleanupMode.RemoveAll));

        Assert.Equal("VoiceInk", result);
    }

    [Fact]
    public void Process_NormalizesUnicodeBeforeApplyingWordReplacements()
    {
        var replacements = new[]
        {
            new WordReplacement(Guid.NewGuid(), "café", "Cafe", DateTimeOffset.UtcNow)
        };

        var result = TextPostProcessor.Process(
            "cafe\u0301",
            new TextPostProcessingOptions(WordReplacements: replacements));

        Assert.Equal("Cafe", result);
    }

    [Fact]
    public void Process_KeepsParagraphFormattingOffByDefault()
    {
        var input = string.Join(
            " ",
            "First sentence has enough words.",
            "Second sentence has enough words.",
            "Third sentence has enough words.",
            "Fourth sentence has enough words.",
            "Fifth sentence has enough words.");

        var result = TextPostProcessor.Process(input, new TextPostProcessingOptions());

        Assert.Equal(input, result);
    }

    [Fact]
    public void Process_AppliesMacStyleParagraphFormattingWhenEnabled()
    {
        var result = TextPostProcessor.Process(
            string.Join(
                " ",
                "First sentence has enough words.",
                "Second sentence has enough words.",
                "Third sentence has enough words.",
                "Fourth sentence has enough words.",
                "Fifth sentence has enough words."),
            new TextPostProcessingOptions(ApplyTextFormatting: true));

        Assert.Equal(
            string.Join(
                "\n\n",
                "First sentence has enough words. Second sentence has enough words. Third sentence has enough words. Fourth sentence has enough words.",
                "Fifth sentence has enough words."),
            result);
    }

    [Fact]
    public void Process_AppliesDictationLineAndParagraphCommandsWhenFormattingIsEnabled()
    {
        var result = TextPostProcessor.Process(
            "First item new line second item new paragraph final item",
            new TextPostProcessingOptions(ApplyTextFormatting: true));

        Assert.Equal(
            string.Join(
                "\n\n",
                string.Join(
                    "\n",
                    "First item",
                    "second item"),
                "final item"),
            result);
    }

    [Fact]
    public void Process_AppliesDictationFormattingCommandsWithTrailingPunctuation()
    {
        var result = TextPostProcessor.Process(
            "First item new line, second item new paragraph. final item",
            new TextPostProcessingOptions(ApplyTextFormatting: true));

        Assert.Equal(
            string.Join(
                "\n\n",
                string.Join(
                    "\n",
                    "First item",
                    "second item"),
                "final item"),
            result);
    }

    [Fact]
    public void Process_KeepsDictationLineAndParagraphCommandsWhenFormattingIsDisabled()
    {
        var input = "First item new line second item new paragraph final item";

        var result = TextPostProcessor.Process(
            input,
            new TextPostProcessingOptions(ApplyTextFormatting: false));

        Assert.Equal(input, result);
    }

    [Fact]
    public void Process_AppliesTextFormattingBeforeWordReplacements()
    {
        var replacements = new[]
        {
            new WordReplacement(
                Guid.NewGuid(),
                "First sentence has enough words. Second sentence has enough words. Third sentence has enough words. Fourth sentence has enough words.",
                "First paragraph replaced.",
                DateTimeOffset.UtcNow)
        };

        var result = TextPostProcessor.Process(
            string.Join(
                " ",
                "First sentence has enough words.",
                "Second sentence has enough words.",
                "Third sentence has enough words.",
                "Fourth sentence has enough words.",
                "Fifth sentence has enough words."),
            new TextPostProcessingOptions(
                WordReplacements: replacements,
                ApplyTextFormatting: true));

        Assert.Equal(
            string.Join(
                "\n\n",
                "First paragraph replaced.",
                "Fifth sentence has enough words."),
            result);
    }

    [Fact]
    public void Process_AppliesTextFormattingBeforePunctuationCleanup()
    {
        var result = TextPostProcessor.Process(
            string.Join(
                " ",
                "First sentence has enough words.",
                "Second sentence has enough words.",
                "Third sentence has enough words.",
                "Fourth sentence has enough words.",
                "Fifth sentence has enough words."),
            new TextPostProcessingOptions(
                ApplyTextFormatting: true,
                PunctuationCleanupMode: PunctuationCleanupMode.RemoveAll));

        Assert.Equal(
            string.Join(
                "\n\n",
                "First sentence has enough words Second sentence has enough words Third sentence has enough words Fourth sentence has enough words",
                "Fifth sentence has enough words"),
            result);
    }
}
