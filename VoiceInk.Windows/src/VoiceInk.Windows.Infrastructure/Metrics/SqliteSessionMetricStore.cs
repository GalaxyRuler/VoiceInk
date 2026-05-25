using System.Globalization;
using Microsoft.Data.Sqlite;
using VoiceInk.Windows.Core.Metrics;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Infrastructure.Metrics;

public sealed class SqliteSessionMetricStore : ISessionMetricStore
{
    private const double AverageTypingWordsPerMinute = 35;
    private const int AverageKeystrokesPerWord = 5;
    private readonly string connectionString;

    public SqliteSessionMetricStore(string databasePath)
    {
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Pooling = false
        }.ToString();
        EnsureDatabase();
    }

    public async Task SaveAsync(SessionMetric metric, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT OR IGNORE INTO session_metrics
                (id, transcription_id, timestamp, timestamp_utc_ticks, source, word_count,
                 audio_duration_ms, transcription_model_name, transcription_duration_ms, speed_factor,
                 power_mode_name, ai_enhancement_model_name, enhancement_duration_ms)
            VALUES
                ($id, $transcription_id, $timestamp, $timestamp_utc_ticks, $source, $word_count,
                 $audio_duration_ms, $transcription_model_name, $transcription_duration_ms, $speed_factor,
                 $power_mode_name, $ai_enhancement_model_name, $enhancement_duration_ms);
            """;
        command.Parameters.AddWithValue("$id", metric.Id.ToString());
        command.Parameters.AddWithValue("$transcription_id", metric.TranscriptionId.ToString());
        command.Parameters.AddWithValue("$timestamp", metric.Timestamp.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$timestamp_utc_ticks", metric.Timestamp.UtcDateTime.Ticks);
        command.Parameters.AddWithValue("$source", string.IsNullOrWhiteSpace(metric.Source) ? "recorder" : metric.Source);
        command.Parameters.AddWithValue("$word_count", Math.Max(0, metric.WordCount));
        command.Parameters.AddWithValue("$audio_duration_ms", Milliseconds(metric.AudioDuration));
        command.Parameters.AddWithValue("$transcription_model_name", ValueOrDbNull(metric.TranscriptionModelName));
        command.Parameters.AddWithValue("$transcription_duration_ms", ValueOrDbNull(Milliseconds(metric.TranscriptionDuration)));
        command.Parameters.AddWithValue("$speed_factor", ValueOrDbNull(metric.SpeedFactor));
        command.Parameters.AddWithValue("$power_mode_name", ValueOrDbNull(metric.PowerModeName));
        command.Parameters.AddWithValue("$ai_enhancement_model_name", ValueOrDbNull(metric.AiEnhancementModelName));
        command.Parameters.AddWithValue("$enhancement_duration_ms", ValueOrDbNull(Milliseconds(metric.EnhancementDuration)));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ClearAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM session_metrics;";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> HasTranscriptionAsync(Guid transcriptionId, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM session_metrics WHERE transcription_id = $transcription_id LIMIT 1;";
        command.Parameters.AddWithValue("$transcription_id", transcriptionId.ToString());

        return await command.ExecuteScalarAsync(cancellationToken) is not null;
    }

    public Task<SessionMetricsSummary> GetSummaryAsync(CancellationToken cancellationToken) =>
        GetSummaryAsync(since: null, cancellationToken);

    public async Task<SessionMetricsSummary> GetSummaryAsync(
        DateTimeOffset? since,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*), COALESCE(SUM(word_count), 0), COALESCE(SUM(audio_duration_ms), 0)
            FROM session_metrics
            WHERE ($since_utc_ticks IS NULL OR timestamp_utc_ticks >= $since_utc_ticks);
            """;
        command.Parameters.AddWithValue("$since_utc_ticks", ValueOrDbNull(since?.UtcDateTime.Ticks));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return SessionMetricsSummary.Empty;
        }

        var totalSessions = checked((int)reader.GetInt64(0));
        var totalWords = checked((int)reader.GetInt64(1));
        var totalAudioDuration = TimeSpan.FromMilliseconds(reader.GetDouble(2));
        return SummaryFromTotals(totalSessions, totalWords, totalAudioDuration);
    }

    public async Task<IReadOnlyList<ModelPerformanceStat>> ListTranscriptionModelPerformanceAsync(
        DateTimeOffset? since,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT MIN(transcription_model_name),
                   COUNT(*),
                   COALESCE(SUM(transcription_duration_ms), 0),
                   COALESCE(SUM(audio_duration_ms), 0)
            FROM session_metrics
            WHERE ($since_utc_ticks IS NULL OR timestamp_utc_ticks >= $since_utc_ticks)
              AND transcription_model_name IS NOT NULL
              AND TRIM(transcription_model_name) <> ''
              AND transcription_duration_ms IS NOT NULL
              AND transcription_duration_ms > 0
            GROUP BY transcription_model_name COLLATE NOCASE
            ORDER BY (COALESCE(SUM(transcription_duration_ms), 0) / COUNT(*)) ASC,
                     MIN(transcription_model_name) COLLATE NOCASE ASC;
            """;
        command.Parameters.AddWithValue("$since_utc_ticks", ValueOrDbNull(since?.UtcDateTime.Ticks));

        var stats = new List<ModelPerformanceStat>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var name = reader.GetString(0);
            var sessionCount = checked((int)reader.GetInt64(1));
            var totalProcessing = TimeSpan.FromMilliseconds(reader.GetDouble(2));
            var totalAudio = TimeSpan.FromMilliseconds(reader.GetDouble(3));
            stats.Add(new ModelPerformanceStat(
                name,
                sessionCount,
                totalProcessing,
                AverageDuration(totalProcessing, sessionCount),
                AverageDuration(totalAudio, sessionCount),
                totalProcessing > TimeSpan.Zero ? totalAudio.TotalSeconds / totalProcessing.TotalSeconds : 0));
        }

        return stats;
    }

    public async Task<IReadOnlyList<ModelPerformanceStat>> ListEnhancementModelPerformanceAsync(
        DateTimeOffset? since,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT MIN(ai_enhancement_model_name),
                   COUNT(*),
                   COALESCE(SUM(enhancement_duration_ms), 0)
            FROM session_metrics
            WHERE ($since_utc_ticks IS NULL OR timestamp_utc_ticks >= $since_utc_ticks)
              AND ai_enhancement_model_name IS NOT NULL
              AND TRIM(ai_enhancement_model_name) <> ''
              AND enhancement_duration_ms IS NOT NULL
              AND enhancement_duration_ms > 0
            GROUP BY ai_enhancement_model_name COLLATE NOCASE
            ORDER BY (COALESCE(SUM(enhancement_duration_ms), 0) / COUNT(*)) ASC,
                     MIN(ai_enhancement_model_name) COLLATE NOCASE ASC;
            """;
        command.Parameters.AddWithValue("$since_utc_ticks", ValueOrDbNull(since?.UtcDateTime.Ticks));

        var stats = new List<ModelPerformanceStat>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var name = reader.GetString(0);
            var sessionCount = checked((int)reader.GetInt64(1));
            var totalProcessing = TimeSpan.FromMilliseconds(reader.GetDouble(2));
            stats.Add(new ModelPerformanceStat(
                name,
                sessionCount,
                totalProcessing,
                AverageDuration(totalProcessing, sessionCount),
                TimeSpan.Zero,
                0));
        }

        return stats;
    }

    private void EnsureDatabase()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS session_metrics (
                id TEXT PRIMARY KEY,
                transcription_id TEXT NOT NULL UNIQUE,
                timestamp TEXT NOT NULL,
                timestamp_utc_ticks INTEGER NOT NULL,
                source TEXT NOT NULL,
                word_count INTEGER NOT NULL,
                audio_duration_ms REAL NOT NULL,
                transcription_model_name TEXT NULL,
                transcription_duration_ms REAL NULL,
                speed_factor REAL NULL,
                power_mode_name TEXT NULL,
                ai_enhancement_model_name TEXT NULL,
                enhancement_duration_ms REAL NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS idx_session_metrics_transcription_id
                ON session_metrics(transcription_id);
            CREATE INDEX IF NOT EXISTS idx_session_metrics_timestamp_utc_ticks
                ON session_metrics(timestamp_utc_ticks DESC);
            """;
        command.ExecuteNonQuery();

        EnsureColumn(connection, "speed_factor", "speed_factor REAL NULL");
        EnsureColumn(connection, "power_mode_name", "power_mode_name TEXT NULL");
        EnsureColumn(connection, "ai_enhancement_model_name", "ai_enhancement_model_name TEXT NULL");
        EnsureColumn(connection, "enhancement_duration_ms", "enhancement_duration_ms REAL NULL");
    }

    private static SessionMetricsSummary SummaryFromTotals(
        int totalSessions,
        int totalWords,
        TimeSpan totalAudioDuration)
    {
        var wordsPerMinute = totalAudioDuration > TimeSpan.Zero
            ? totalWords / totalAudioDuration.TotalMinutes
            : 0;
        var estimatedTypingTime = TimeSpan.FromMinutes(totalWords / AverageTypingWordsPerMinute);
        var timeSaved = estimatedTypingTime > totalAudioDuration
            ? estimatedTypingTime - totalAudioDuration
            : TimeSpan.Zero;

        return new SessionMetricsSummary(
            totalSessions,
            totalWords,
            totalAudioDuration,
            wordsPerMinute,
            totalWords * AverageKeystrokesPerWord,
            timeSaved);
    }

    private static TimeSpan AverageDuration(TimeSpan totalDuration, int count) =>
        count <= 0 ? TimeSpan.Zero : TimeSpan.FromTicks(totalDuration.Ticks / count);

    private static void EnsureColumn(SqliteConnection connection, string columnName, string definition)
    {
        if (ColumnExists(connection, columnName))
        {
            return;
        }

        using var command = connection.CreateCommand();
        command.CommandText = $"ALTER TABLE session_metrics ADD COLUMN {definition};";
        command.ExecuteNonQuery();
    }

    private static bool ColumnExists(SqliteConnection connection, string columnName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info(session_metrics);";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static double Milliseconds(TimeSpan duration) =>
        duration > TimeSpan.Zero ? duration.TotalMilliseconds : 0;

    private static double? Milliseconds(TimeSpan? duration) =>
        duration is not null && duration.Value > TimeSpan.Zero
            ? duration.Value.TotalMilliseconds
            : null;

    private static object ValueOrDbNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();

    private static object ValueOrDbNull(double? value) =>
        value is null ? DBNull.Value : value.Value;

    private static object ValueOrDbNull(long? value) =>
        value is null ? DBNull.Value : value.Value;
}
