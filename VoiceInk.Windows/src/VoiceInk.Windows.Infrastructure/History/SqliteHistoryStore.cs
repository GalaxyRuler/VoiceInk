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

        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO transcriptions
                (id, created_at, text, provider_name, audio_duration_ms, transcription_duration_ms)
            VALUES
                ($id, $created_at, $text, $provider_name, $audio_duration_ms, $transcription_duration_ms);
            """;
        command.Parameters.AddWithValue("$id", item.Id.ToString());
        command.Parameters.AddWithValue("$created_at", item.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("$text", item.Text);
        command.Parameters.AddWithValue("$provider_name", item.ProviderName);
        command.Parameters.AddWithValue("$audio_duration_ms", item.AudioDuration.TotalMilliseconds);
        command.Parameters.AddWithValue("$transcription_duration_ms", item.TranscriptionDuration.TotalMilliseconds);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TranscriptionHistoryItem>> ListRecentAsync(int limit, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, created_at, text, provider_name, audio_duration_ms, transcription_duration_ms
            FROM transcriptions
            ORDER BY created_at DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit", limit);

        var items = new List<TranscriptionHistoryItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new TranscriptionHistoryItem(
                Guid.Parse(reader.GetString(0)),
                DateTimeOffset.Parse(reader.GetString(1)),
                reader.GetString(2),
                reader.GetString(3),
                TimeSpan.FromMilliseconds(reader.GetDouble(4)),
                TimeSpan.FromMilliseconds(reader.GetDouble(5))));
        }

        return items;
    }

    private void EnsureDatabase()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS transcriptions (
                id TEXT PRIMARY KEY,
                created_at TEXT NOT NULL,
                text TEXT NOT NULL,
                provider_name TEXT NOT NULL,
                audio_duration_ms REAL NOT NULL,
                transcription_duration_ms REAL NOT NULL
            );
            CREATE INDEX IF NOT EXISTS idx_transcriptions_created_at
                ON transcriptions(created_at DESC);
            """;
        command.ExecuteNonQuery();
    }
}
