using System.Globalization;
using Microsoft.Data.Sqlite;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Infrastructure.History;

public sealed class SqliteHistoryStore : IHistoryStore
{
    private readonly string connectionString;

    public SqliteHistoryStore(string databasePath)
    {
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath, Pooling = false }.ToString();
        EnsureDatabase();
    }

    public async Task SaveAsync(TranscriptionHistoryItem item, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO transcriptions
                (id, created_at, created_at_utc_ticks, text, original_text, enhanced_text, status,
                 provider_name, language, model_path, prompt_name, audio_duration_ms,
                 transcription_duration_ms, enhancement_duration_ms, error_message)
            VALUES
                ($id, $created_at, $created_at_utc_ticks, $text, $original_text, $enhanced_text, $status,
                 $provider_name, $language, $model_path, $prompt_name, $audio_duration_ms,
                 $transcription_duration_ms, $enhancement_duration_ms, $error_message);
            """;
        command.Parameters.AddWithValue("$id", item.Id.ToString());
        command.Parameters.AddWithValue("$created_at", item.CreatedAt.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$created_at_utc_ticks", item.CreatedAt.UtcDateTime.Ticks);
        command.Parameters.AddWithValue("$text", item.Text);
        command.Parameters.AddWithValue("$original_text", item.OriginalText);
        command.Parameters.AddWithValue("$enhanced_text", ValueOrDbNull(item.EnhancedText));
        command.Parameters.AddWithValue("$status", StatusToStorage(item.Status));
        command.Parameters.AddWithValue("$provider_name", item.ProviderName);
        command.Parameters.AddWithValue("$language", item.Language);
        command.Parameters.AddWithValue("$model_path", ValueOrDbNull(item.ModelPath));
        command.Parameters.AddWithValue("$prompt_name", ValueOrDbNull(item.PromptName));
        command.Parameters.AddWithValue("$audio_duration_ms", item.AudioDuration.TotalMilliseconds);
        command.Parameters.AddWithValue("$transcription_duration_ms", item.TranscriptionDuration.TotalMilliseconds);
        command.Parameters.AddWithValue("$enhancement_duration_ms", ValueOrDbNull(item.EnhancementDuration?.TotalMilliseconds));
        command.Parameters.AddWithValue("$error_message", ValueOrDbNull(item.ErrorMessage));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TranscriptionHistoryItem>> ListRecentAsync(int limit, CancellationToken cancellationToken)
    {
        if (limit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), limit, "Limit must be non-negative.");
        }

        if (limit == 0)
        {
            return [];
        }

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, created_at, text, provider_name, audio_duration_ms, transcription_duration_ms,
                   COALESCE(NULLIF(original_text, ''), text), enhanced_text, status, language,
                   model_path, prompt_name, enhancement_duration_ms, error_message
            FROM transcriptions
            ORDER BY created_at_utc_ticks DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit", limit);

        var items = new List<TranscriptionHistoryItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(ReadHistoryItem(reader));
        }

        return items;
    }

    public async Task<IReadOnlyList<TranscriptionHistoryItem>> SearchAsync(
        string query,
        int limit,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await ListRecentAsync(limit, cancellationToken);
        }

        if (limit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), limit, "Limit must be non-negative.");
        }

        if (limit == 0)
        {
            return [];
        }

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, created_at, text, provider_name, audio_duration_ms, transcription_duration_ms,
                   COALESCE(NULLIF(original_text, ''), text), enhanced_text, status, language,
                   model_path, prompt_name, enhancement_duration_ms, error_message
            FROM transcriptions
            WHERE text LIKE $query ESCAPE '\'
               OR original_text LIKE $query ESCAPE '\'
               OR enhanced_text LIKE $query ESCAPE '\'
               OR provider_name LIKE $query ESCAPE '\'
               OR language LIKE $query ESCAPE '\'
               OR model_path LIKE $query ESCAPE '\'
               OR prompt_name LIKE $query ESCAPE '\'
               OR error_message LIKE $query ESCAPE '\'
            ORDER BY created_at_utc_ticks DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$query", $"%{EscapeLikePattern(query.Trim())}%");
        command.Parameters.AddWithValue("$limit", limit);

        var items = new List<TranscriptionHistoryItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(ReadHistoryItem(reader));
        }

        return items;
    }

    public async Task<TranscriptionHistoryItem?> GetLatestCompletedAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, created_at, text, provider_name, audio_duration_ms, transcription_duration_ms,
                   COALESCE(NULLIF(original_text, ''), text), enhanced_text, status, language,
                   model_path, prompt_name, enhancement_duration_ms, error_message
            FROM transcriptions
            WHERE status = 'completed'
            ORDER BY created_at_utc_ticks DESC
            LIMIT 1;
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? ReadHistoryItem(reader)
            : null;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM transcriptions WHERE id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString());

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private static TranscriptionHistoryItem ReadHistoryItem(SqliteDataReader reader) =>
        new(
            Guid.Parse(reader.GetString(0)),
            DateTimeOffset.ParseExact(reader.GetString(1), "O", CultureInfo.InvariantCulture),
            reader.GetString(2),
            reader.GetString(3),
            TimeSpan.FromMilliseconds(reader.GetDouble(4)),
            TimeSpan.FromMilliseconds(reader.GetDouble(5)),
            originalText: reader.GetString(6),
            enhancedText: GetNullableString(reader, 7),
            status: ParseStatus(reader.GetString(8)),
            language: reader.GetString(9),
            modelPath: GetNullableString(reader, 10),
            promptName: GetNullableString(reader, 11),
            enhancementDuration: GetNullableTimeSpan(reader, 12),
            errorMessage: GetNullableString(reader, 13));

    private void EnsureDatabase()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS transcriptions (
                id TEXT PRIMARY KEY,
                created_at TEXT NOT NULL,
                created_at_utc_ticks INTEGER NOT NULL,
                text TEXT NOT NULL,
                original_text TEXT NOT NULL DEFAULT '',
                enhanced_text TEXT NULL,
                status TEXT NOT NULL DEFAULT 'completed',
                provider_name TEXT NOT NULL,
                language TEXT NOT NULL DEFAULT 'auto',
                model_path TEXT NULL,
                prompt_name TEXT NULL,
                audio_duration_ms REAL NOT NULL,
                transcription_duration_ms REAL NOT NULL,
                enhancement_duration_ms REAL NULL,
                error_message TEXT NULL
            );
            CREATE INDEX IF NOT EXISTS idx_transcriptions_created_at_utc_ticks
                ON transcriptions(created_at_utc_ticks DESC);
            """;
        command.ExecuteNonQuery();

        EnsureColumn(connection, "original_text", "original_text TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "enhanced_text", "enhanced_text TEXT NULL");
        EnsureColumn(connection, "status", "status TEXT NOT NULL DEFAULT 'completed'");
        EnsureColumn(connection, "language", "language TEXT NOT NULL DEFAULT 'auto'");
        EnsureColumn(connection, "model_path", "model_path TEXT NULL");
        EnsureColumn(connection, "prompt_name", "prompt_name TEXT NULL");
        EnsureColumn(connection, "enhancement_duration_ms", "enhancement_duration_ms REAL NULL");
        EnsureColumn(connection, "error_message", "error_message TEXT NULL");
    }

    private static void EnsureColumn(SqliteConnection connection, string columnName, string definition)
    {
        if (ColumnExists(connection, columnName))
        {
            return;
        }

        using var command = connection.CreateCommand();
        command.CommandText = $"ALTER TABLE transcriptions ADD COLUMN {definition};";
        command.ExecuteNonQuery();
    }

    private static bool ColumnExists(SqliteConnection connection, string columnName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info(transcriptions);";
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

    private static object ValueOrDbNull(string? value) =>
        string.IsNullOrEmpty(value) ? DBNull.Value : value;

    private static object ValueOrDbNull(double? value) =>
        value is null ? DBNull.Value : value.Value;

    private static string EscapeLikePattern(string value) =>
        value
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal);

    private static string? GetNullableString(SqliteDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);

    private static TimeSpan? GetNullableTimeSpan(SqliteDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? null : TimeSpan.FromMilliseconds(reader.GetDouble(ordinal));

    private static string StatusToStorage(TranscriptionHistoryStatus status) =>
        status switch
        {
            TranscriptionHistoryStatus.Pending => "pending",
            TranscriptionHistoryStatus.Completed => "completed",
            TranscriptionHistoryStatus.Failed => "failed",
            TranscriptionHistoryStatus.Canceled => "canceled",
            _ => "completed"
        };

    private static TranscriptionHistoryStatus ParseStatus(string value) =>
        Enum.TryParse<TranscriptionHistoryStatus>(value, ignoreCase: true, out var status)
            ? status
            : TranscriptionHistoryStatus.Completed;
}
