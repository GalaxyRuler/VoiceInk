using VoiceInk.Windows.Core.AudioFiles;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.AudioFiles;

public sealed class AudioFileQueueServiceTests
{
    [Fact]
    public void AddFiles_AddsExistingSupportedFilesWithDeterministicIds()
    {
        using var wav = new TempMediaFile(".wav");
        using var mp3 = new TempMediaFile(".mp3");
        var ids = new Queue<Guid>([Guid.Parse("11111111-1111-1111-1111-111111111111"), Guid.Parse("22222222-2222-2222-2222-222222222222")]);
        var service = new AudioFileQueueService(() => ids.Dequeue());

        var update = service.AddFiles([], [wav.Path, mp3.Path]);

        Assert.Equal(2, update.AddedCount);
        Assert.Equal(0, update.SkippedCount);
        Assert.Collection(
            update.Items,
            item =>
            {
                Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), item.Id);
                Assert.Equal(wav.Path, item.FilePath);
                Assert.Equal(Path.GetFileName(wav.Path), item.FileName);
                Assert.Equal(AudioFileQueueStatus.Pending, item.Status);
            },
            item =>
            {
                Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), item.Id);
                Assert.Equal(mp3.Path, item.FilePath);
                Assert.Equal(AudioFileQueueStatus.Pending, item.Status);
            });
    }

    [Fact]
    public void AddFiles_SkipsMissingUnsupportedAndDuplicateActiveFiles()
    {
        using var wav = new TempMediaFile(".wav");
        using var text = new TempMediaFile(".txt");
        var existing = new AudioFileQueueItem(
            Guid.NewGuid(),
            wav.Path,
            Path.GetFileName(wav.Path),
            AudioFileQueueStatus.Processing,
            "Transcribing");
        var service = new AudioFileQueueService(() => Guid.NewGuid());

        var update = service.AddFiles([existing], [wav.Path, text.Path, Path.Combine(Path.GetTempPath(), "missing.wav")]);

        Assert.Equal(0, update.AddedCount);
        Assert.Equal(3, update.SkippedCount);
        Assert.Single(update.Items);
        Assert.Equal(existing, update.Items[0]);
    }

    [Fact]
    public void RemovePending_RemovesOnlyPendingItems()
    {
        var pending = Item(AudioFileQueueStatus.Pending);
        var processing = Item(AudioFileQueueStatus.Processing);
        var service = new AudioFileQueueService();

        var update = service.RemovePending([pending, processing], pending.Id);
        var ignored = service.RemovePending([pending, processing], processing.Id);

        Assert.Single(update.Items);
        Assert.Equal(processing.Id, update.Items[0].Id);
        Assert.Equal(2, ignored.Items.Count);
    }

    [Fact]
    public void RetryFailed_ReturnsFailedItemToPending()
    {
        var failed = Item(AudioFileQueueStatus.Failed, "Decoder failed");
        var service = new AudioFileQueueService();

        var update = service.RetryFailed([failed], failed.Id);

        var item = Assert.Single(update.Items);
        Assert.Equal(AudioFileQueueStatus.Pending, item.Status);
        Assert.Equal("Waiting", item.StatusDetail);
        Assert.Null(item.ErrorMessage);
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var service = new AudioFileQueueService();

        var update = service.Clear([Item(AudioFileQueueStatus.Pending), Item(AudioFileQueueStatus.Completed)]);

        Assert.Empty(update.Items);
        Assert.Equal(2, update.RemovedCount);
    }

    private static AudioFileQueueItem Item(AudioFileQueueStatus status, string? error = null)
    {
        var id = Guid.NewGuid();
        return new AudioFileQueueItem(
            id,
            Path.Combine(Path.GetTempPath(), $"{id}.wav"),
            $"{id}.wav",
            status,
            status == AudioFileQueueStatus.Pending ? "Waiting" : status.ToString(),
            ErrorMessage: error);
    }

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
