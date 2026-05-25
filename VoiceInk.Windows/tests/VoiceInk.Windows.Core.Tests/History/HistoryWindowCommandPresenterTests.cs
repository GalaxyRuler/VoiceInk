using VoiceInk.Windows.Core.History;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.History;

public sealed class HistoryWindowCommandPresenterTests
{
    [Fact]
    public void Present_DisablesSelectedRowCommandsWhenNoItemIsSelected()
    {
        var state = HistoryWindowCommandPresenter.Present(null);

        Assert.False(state.CanCopyOriginal);
        Assert.False(state.CanCopyFinal);
        Assert.False(state.CanCopyEnhanced);
        Assert.False(state.CanCopyAiRequest);
        Assert.False(state.CanRetry);
        Assert.False(state.CanReenhance);
        Assert.False(state.CanOpenAudio);
        Assert.False(state.CanDelete);
        Assert.Equal("Select a transcription", state.SelectionStatus);
    }

    [Fact]
    public void Present_EnablesTextOnlyCompletedRowActions()
    {
        var state = HistoryWindowCommandPresenter.Present(HistoryItem());

        Assert.True(state.CanCopyOriginal);
        Assert.True(state.CanCopyFinal);
        Assert.False(state.CanCopyEnhanced);
        Assert.False(state.CanCopyAiRequest);
        Assert.False(state.CanRetry);
        Assert.True(state.CanReenhance);
        Assert.False(state.CanOpenAudio);
        Assert.True(state.CanDelete);
        Assert.Equal("Completed transcription selected", state.SelectionStatus);
    }

    [Fact]
    public void Present_EnablesMatchingActionsForRichCompletedRow()
    {
        var state = HistoryWindowCommandPresenter.Present(HistoryItem() with
        {
            EnhancedText = "enhanced",
            AudioFilePath = @"C:\Recordings\sample.wav",
            AiRequestSystemMessage = "system",
            AiRequestUserMessage = "user"
        });

        Assert.True(state.CanCopyOriginal);
        Assert.True(state.CanCopyFinal);
        Assert.True(state.CanCopyEnhanced);
        Assert.True(state.CanCopyAiRequest);
        Assert.True(state.CanRetry);
        Assert.True(state.CanReenhance);
        Assert.True(state.CanOpenAudio);
        Assert.True(state.CanDelete);
    }

    [Theory]
    [InlineData(TranscriptionHistoryStatus.Failed)]
    [InlineData(TranscriptionHistoryStatus.Canceled)]
    public void Present_DisablesRetryAndReenhanceForNonCompletedRows(TranscriptionHistoryStatus status)
    {
        var state = HistoryWindowCommandPresenter.Present(HistoryItem() with
        {
            Status = status,
            AudioFilePath = @"C:\Recordings\sample.wav",
            Text = "diagnostic text"
        });

        Assert.True(state.CanCopyOriginal);
        Assert.True(state.CanCopyFinal);
        Assert.False(state.CanRetry);
        Assert.False(state.CanReenhance);
        Assert.True(state.CanOpenAudio);
        Assert.True(state.CanDelete);
        Assert.Equal($"{status} transcription selected", state.SelectionStatus);
    }

    private static TranscriptionHistoryItem HistoryItem() =>
        new(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "final text",
            "local-whisper",
            TimeSpan.FromSeconds(3),
            TimeSpan.FromMilliseconds(300),
            originalText: "original text",
            status: TranscriptionHistoryStatus.Completed);
}
