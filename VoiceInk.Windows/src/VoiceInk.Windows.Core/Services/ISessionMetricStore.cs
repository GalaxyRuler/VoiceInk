using VoiceInk.Windows.Core.Metrics;

namespace VoiceInk.Windows.Core.Services;

public interface ISessionMetricStore
{
    Task SaveAsync(SessionMetric metric, CancellationToken cancellationToken);
    Task<bool> HasTranscriptionAsync(Guid transcriptionId, CancellationToken cancellationToken);
    Task<SessionMetricsSummary> GetSummaryAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ModelPerformanceStat>> ListTranscriptionModelPerformanceAsync(
        DateTimeOffset? since,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<ModelPerformanceStat>> ListEnhancementModelPerformanceAsync(
        DateTimeOffset? since,
        CancellationToken cancellationToken);
}
