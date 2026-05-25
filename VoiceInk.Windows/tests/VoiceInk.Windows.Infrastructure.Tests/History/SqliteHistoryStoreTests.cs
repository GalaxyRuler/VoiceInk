using Microsoft.Data.Sqlite;
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

    [Fact]
    public async Task SaveAndListRecentAsync_RoundTripsRichMetadata()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var item = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            new DateTimeOffset(2026, 5, 24, 12, 0, 0, TimeSpan.Zero),
            "final text",
            "local-whisper",
            TimeSpan.FromSeconds(4),
            TimeSpan.FromMilliseconds(700),
            originalText: " original text ",
            enhancedText: "enhanced text",
            status: TranscriptionHistoryStatus.Completed,
            language: "en",
            modelPath: "C:\\Models\\ggml-base.en.bin",
            promptName: "Default",
            enhancementDuration: TimeSpan.FromMilliseconds(250),
            errorMessage: null,
            enhancementProviderName: "openai-compatible",
            enhancementModelName: "gpt-compatible",
            aiRequestSystemMessage: "system prompt",
            aiRequestUserMessage: "user prompt",
            powerModeName: "Chat",
            powerModeEmoji: "C");

        await store.SaveAsync(item, CancellationToken.None);

        var loaded = await store.ListRecentAsync(1, CancellationToken.None);

        Assert.Equal(item, Assert.Single(loaded));
    }

    [Fact]
    public async Task SaveAndListRecentAsync_RoundTripsPowerModeMetadata()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var item = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            new DateTimeOffset(2026, 5, 24, 12, 0, 0, TimeSpan.Zero),
            "final text",
            "local-whisper",
            TimeSpan.FromSeconds(4),
            TimeSpan.FromMilliseconds(700),
            powerModeName: "Docs",
            powerModeEmoji: "D");

        await store.SaveAsync(item, CancellationToken.None);

        var loaded = Assert.Single(await store.ListRecentAsync(1, CancellationToken.None));

        Assert.Equal("Docs", loaded.PowerModeName);
        Assert.Equal("D", loaded.PowerModeEmoji);
        Assert.Equal(item, loaded);
    }

    [Fact]
    public async Task SaveAndListRecentAsync_RoundTripsAudioFilePath()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var item = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            new DateTimeOffset(2026, 5, 24, 12, 0, 0, TimeSpan.Zero),
            "final text",
            "local-whisper",
            TimeSpan.FromSeconds(4),
            TimeSpan.FromMilliseconds(700),
            audioFilePath: @"C:\Recordings\sample.wav");

        await store.SaveAsync(item, CancellationToken.None);

        var recent = Assert.Single(await store.ListRecentAsync(1, CancellationToken.None));
        var search = Assert.Single(await store.SearchAsync("final text", 1, CancellationToken.None));
        var latest = await store.GetLatestCompletedAsync(CancellationToken.None);

        Assert.Equal(@"C:\Recordings\sample.wav", recent.AudioFilePath);
        Assert.Equal(item, recent);
        Assert.Equal(item, search);
        Assert.Equal(item, latest);
    }

    [Fact]
    public async Task ListRecentAsync_MigratesMvpSchemaAndMapsOriginalTextToText()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        CreateMvpHistoryDatabase(dbPath);

        var store = new SqliteHistoryStore(dbPath);
        var results = await store.ListRecentAsync(10, CancellationToken.None);

        var item = Assert.Single(results);
        Assert.Equal("legacy text", item.Text);
        Assert.Equal("legacy text", item.OriginalText);
        Assert.Equal("legacy-provider", item.ProviderName);
        Assert.Equal(TranscriptionHistoryStatus.Completed, item.Status);
        Assert.Equal("auto", item.Language);
        Assert.Null(item.EnhancedText);
        Assert.Null(item.ModelPath);
        Assert.Null(item.PowerModeName);
        Assert.Null(item.PowerModeEmoji);
    }

    [Fact]
    public async Task MigrateLegacyDatabase_SetsAudioFilePathToNull()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        CreateMvpHistoryDatabase(dbPath);

        var store = new SqliteHistoryStore(dbPath);
        var item = Assert.Single(await store.ListRecentAsync(10, CancellationToken.None));

        Assert.Null(item.AudioFilePath);
    }

    [Fact]
    public async Task SaveAndListRecentAsync_RoundTripsFailedAndCanceledStatuses()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var failed = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(-1),
            "Transcription Failed: model failed",
            "local-whisper",
            TimeSpan.FromSeconds(3),
            TimeSpan.Zero,
            status: TranscriptionHistoryStatus.Failed,
            errorMessage: "model failed");
        var canceled = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "The transcription was canceled.",
            "local-whisper",
            TimeSpan.FromSeconds(1),
            TimeSpan.Zero,
            status: TranscriptionHistoryStatus.Canceled);

        await store.SaveAsync(failed, CancellationToken.None);
        await store.SaveAsync(canceled, CancellationToken.None);

        var results = await store.ListRecentAsync(10, CancellationToken.None);

        Assert.Equal(TranscriptionHistoryStatus.Canceled, results[0].Status);
        Assert.Equal(TranscriptionHistoryStatus.Failed, results[1].Status);
        Assert.Equal("model failed", results[1].ErrorMessage);
    }

    [Fact]
    public async Task GetLatestCompletedAsync_ReturnsCompletedItemAfterNewerNonCompletedItems()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var completed = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(-1),
            "completed",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero);
        await store.SaveAsync(completed, CancellationToken.None);

        for (var index = 0; index < 12; index++)
        {
            await store.SaveAsync(
                new TranscriptionHistoryItem(
                    Guid.NewGuid(),
                    DateTimeOffset.UtcNow.AddMinutes(index),
                    $"failed {index}",
                    "local-whisper",
                    TimeSpan.Zero,
                    TimeSpan.Zero,
                    status: TranscriptionHistoryStatus.Failed),
                CancellationToken.None);
        }

        var latestCompleted = await store.GetLatestCompletedAsync(CancellationToken.None);

        Assert.Equal(completed, latestCompleted);
    }

    [Fact]
    public async Task GetLatestCompletedWithAudioAsync_ReturnsLatestCompletedItemWithAudioPath()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var retryable = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(-5),
            "retryable",
            "local-whisper",
            TimeSpan.FromSeconds(1),
            TimeSpan.Zero,
            audioFilePath: "C:\\Audio\\retryable.wav");
        var textOnly = retryable with
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            Text = "newer text-only",
            OriginalText = "newer text-only",
            AudioFilePath = null
        };

        await store.SaveAsync(retryable, CancellationToken.None);
        await store.SaveAsync(textOnly, CancellationToken.None);

        var latestCompleted = await store.GetLatestCompletedWithAudioAsync(CancellationToken.None);

        Assert.Equal(retryable, latestCompleted);
    }

    [Fact]
    public async Task SearchAsync_MatchesFinalOriginalEnhancedAndMetadata()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var match = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "final VoiceInk text",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero,
            originalText: "raw dictated text",
            enhancedText: "polished transcript",
            language: "en",
            modelPath: "C:\\Models\\ggml-base.en.bin");
        var miss = match with
        {
            Id = Guid.NewGuid(),
            Text = "unrelated",
            OriginalText = "other",
            EnhancedText = null,
            ModelPath = null
        };

        await store.SaveAsync(miss, CancellationToken.None);
        await store.SaveAsync(match, CancellationToken.None);

        var results = await store.SearchAsync("polished", 10, CancellationToken.None);

        var item = Assert.Single(results);
        Assert.Equal(match, item);
    }

    [Fact]
    public async Task SearchAsync_EscapesPercentLikeWildcard()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var literalPercent = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "literal 100% value",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero);
        var unrelated = literalPercent with
        {
            Id = Guid.NewGuid(),
            Text = "literal 1000 value",
            OriginalText = "literal 1000 value"
        };

        await store.SaveAsync(unrelated, CancellationToken.None);
        await store.SaveAsync(literalPercent, CancellationToken.None);

        var results = await store.SearchAsync("100%", 10, CancellationToken.None);

        var item = Assert.Single(results);
        Assert.Equal(literalPercent, item);
    }

    [Fact]
    public async Task SearchAsync_EscapesUnderscoreLikeWildcard()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var literalUnderscore = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "literal voice_ink value",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero);
        var unrelated = literalUnderscore with
        {
            Id = Guid.NewGuid(),
            Text = "literal voiceXink value",
            OriginalText = "literal voiceXink value"
        };

        await store.SaveAsync(unrelated, CancellationToken.None);
        await store.SaveAsync(literalUnderscore, CancellationToken.None);

        var results = await store.SearchAsync("voice_ink", 10, CancellationToken.None);

        var item = Assert.Single(results);
        Assert.Equal(literalUnderscore, item);
    }

    [Fact]
    public async Task SearchAsync_EscapesBackslashLikeEscapeCharacter()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var literalBackslash = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "path C:\\Models\\ggml-base.en.bin",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero);
        var unrelated = literalBackslash with
        {
            Id = Guid.NewGuid(),
            Text = "path C:Modelsggml-base.en.bin",
            OriginalText = "path C:Modelsggml-base.en.bin"
        };

        await store.SaveAsync(unrelated, CancellationToken.None);
        await store.SaveAsync(literalBackslash, CancellationToken.None);

        var results = await store.SearchAsync(@"C:\Models", 10, CancellationToken.None);

        var item = Assert.Single(results);
        Assert.Equal(literalBackslash, item);
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQueryReturnsRecentItems()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var item = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "recent",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero);

        await store.SaveAsync(item, CancellationToken.None);

        var results = await store.SearchAsync(" ", 10, CancellationToken.None);

        var loaded = Assert.Single(results);
        Assert.Equal(item, loaded);
    }

    [Fact]
    public async Task DeleteAsync_RemovesOnlyMatchingItem()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var deleted = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "delete me",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero);
        var kept = deleted with
        {
            Id = Guid.NewGuid(),
            Text = "keep me"
        };

        await store.SaveAsync(deleted, CancellationToken.None);
        await store.SaveAsync(kept, CancellationToken.None);

        Assert.True(await store.DeleteAsync(deleted.Id, CancellationToken.None));
        Assert.False(await store.DeleteAsync(deleted.Id, CancellationToken.None));

        var results = await store.ListRecentAsync(10, CancellationToken.None);
        var item = Assert.Single(results);
        Assert.Equal(kept, item);
    }

    [Fact]
    public async Task ListOlderThanAsync_ReturnsOnlyItemsOlderThanCutoff()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var old = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.Parse("2026-05-25T10:00:00Z"),
            "old",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero);
        var newItem = old with
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.Parse("2026-05-25T12:00:00Z"),
            Text = "new",
            OriginalText = "new"
        };

        await store.SaveAsync(old, CancellationToken.None);
        await store.SaveAsync(newItem, CancellationToken.None);

        var results = await store.ListOlderThanAsync(
            DateTimeOffset.Parse("2026-05-25T11:00:00Z"),
            CancellationToken.None);

        var item = Assert.Single(results);
        Assert.Equal(old, item);
    }

    [Fact]
    public async Task ClearAudioFilePathAsync_ClearsOnlySelectedRows()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var cleared = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(-2),
            "clear",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero,
            audioFilePath: @"C:\Audio\clear.wav");
        var kept = cleared with
        {
            Id = Guid.NewGuid(),
            Text = "keep",
            OriginalText = "keep",
            AudioFilePath = @"C:\Audio\keep.wav"
        };

        await store.SaveAsync(cleared, CancellationToken.None);
        await store.SaveAsync(kept, CancellationToken.None);

        var count = await store.ClearAudioFilePathAsync([cleared.Id], CancellationToken.None);

        Assert.Equal(1, count);
        var rows = await store.ListRecentAsync(10, CancellationToken.None);
        Assert.Null(rows.Single(row => row.Id == cleared.Id).AudioFilePath);
        Assert.Equal(@"C:\Audio\keep.wav", rows.Single(row => row.Id == kept.Id).AudioFilePath);
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

    private static void CreateMvpHistoryDatabase(string dbPath)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Pooling = false
        }.ToString());
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE transcriptions (
                id TEXT PRIMARY KEY,
                created_at TEXT NOT NULL,
                created_at_utc_ticks INTEGER NOT NULL,
                text TEXT NOT NULL,
                provider_name TEXT NOT NULL,
                audio_duration_ms REAL NOT NULL,
                transcription_duration_ms REAL NOT NULL
            );
            INSERT INTO transcriptions
                (id, created_at, created_at_utc_ticks, text, provider_name, audio_duration_ms, transcription_duration_ms)
            VALUES
                ('11111111-1111-1111-1111-111111111111', '2026-05-24T12:00:00.0000000+00:00', 638837136000000000, 'legacy text', 'legacy-provider', 1200, 340);
            """;
        command.ExecuteNonQuery();
    }
}
