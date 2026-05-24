using VoiceInk.Windows.Core.History;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.History;

public sealed class HistoryCsvExporterTests
{
    [Fact]
    public void Export_ReturnsMacStyleHeaderAndRichMetadataRows()
    {
        var item = new TranscriptionHistoryItem(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            new DateTimeOffset(2026, 5, 24, 12, 0, 0, TimeSpan.Zero),
            "final text",
            "local-whisper",
            TimeSpan.FromSeconds(4),
            TimeSpan.FromMilliseconds(700),
            originalText: "original text",
            enhancedText: "enhanced text",
            status: TranscriptionHistoryStatus.Completed,
            language: "en",
            modelPath: "C:\\Models\\ggml-base.en.bin",
            promptName: "Default",
            enhancementDuration: TimeSpan.FromMilliseconds(250));

        var csv = HistoryCsvExporter.Export([item]);

        Assert.StartsWith(
            "Original Transcript,Enhanced Transcript,Prompt Name,Transcription Model,Provider,Status,Language,Transcription Time,Enhancement Time,Timestamp,Duration,Error Message",
            csv);
        Assert.Contains(
            "original text,enhanced text,Default,C:\\Models\\ggml-base.en.bin,local-whisper,Completed,en,0.700,0.250,2026-05-24T12:00:00.0000000+00:00,4.000,",
            csv);
    }

    [Fact]
    public void Export_EscapesCommasQuotesAndNewlines()
    {
        var item = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UnixEpoch,
            "final",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero,
            originalText: "hello, \"VoiceInk\"\nworld",
            enhancedText: null);

        var csv = HistoryCsvExporter.Export([item]);

        Assert.Contains("\"hello, \"\"VoiceInk\"\"\nworld\",", csv);
    }

    [Fact]
    public void Export_IncludesFailedAndCanceledMetadata()
    {
        var failed = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UnixEpoch,
            "Transcription Failed: model failed",
            "local-whisper",
            TimeSpan.FromSeconds(2),
            TimeSpan.Zero,
            status: TranscriptionHistoryStatus.Failed,
            errorMessage: "model failed");

        var csv = HistoryCsvExporter.Export([failed]);

        Assert.Contains(
            ",local-whisper,Failed,auto,0.000,,1970-01-01T00:00:00.0000000+00:00,2.000,model failed",
            csv);
    }
}
