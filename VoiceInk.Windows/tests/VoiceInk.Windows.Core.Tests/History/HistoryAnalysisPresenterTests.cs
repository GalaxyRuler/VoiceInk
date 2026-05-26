using VoiceInk.Windows.Core.History;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.History;

public sealed class HistoryAnalysisPresenterTests
{
    [Fact]
    public void Present_CompletedEnhancedItem_ReturnsLocalAnalysisRows()
    {
        var item = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.Parse("2026-05-26T12:00:00Z"),
            "rough words",
            "Local Whisper",
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(5),
            originalText: "rough words",
            enhancedText: "polished words for sending",
            status: TranscriptionHistoryStatus.Completed,
            enhancementDuration: TimeSpan.FromSeconds(2));

        var rows = HistoryAnalysisPresenter.Present(item);

        Assert.Collection(
            rows,
            row =>
            {
                Assert.Equal("Words", row.Title);
                Assert.Equal("4", row.Value);
                Assert.Equal("Final transcript", row.Detail);
            },
            row =>
            {
                Assert.Equal("Audio", row.Title);
                Assert.Equal("30s", row.Value);
                Assert.Equal("8 wpm", row.Detail);
            },
            row =>
            {
                Assert.Equal("Enhancement", row.Title);
                Assert.Equal("Enhanced", row.Value);
                Assert.Equal("AI cleanup completed in 2s", row.Detail);
            },
            row =>
            {
                Assert.Equal("Provider", row.Title);
                Assert.Equal("Local Whisper", row.Value);
                Assert.Equal("Transcription completed in 5s.", row.Detail);
            },
            row =>
            {
                Assert.Equal("Audio Storage", row.Title);
                Assert.Equal("Text only", row.Value);
                Assert.Equal("No audio file is attached to this history item.", row.Detail);
            });
    }

    [Fact]
    public void Present_FailedItem_UsesFailureStatusAndOriginalText()
    {
        var item = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.Parse("2026-05-26T12:00:00Z"),
            "fallback text",
            "Local Whisper",
            TimeSpan.Zero,
            TimeSpan.Zero,
            status: TranscriptionHistoryStatus.Failed,
            errorMessage: "Provider failed");

        var rows = HistoryAnalysisPresenter.Present(item);

        Assert.Equal("2", rows[0].Value);
        Assert.Equal("0s", rows[1].Value);
        Assert.Equal("Speech rate unavailable", rows[1].Detail);
        Assert.Equal("Failed", rows[2].Value);
        Assert.Equal("Provider failed", rows[2].Detail);
        Assert.Equal("Local Whisper", rows[3].Value);
        Assert.Equal("Transcription duration unavailable.", rows[3].Detail);
        Assert.Equal("Text only", rows[4].Value);
    }

    [Fact]
    public void Present_ItemWithAudioFile_ShowsReplayStorageAvailability()
    {
        var item = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.Parse("2026-05-26T12:00:00Z"),
            "saved audio text",
            "Deepgram",
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(2),
            audioFilePath: @"C:\Recordings\sample.wav");

        var rows = HistoryAnalysisPresenter.Present(item);

        Assert.Equal("Provider", rows[3].Title);
        Assert.Equal("Deepgram", rows[3].Value);
        Assert.Equal("Transcription completed in 2s.", rows[3].Detail);
        Assert.Equal("Audio Storage", rows[4].Title);
        Assert.Equal("Audio saved", rows[4].Value);
        Assert.Equal("Audio can be opened or replayed while the file remains on disk.", rows[4].Detail);
    }
}
