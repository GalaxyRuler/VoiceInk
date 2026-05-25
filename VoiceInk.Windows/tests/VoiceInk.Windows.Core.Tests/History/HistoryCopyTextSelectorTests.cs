using VoiceInk.Windows.Core.History;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.History;

public sealed class HistoryCopyTextSelectorTests
{
    [Fact]
    public void Select_ReturnsOriginalText()
    {
        var item = HistoryItem() with { OriginalText = "raw transcript" };

        var result = HistoryCopyTextSelector.Select(item, HistoryCopyTextKind.Original);

        Assert.True(result.Success);
        Assert.Equal("Original transcription copied", result.Message);
        Assert.Equal("raw transcript", result.Text);
    }

    [Fact]
    public void Select_ReturnsFinalText()
    {
        var item = HistoryItem() with { Text = "final transcript" };

        var result = HistoryCopyTextSelector.Select(item, HistoryCopyTextKind.Final);

        Assert.True(result.Success);
        Assert.Equal("Final transcription copied", result.Message);
        Assert.Equal("final transcript", result.Text);
    }

    [Fact]
    public void Select_ReturnsEnhancedTextWhenAvailable()
    {
        var item = HistoryItem() with { EnhancedText = "enhanced transcript" };

        var result = HistoryCopyTextSelector.Select(item, HistoryCopyTextKind.Enhanced);

        Assert.True(result.Success);
        Assert.Equal("Enhanced transcription copied", result.Message);
        Assert.Equal("enhanced transcript", result.Text);
    }

    [Fact]
    public void Select_ReturnsFailureWhenEnhancedTextIsMissing()
    {
        var result = HistoryCopyTextSelector.Select(HistoryItem(), HistoryCopyTextKind.Enhanced);

        Assert.False(result.Success);
        Assert.Equal("No enhanced transcription available", result.Message);
        Assert.Equal(string.Empty, result.Text);
    }

    [Fact]
    public void Select_CombinesAiRequestMessages()
    {
        var item = HistoryItem() with
        {
            AiRequestSystemMessage = "system prompt",
            AiRequestUserMessage = "user message"
        };

        var result = HistoryCopyTextSelector.Select(item, HistoryCopyTextKind.AiRequest);

        Assert.True(result.Success);
        Assert.Equal("AI request copied", result.Message);
        Assert.Equal(
            $"System Prompt:{Environment.NewLine}system prompt{Environment.NewLine}{Environment.NewLine}User Message:{Environment.NewLine}user message",
            result.Text);
    }

    [Fact]
    public void Select_ReturnsFailureWhenAiRequestMessagesAreMissing()
    {
        var result = HistoryCopyTextSelector.Select(HistoryItem(), HistoryCopyTextKind.AiRequest);

        Assert.False(result.Success);
        Assert.Equal("No AI request available", result.Message);
        Assert.Equal(string.Empty, result.Text);
    }

    [Fact]
    public void Select_ReturnsFailureWhenRequestedTextIsBlank()
    {
        var item = HistoryItem() with { Text = " " };

        var result = HistoryCopyTextSelector.Select(item, HistoryCopyTextKind.Final);

        Assert.False(result.Success);
        Assert.Equal("No final transcription available", result.Message);
        Assert.Equal(string.Empty, result.Text);
    }

    private static TranscriptionHistoryItem HistoryItem() =>
        new(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "final text",
            "local-whisper",
            TimeSpan.FromSeconds(1),
            TimeSpan.FromMilliseconds(100),
            originalText: "original text",
            status: TranscriptionHistoryStatus.Completed);
}
