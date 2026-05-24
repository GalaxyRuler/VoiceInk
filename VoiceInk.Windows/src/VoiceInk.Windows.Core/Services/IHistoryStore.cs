using VoiceInk.Windows.Core.History;

namespace VoiceInk.Windows.Core.Services;

public interface IHistoryStore
{
    Task SaveAsync(TranscriptionHistoryItem item, CancellationToken cancellationToken);
    Task<IReadOnlyList<TranscriptionHistoryItem>> ListRecentAsync(int limit, CancellationToken cancellationToken);
    Task<IReadOnlyList<TranscriptionHistoryItem>> SearchAsync(string query, int limit, CancellationToken cancellationToken);
    Task<TranscriptionHistoryItem?> GetLatestCompletedAsync(CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
