using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Metrics;
using VoiceInk.Windows.Core.Services;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Metrics;

public sealed class SessionMetricRecorderTests
{
    [Fact]
    public async Task RecordAsync_SavesMetricForCompletedHistoryItem()
    {
        var store = new FakeSessionMetricStore();
        var item = CompletedHistoryItem() with
        {
            Text = "hello windows",
            AudioDuration = TimeSpan.FromSeconds(10),
            TranscriptionDuration = TimeSpan.FromSeconds(2),
            ModelPath = "ggml-base.en.bin",
            PowerModeName = "Docs"
        };

        var result = await SessionMetricRecorder.RecordAsync(
            item,
            store,
            "recorder",
            CancellationToken.None);

        Assert.True(result.Recorded);
        Assert.Null(result.WarningMessage);
        var metric = Assert.Single(store.Saved);
        Assert.Equal(item.Id, metric.TranscriptionId);
        Assert.Equal(item.CreatedAt, metric.Timestamp);
        Assert.Equal("recorder", metric.Source);
        Assert.Equal(2, metric.WordCount);
        Assert.Equal(TimeSpan.FromSeconds(10), metric.AudioDuration);
        Assert.Equal("ggml-base.en.bin", metric.TranscriptionModelName);
        Assert.Equal(TimeSpan.FromSeconds(2), metric.TranscriptionDuration);
        Assert.Equal(5, metric.SpeedFactor);
        Assert.Equal("Docs", metric.PowerModeName);
    }

    [Theory]
    [InlineData(TranscriptionHistoryStatus.Canceled)]
    [InlineData(TranscriptionHistoryStatus.Failed)]
    [InlineData(TranscriptionHistoryStatus.Pending)]
    public async Task RecordAsync_IgnoresNonCompletedHistoryItems(TranscriptionHistoryStatus status)
    {
        var store = new FakeSessionMetricStore();
        var item = CompletedHistoryItem() with { Status = status };

        var result = await SessionMetricRecorder.RecordAsync(
            item,
            store,
            "recorder",
            CancellationToken.None);

        Assert.False(result.Recorded);
        Assert.Null(result.WarningMessage);
        Assert.Empty(store.Saved);
        Assert.Equal(0, store.HasTranscriptionCallCount);
    }

    [Fact]
    public async Task RecordAsync_CountsEnhancedTextWhenEnhancementRan()
    {
        var store = new FakeSessionMetricStore();
        var item = CompletedHistoryItem() with
        {
            Text = "rough text",
            EnhancedText = "polished text with more words",
            EnhancementDuration = TimeSpan.FromSeconds(3),
            EnhancementModelName = "gpt-4o-mini"
        };

        await SessionMetricRecorder.RecordAsync(item, store, "audio-file", CancellationToken.None);

        var metric = Assert.Single(store.Saved);
        Assert.Equal("audio-file", metric.Source);
        Assert.Equal(5, metric.WordCount);
        Assert.Equal("gpt-4o-mini", metric.AiEnhancementModelName);
        Assert.Equal(TimeSpan.FromSeconds(3), metric.EnhancementDuration);
    }

    [Fact]
    public async Task RecordAsync_UsesFinalTextWhenEnhancedTextExistsWithoutEnhancementDuration()
    {
        var store = new FakeSessionMetricStore();
        var item = CompletedHistoryItem() with
        {
            Text = "count final text",
            EnhancedText = "do not count this longer text"
        };

        await SessionMetricRecorder.RecordAsync(item, store, "retry", CancellationToken.None);

        var metric = Assert.Single(store.Saved);
        Assert.Equal("retry", metric.Source);
        Assert.Equal(3, metric.WordCount);
    }

    [Fact]
    public async Task RecordAsync_SkipsDuplicateTranscriptionIds()
    {
        var store = new FakeSessionMetricStore { HasTranscriptionResult = true };

        var result = await SessionMetricRecorder.RecordAsync(
            CompletedHistoryItem(),
            store,
            "recorder",
            CancellationToken.None);

        Assert.False(result.Recorded);
        Assert.Null(result.WarningMessage);
        Assert.Empty(store.Saved);
        Assert.Equal(1, store.HasTranscriptionCallCount);
    }

    [Fact]
    public async Task RecordAsync_SkipsDisabledSessionMetricStoreWithoutWarning()
    {
        var result = await SessionMetricRecorder.RecordAsync(
            CompletedHistoryItem(),
            DisabledSessionMetricStore.Instance,
            "recorder",
            CancellationToken.None);

        Assert.False(result.Recorded);
        Assert.Null(result.WarningMessage);
        Assert.Equal(SessionMetricsSummary.Empty, await DisabledSessionMetricStore.Instance.GetSummaryAsync(CancellationToken.None));
        Assert.Empty(await DisabledSessionMetricStore.Instance.ListTranscriptionModelPerformanceAsync(null, CancellationToken.None));
        Assert.Empty(await DisabledSessionMetricStore.Instance.ListEnhancementModelPerformanceAsync(null, CancellationToken.None));
    }

    [Fact]
    public async Task RecordAsync_ReturnsWarningWhenStoreFails()
    {
        var store = new FakeSessionMetricStore
        {
            SaveException = new InvalidOperationException("database locked")
        };

        var result = await SessionMetricRecorder.RecordAsync(
            CompletedHistoryItem(),
            store,
            "recorder",
            CancellationToken.None);

        Assert.False(result.Recorded);
        Assert.Equal("Metrics save failed: database locked", result.WarningMessage);
    }

    private static TranscriptionHistoryItem CompletedHistoryItem() =>
        new(
            Guid.NewGuid(),
            new DateTimeOffset(2026, 5, 25, 12, 0, 0, TimeSpan.Zero),
            "hello world",
            "local-whisper",
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(1),
            status: TranscriptionHistoryStatus.Completed,
            modelPath: "base.bin");

    private sealed class FakeSessionMetricStore : ISessionMetricStore
    {
        public List<SessionMetric> Saved { get; } = [];
        public bool HasTranscriptionResult { get; init; }
        public int HasTranscriptionCallCount { get; private set; }
        public Exception? SaveException { get; init; }

        public Task SaveAsync(SessionMetric metric, CancellationToken cancellationToken)
        {
            if (SaveException is not null)
            {
                throw SaveException;
            }

            Saved.Add(metric);
            return Task.CompletedTask;
        }

        public Task<bool> HasTranscriptionAsync(Guid transcriptionId, CancellationToken cancellationToken)
        {
            HasTranscriptionCallCount++;
            return Task.FromResult(HasTranscriptionResult);
        }

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
}
