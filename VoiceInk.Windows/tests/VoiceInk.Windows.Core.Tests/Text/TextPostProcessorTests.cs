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
}
