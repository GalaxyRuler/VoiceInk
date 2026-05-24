using VoiceInk.Windows.Core.History;

namespace VoiceInk.Windows.Core.Services;

public interface IHistoryStore
{
    Task SaveAsync(TranscriptionHistoryItem item, CancellationToken cancellationToken);
    Task<IReadOnlyList<TranscriptionHistoryItem>> ListRecentAsync(int limit, CancellationToken cancellationToken);
}
