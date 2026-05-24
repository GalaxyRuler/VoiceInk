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

        store = new SqliteHistoryStore(dbPath);
        var results = await store.ListRecentAsync(10, CancellationToken.None);

        Assert.Collection(
            results,
            item => Assert.Equal(newer, item),
            item => Assert.Equal(older, item));
    }

    [Fact]
    public async Task ListRecentAsync_OrdersNewestFirstByInstantAcrossOffsets()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var olderInstantWithLaterLocalTime = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            new DateTimeOffset(2026, 1, 1, 10, 30, 0, TimeSpan.FromHours(2)),
            "older instant",
            "local-whisper",
            TimeSpan.FromSeconds(2),
            TimeSpan.FromMilliseconds(200));
        var newerInstantWithEarlierLocalTime = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.Zero),
            "newer instant",
            "cloud",
            TimeSpan.FromSeconds(3),
            TimeSpan.FromMilliseconds(300));

        await store.SaveAsync(olderInstantWithLaterLocalTime, CancellationToken.None);
        await store.SaveAsync(newerInstantWithEarlierLocalTime, CancellationToken.None);

        var results = await store.ListRecentAsync(10, CancellationToken.None);

        Assert.Collection(
            results,
            item => Assert.Equal(newerInstantWithEarlierLocalTime, item),
            item => Assert.Equal(olderInstantWithLaterLocalTime, item));
    }

    [Fact]
    public async Task ListRecentAsync_OrdersNewestFirstWhenInstantsDifferWithinSameMillisecond()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var createdAt = new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.Zero).AddTicks(1_234);
        var older = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            createdAt,
            "older same millisecond",
            "local-whisper",
            TimeSpan.FromSeconds(2),
            TimeSpan.FromMilliseconds(200));
        var newer = older with
        {
            Id = Guid.NewGuid(),
            CreatedAt = createdAt.AddTicks(5),
            Text = "newer same millisecond"
        };

        await store.SaveAsync(older, CancellationToken.None);
        await store.SaveAsync(newer, CancellationToken.None);

        var results = await store.ListRecentAsync(10, CancellationToken.None);

        Assert.Collection(
            results,
            item => Assert.Equal(newer, item),
            item => Assert.Equal(older, item));
    }

    [Fact]
    public async Task ListRecentAsync_WithLimitOne_ReturnsOnlyNewestItem()
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

        var results = await store.ListRecentAsync(1, CancellationToken.None);

        var item = Assert.Single(results);
        Assert.Equal(newer, item);
    }

    [Fact]
    public async Task ListRecentAsync_WithLimitZero_ReturnsEmptyList()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var item = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "text",
            "local-whisper",
            TimeSpan.FromSeconds(1),
            TimeSpan.FromMilliseconds(100));

        await store.SaveAsync(item, CancellationToken.None);

        var results = await store.ListRecentAsync(0, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task ListRecentAsync_WithNegativeLimit_ThrowsArgumentOutOfRangeException()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => store.ListRecentAsync(-1, CancellationToken.None));
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
