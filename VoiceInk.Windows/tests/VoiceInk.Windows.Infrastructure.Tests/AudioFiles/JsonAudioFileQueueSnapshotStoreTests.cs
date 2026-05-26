using VoiceInk.Windows.Core.AudioFiles;
using VoiceInk.Windows.Infrastructure.AudioFiles;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.AudioFiles;

public sealed class JsonAudioFileQueueSnapshotStoreTests
{
    [Fact]
    public async Task LoadAsync_WhenFileDoesNotExist_ReturnsEmptySnapshot()
    {
        var path = TempPath();
        var store = new JsonAudioFileQueueSnapshotStore(path);

        var snapshot = await store.LoadAsync(CancellationToken.None);

        Assert.Empty(snapshot.Items);
    }

    [Fact]
    public async Task SaveAndLoadAsync_RoundTripsSnapshot()
    {
        using var directory = new TempDirectory();
        var path = Path.Combine(directory.Path, "queue.json");
        var store = new JsonAudioFileQueueSnapshotStore(path);
        var expected = new AudioFileQueueSnapshot(
            [
                new AudioFileQueueSnapshotItem(
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    @"C:\Audio\clip.wav",
                    AudioFileQueueStatus.Failed,
                    "Failed",
                    "Decoder failed")
            ]);

        await store.SaveAsync(expected, CancellationToken.None);
        var actual = await store.LoadAsync(CancellationToken.None);

        var item = Assert.Single(actual.Items);
        Assert.Equal(expected.Items[0], item);
    }

    [Fact]
    public async Task SaveAsync_WithEmptySnapshot_DeletesExistingFile()
    {
        using var directory = new TempDirectory();
        var path = Path.Combine(directory.Path, "queue.json");
        var store = new JsonAudioFileQueueSnapshotStore(path);
        await store.SaveAsync(new AudioFileQueueSnapshot(
            [new AudioFileQueueSnapshotItem(Guid.NewGuid(), @"C:\Audio\clip.wav", AudioFileQueueStatus.Pending, "Waiting", null)]),
            CancellationToken.None);

        await store.SaveAsync(AudioFileQueueSnapshot.Empty, CancellationToken.None);

        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task LoadAsync_WithMalformedJson_ReturnsEmptySnapshot()
    {
        using var directory = new TempDirectory();
        var path = Path.Combine(directory.Path, "queue.json");
        await File.WriteAllTextAsync(path, "{ not json", CancellationToken.None);
        var store = new JsonAudioFileQueueSnapshotStore(path);

        var snapshot = await store.LoadAsync(CancellationToken.None);

        Assert.Empty(snapshot.Items);
    }

    private static string TempPath() =>
        Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
