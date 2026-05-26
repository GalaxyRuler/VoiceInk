using VoiceInk.Windows.Core.AudioFiles;
using VoiceInk.Windows.Core.History;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.AudioFiles;

public sealed class AudioFileQueuePersistenceTests
{
    [Fact]
    public void CreateSnapshot_IncludesPendingFailedAndProcessingButExcludesCompleted()
    {
        using var pendingFile = new TempMediaFile(".wav");
        using var failedFile = new TempMediaFile(".mp3");
        using var processingFile = new TempMediaFile(".m4a");
        using var completedFile = new TempMediaFile(".wav");
        var pending = Item(pendingFile.Path, AudioFileQueueStatus.Pending, "Waiting");
        var failed = Item(failedFile.Path, AudioFileQueueStatus.Failed, "Failed", "Decoder failed");
        var processing = Item(processingFile.Path, AudioFileQueueStatus.Processing, "Transcribing");
        var completed = Item(completedFile.Path, AudioFileQueueStatus.Completed, "Completed")
            .MarkCompleted(new TranscriptionHistoryItem(
                Guid.NewGuid(),
                DateTimeOffset.Now,
                "done",
                "Local Whisper",
                TimeSpan.Zero,
                TimeSpan.Zero));

        var snapshot = AudioFileQueuePersistence.CreateSnapshot([pending, failed, processing, completed]);

        Assert.Collection(
            snapshot.Items,
            item => Assert.Equal(pending.Id, item.Id),
            item => Assert.Equal(failed.Id, item.Id),
            item => Assert.Equal(processing.Id, item.Id));
    }

    [Fact]
    public void Restore_ResetsProcessingToPendingAndPreservesFailedError()
    {
        using var processingFile = new TempMediaFile(".wav");
        using var failedFile = new TempMediaFile(".flac");
        var snapshot = new AudioFileQueueSnapshot(
            [
                new AudioFileQueueSnapshotItem(
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    processingFile.Path,
                    AudioFileQueueStatus.Processing,
                    "Transcribing",
                    null),
                new AudioFileQueueSnapshotItem(
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    failedFile.Path,
                    AudioFileQueueStatus.Failed,
                    "Failed",
                    "Decoder failed")
            ]);

        var restored = AudioFileQueuePersistence.Restore(snapshot);

        Assert.Collection(
            restored,
            item =>
            {
                Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), item.Id);
                Assert.Equal(AudioFileQueueStatus.Pending, item.Status);
                Assert.Equal("Waiting", item.StatusDetail);
                Assert.Null(item.ErrorMessage);
            },
            item =>
            {
                Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), item.Id);
                Assert.Equal(AudioFileQueueStatus.Failed, item.Status);
                Assert.Equal("Decoder failed", item.ErrorMessage);
            });
    }

    [Fact]
    public void Restore_SkipsMissingUnsupportedAndDuplicateActivePaths()
    {
        using var wav = new TempMediaFile(".wav");
        using var text = new TempMediaFile(".txt");
        var snapshot = new AudioFileQueueSnapshot(
            [
                new AudioFileQueueSnapshotItem(Guid.NewGuid(), wav.Path, AudioFileQueueStatus.Pending, "Waiting", null),
                new AudioFileQueueSnapshotItem(Guid.NewGuid(), wav.Path, AudioFileQueueStatus.Pending, "Waiting", null),
                new AudioFileQueueSnapshotItem(Guid.NewGuid(), text.Path, AudioFileQueueStatus.Pending, "Waiting", null),
                new AudioFileQueueSnapshotItem(Guid.NewGuid(), Path.Combine(Path.GetTempPath(), "missing.wav"), AudioFileQueueStatus.Pending, "Waiting", null)
            ]);

        var restored = AudioFileQueuePersistence.Restore(snapshot);

        var item = Assert.Single(restored);
        Assert.Equal(Path.GetFullPath(wav.Path), item.FilePath);
    }

    private static AudioFileQueueItem Item(
        string filePath,
        AudioFileQueueStatus status,
        string detail,
        string? error = null) =>
        new(
            Guid.NewGuid(),
            filePath,
            Path.GetFileName(filePath),
            status,
            detail,
            ErrorMessage: error);

    private sealed class TempMediaFile : IDisposable
    {
        public TempMediaFile(string extension)
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");
            File.WriteAllText(Path, string.Empty);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }
}
