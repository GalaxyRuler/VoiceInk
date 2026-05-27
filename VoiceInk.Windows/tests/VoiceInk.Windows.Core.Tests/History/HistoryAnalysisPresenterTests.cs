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
            },
            row =>
            {
                Assert.Equal("Retry and Re-enhance", row.Title);
                Assert.Equal("Original text", row.Value);
                Assert.Equal("Retry needs saved audio; re-enhance uses the original transcript text for a fresh AI pass.", row.Detail);
            },
            row =>
            {
                Assert.Equal("Export Scope", row.Title);
                Assert.Equal("User initiated", row.Value);
                Assert.Equal("History text, metadata, and audio paths stay local unless you copy, paste, or export them.", row.Detail);
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
        Assert.Equal("Retry and Re-enhance", rows[5].Title);
        Assert.Equal("Unavailable", rows[5].Value);
        Assert.Equal("Retry and re-enhance are available only for completed history items.", rows[5].Detail);
        Assert.Equal("Export Scope", rows[6].Title);
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
        Assert.Equal("Retry and Re-enhance", rows[5].Title);
        Assert.Equal("Audio and original text", rows[5].Value);
        Assert.Equal("Retry uses the saved audio file; re-enhance uses the original transcript text for a fresh AI pass.", rows[5].Detail);
        Assert.Equal("Export Scope", rows[6].Title);
        Assert.Equal("User initiated", rows[6].Value);
    }
}
