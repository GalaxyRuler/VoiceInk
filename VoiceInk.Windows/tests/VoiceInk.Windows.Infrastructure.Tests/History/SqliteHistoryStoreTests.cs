using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Infrastructure.History;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.History;

public sealed class SqliteHistoryStoreTests
{
    [Fact]
    public async Task SaveAndListRecentAsync_ReturnsNewestFirst()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var older = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(-5),
            "older text",
            "local-whisper",
            TimeSpan.FromSeconds(1),
            TimeSpan.FromMilliseconds(100));
        var newer = older with
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            Text = "newer text"
        };

        await store.SaveAsync(older, CancellationToken.None);
        await store.SaveAsync(newer, CancellationToken.None);

        var results = await store.ListRecentAsync(10, CancellationToken.None);

        Assert.Collection(
            results,
            item => Assert.Equal("newer text", item.Text),
            item => Assert.Equal("older text", item.Text));
    }

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"voiceink-{Guid.NewGuid():N}");

        public TempDirectory()
        {
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
