using VoiceInk.Windows.Core.AudioFiles;
using VoiceInk.Windows.Core.History;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.AudioFiles;

public sealed class AudioFileQueueTextActionsTests
{
    [Fact]
    public void TryGetActionText_UsesEnhancedTextWhenCompletedItemHasEnhancement()
    {
        var item = CompletedItem(text: "original text", enhancedText: "enhanced text");

        var success = AudioFileQueueTextActions.TryGetActionText(item, out var text, out var message);

        Assert.True(success);
        Assert.Equal("enhanced text", text);
        Assert.Equal("Ready", message);
    }

    [Fact]
    public void TryGetActionText_UsesFinalTextWhenNoEnhancedTextExists()
    {
        var item = CompletedItem(text: "final text");

        var success = AudioFileQueueTextActions.TryGetActionText(item, out var text, out _);

        Assert.True(success);
        Assert.Equal("final text", text);
    }

    [Theory]
    [InlineData(AudioFileQueueStatus.Pending, "Transcribe the selected file before copying or saving")]
    [InlineData(AudioFileQueueStatus.Processing, "Transcribe the selected file before copying or saving")]
    [InlineData(AudioFileQueueStatus.Failed, "Transcribe the selected file before copying or saving")]
    public void TryGetActionText_RejectsItemsThatAreNotCompleted(AudioFileQueueStatus status, string expectedMessage)
    {
        var item = new AudioFileQueueItem(
            Guid.NewGuid(),
            Path.Combine(Path.GetTempPath(), "clip.wav"),
            "clip.wav",
            status,
            status.ToString());

        var success = AudioFileQueueTextActions.TryGetActionText(item, out var text, out var message);

        Assert.False(success);
        Assert.Equal(string.Empty, text);
        Assert.Equal(expectedMessage, message);
    }

    [Fact]
    public void TryGetActionText_RejectsCompletedItemWithoutText()
    {
        var item = CompletedItem(text: " ");

        var success = AudioFileQueueTextActions.TryGetActionText(item, out var text, out var message);

        Assert.False(success);
        Assert.Equal(string.Empty, text);
        Assert.Equal("The selected transcription has no text to copy or save", message);
    }

    [Theory]
    [InlineData("Hello, Windows transcription!", "hello-windows-transcription")]
    [InlineData("One two three four five six seven eight nine", "one-two-three-four-five-six-seven-eight")]
    [InlineData("Symbols <>:\"/\\|?* stay safe", "symbols-stay-safe")]
    [InlineData("   ", "transcription")]
    public void SuggestFileName_UsesFirstUsefulWordsAndSanitizesWindowsNames(string text, string expected)
    {
        var fileName = AudioFileQueueTextActions.SuggestFileName(text);

        Assert.Equal(expected, fileName);
    }

    [Fact]
    public void FormatMarkdown_IncludesDateAndTranscript()
    {
        var createdAt = new DateTimeOffset(2026, 5, 26, 8, 30, 0, TimeSpan.Zero);

        var markdown = AudioFileQueueTextActions.FormatMarkdown("hello world", createdAt);

        Assert.Contains("# Transcription", markdown);
        Assert.Contains("**Date:**", markdown);
        Assert.Contains("hello world", markdown);
        Assert.DoesNotContain("\r\n\r\n\r\n", markdown);
    }

    private static AudioFileQueueItem CompletedItem(string text, string? enhancedText = null)
    {
        var historyItem = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.Parse("2026-05-26T08:30:00Z"),
            text,
            "Local Whisper",
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(2),
            enhancedText: enhancedText,
            audioFilePath: Path.Combine(Path.GetTempPath(), "clip.wav"));

        return new AudioFileQueueItem(
            Guid.NewGuid(),
            Path.Combine(Path.GetTempPath(), "clip.wav"),
            "clip.wav",
            AudioFileQueueStatus.Completed,
            "Completed",
            historyItem);
    }
}
