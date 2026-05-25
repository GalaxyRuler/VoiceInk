using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Core.Metrics;

public sealed class DisabledSessionMetricStore : ISessionMetricStore
{
    public static DisabledSessionMetricStore Instance { get; } = new();

    private DisabledSessionMetricStore()
    {
    }

    public Task SaveAsync(SessionMetric metric, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task<bool> HasTranscriptionAsync(Guid transcriptionId, CancellationToken cancellationToken) =>
        Task.FromResult(true);

    public Task<SessionMetricsSummary> GetSummaryAsync(CancellationToken cancellationToken) =>
        Task.FromResult(SessionMetricsSummary.Empty);

    public Task<IReadOnlyList<ModelPerformanceStat>> ListTranscriptionModelPerformanceAsync(
        DateTimeOffset? since,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ModelPerformanceStat>>([]);

    public Task<IReadOnlyList<ModelPerformanceStat>> ListEnhancementModelPerformanceAsync(
        DateTimeOffset? since,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ModelPerformanceStat>>([]);
}
