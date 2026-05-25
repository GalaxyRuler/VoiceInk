using Microsoft.Data.Sqlite;
using VoiceInk.Windows.Core.Metrics;
using VoiceInk.Windows.Infrastructure.Metrics;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Metrics;

public sealed class SqliteSessionMetricStoreTests
{
    [Fact]
    public async Task SaveAndGetSummaryAsync_PersistsAcrossStoreRecreation()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "metrics.db");
        var store = new SqliteSessionMetricStore(dbPath);
        await store.SaveAsync(Metric(wordCount: 70, audioDuration: TimeSpan.FromMinutes(1)), CancellationToken.None);
        await store.SaveAsync(Metric(wordCount: 35, audioDuration: TimeSpan.FromMinutes(2)), CancellationToken.None);

        store = new SqliteSessionMetricStore(dbPath);
        var summary = await store.GetSummaryAsync(CancellationToken.None);

        Assert.Equal(2, summary.TotalSessions);
        Assert.Equal(105, summary.TotalWords);
        Assert.Equal(TimeSpan.FromMinutes(3), summary.TotalAudioDuration);
        Assert.Equal(35, summary.WordsPerMinute);
        Assert.Equal(525, summary.KeystrokesSaved);
    }

    [Fact]
    public async Task SaveAsync_IgnoresDuplicateTranscriptionIds()
    {
        using var temp = new TempDirectory();
        var store = new SqliteSessionMetricStore(Path.Combine(temp.Path, "metrics.db"));
        var transcriptionId = Guid.NewGuid();

        await store.SaveAsync(Metric(transcriptionId: transcriptionId, wordCount: 10), CancellationToken.None);
        await store.SaveAsync(Metric(transcriptionId: transcriptionId, wordCount: 99), CancellationToken.None);

        Assert.True(await store.HasTranscriptionAsync(transcriptionId, CancellationToken.None));
        var summary = await store.GetSummaryAsync(CancellationToken.None);
        Assert.Equal(1, summary.TotalSessions);
        Assert.Equal(10, summary.TotalWords);
    }

    [Fact]
    public async Task ListTranscriptionModelPerformanceAsync_GroupsAndSortsByAverageProcessingTime()
    {
        using var temp = new TempDirectory();
        var store = new SqliteSessionMetricStore(Path.Combine(temp.Path, "metrics.db"));
        await store.SaveAsync(Metric(
            transcriptionModelName: "base",
            audioDuration: TimeSpan.FromSeconds(60),
            transcriptionDuration: TimeSpan.FromSeconds(15)), CancellationToken.None);
        await store.SaveAsync(Metric(
            transcriptionModelName: "base",
            audioDuration: TimeSpan.FromSeconds(30),
            transcriptionDuration: TimeSpan.FromSeconds(5)), CancellationToken.None);
        await store.SaveAsync(Metric(
            transcriptionModelName: "large",
            audioDuration: TimeSpan.FromSeconds(60),
            transcriptionDuration: TimeSpan.FromSeconds(30)), CancellationToken.None);
        await store.SaveAsync(Metric(
            transcriptionModelName: null,
            transcriptionDuration: TimeSpan.FromSeconds(1)), CancellationToken.None);

        var stats = await store.ListTranscriptionModelPerformanceAsync(null, CancellationToken.None);

        Assert.Collection(
            stats,
            stat =>
            {
                Assert.Equal("base", stat.Name);
                Assert.Equal(2, stat.SessionCount);
                Assert.Equal(TimeSpan.FromSeconds(10), stat.AverageProcessingDuration);
                Assert.Equal(TimeSpan.FromSeconds(45), stat.AverageAudioDuration);
                Assert.Equal(4.5, stat.SpeedFactor);
            },
            stat =>
            {
                Assert.Equal("large", stat.Name);
                Assert.Equal(1, stat.SessionCount);
            });
    }

    [Fact]
    public async Task ListEnhancementModelPerformanceAsync_FiltersBySince()
    {
        using var temp = new TempDirectory();
        var store = new SqliteSessionMetricStore(Path.Combine(temp.Path, "metrics.db"));
        var since = new DateTimeOffset(2026, 5, 25, 12, 0, 0, TimeSpan.Zero);
        await store.SaveAsync(Metric(
            timestamp: since.AddMinutes(-1),
            enhancementModelName: "old-model",
            enhancementDuration: TimeSpan.FromSeconds(1)), CancellationToken.None);
        await store.SaveAsync(Metric(
            timestamp: since.AddMinutes(1),
            enhancementModelName: "new-model",
            enhancementDuration: TimeSpan.FromSeconds(4)), CancellationToken.None);

        var stats = await store.ListEnhancementModelPerformanceAsync(since, CancellationToken.None);

        var stat = Assert.Single(stats);
        Assert.Equal("new-model", stat.Name);
        Assert.Equal(1, stat.SessionCount);
        Assert.Equal(TimeSpan.FromSeconds(4), stat.AverageProcessingDuration);
    }

    [Fact]
    public async Task ExistingDatabase_MigratesMissingOptionalColumns()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "metrics.db");
        CreateLegacyMetricsDatabase(dbPath);

        var store = new SqliteSessionMetricStore(dbPath);
        await store.SaveAsync(Metric(enhancementModelName: "gpt-4o-mini", enhancementDuration: TimeSpan.FromSeconds(2)), CancellationToken.None);

        var summary = await store.GetSummaryAsync(CancellationToken.None);
        var enhancement = Assert.Single(await store.ListEnhancementModelPerformanceAsync(null, CancellationToken.None));
        Assert.Equal(2, summary.TotalSessions);
        Assert.Equal(15, summary.TotalWords);
        Assert.Equal("gpt-4o-mini", enhancement.Name);
    }

    private static SessionMetric Metric(
        Guid? transcriptionId = null,
        DateTimeOffset? timestamp = null,
        int wordCount = 5,
        TimeSpan? audioDuration = null,
        string? transcriptionModelName = "base",
        TimeSpan? transcriptionDuration = null,
        string? enhancementModelName = null,
        TimeSpan? enhancementDuration = null) =>
        new(
            Guid.NewGuid(),
            transcriptionId ?? Guid.NewGuid(),
            timestamp ?? new DateTimeOffset(2026, 5, 25, 10, 0, 0, TimeSpan.Zero),
            "recorder",
            wordCount,
            audioDuration ?? TimeSpan.FromSeconds(10),
            transcriptionModelName,
            transcriptionDuration,
            transcriptionDuration is null || transcriptionDuration <= TimeSpan.Zero
                ? null
                : (audioDuration ?? TimeSpan.FromSeconds(10)).TotalSeconds / transcriptionDuration.Value.TotalSeconds,
            "Docs",
            enhancementModelName,
            enhancementDuration);

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"voiceink-metrics-{Guid.NewGuid():N}");

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

    private static void CreateLegacyMetricsDatabase(string dbPath)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Pooling = false
        }.ToString());
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE session_metrics (
                id TEXT PRIMARY KEY,
                transcription_id TEXT NOT NULL UNIQUE,
                timestamp TEXT NOT NULL,
                timestamp_utc_ticks INTEGER NOT NULL,
                source TEXT NOT NULL,
                word_count INTEGER NOT NULL,
                audio_duration_ms REAL NOT NULL,
                transcription_model_name TEXT NULL,
                transcription_duration_ms REAL NULL
            );
            INSERT INTO session_metrics
                (id, transcription_id, timestamp, timestamp_utc_ticks, source, word_count,
                 audio_duration_ms, transcription_model_name, transcription_duration_ms)
            VALUES
                ('11111111-1111-1111-1111-111111111111',
                 '22222222-2222-2222-2222-222222222222',
                 '2026-05-25T10:00:00.0000000+00:00',
                 638837640000000000,
                 'recorder',
                 10,
                 1000,
                 'base',
                 250);
            """;
        command.ExecuteNonQuery();
    }
}
