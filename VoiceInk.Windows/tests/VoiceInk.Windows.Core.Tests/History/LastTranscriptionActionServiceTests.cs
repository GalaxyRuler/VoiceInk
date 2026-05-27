using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Services;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.History;

public sealed class LastTranscriptionActionServiceTests
{
    [Fact]
    public async Task PasteLastAsync_InsertsNewestCompletedFinalText()
    {
        var newest = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "final cleaned",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero,
            originalText: "raw original");
        var history = new FakeHistoryStore([newest]);
        var insertion = new FakeTextInjectionService();
        var service = new LastTranscriptionActionService(history, insertion);

        var result = await service.PasteLastAsync(LastTranscriptionTextKind.Final, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Last transcription pasted", result.Message);
        Assert.Equal("final cleaned", insertion.InsertedText);
    }

    [Fact]
    public async Task PasteLastAsync_InsertsNewestCompletedEnhancedTextWhenAvailable()
    {
        var item = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "final cleaned",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero,
            enhancedText: "enhanced text");
        var insertion = new FakeTextInjectionService();
        var service = new LastTranscriptionActionService(new FakeHistoryStore([item]), insertion);

        var result = await service.PasteLastAsync(LastTranscriptionTextKind.EnhancedPreferred, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Last enhanced transcription pasted", result.Message);
        Assert.Equal("enhanced text", insertion.InsertedText);
    }

    [Fact]
    public async Task PasteLastAsync_FallsBackToFinalTextWhenEnhancedIsMissing()
    {
        var item = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "final cleaned",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero);
        var insertion = new FakeTextInjectionService();
        var service = new LastTranscriptionActionService(new FakeHistoryStore([item]), insertion);

        var result = await service.PasteLastAsync(LastTranscriptionTextKind.EnhancedPreferred, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Last transcription pasted", result.Message);
        Assert.Equal("final cleaned", insertion.InsertedText);
    }

    [Fact]
    public async Task PasteLastAsync_ReturnsErrorWhenNoCompletedTranscriptionExists()
    {
        var service = new LastTranscriptionActionService(new FakeHistoryStore([]), new FakeTextInjectionService());

        var result = await service.PasteLastAsync(LastTranscriptionTextKind.Final, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("No transcription available", result.Message);
    }

    [Fact]
    public async Task PasteLastAsync_SkipsFailedAndCanceledItems()
    {
        var failed = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "failed",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero,
            status: TranscriptionHistoryStatus.Failed);
        var completed = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(-1),
            "completed",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero);
        var insertion = new FakeTextInjectionService();
        var service = new LastTranscriptionActionService(new FakeHistoryStore([failed, completed]), insertion);

        var result = await service.PasteLastAsync(LastTranscriptionTextKind.Final, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("completed", insertion.InsertedText);
    }

    [Fact]
    public async Task PasteLastAsync_UsesLatestCompletedHistoryQuery()
    {
        var completed = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "completed",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero);
        var history = new FakeHistoryStore(
            Enumerable.Range(0, 12)
                .Select(index => new TranscriptionHistoryItem(
                    Guid.NewGuid(),
                    DateTimeOffset.UtcNow.AddMinutes(index),
                    $"failed {index}",
                    "local-whisper",
                    TimeSpan.Zero,
                    TimeSpan.Zero,
                    status: TranscriptionHistoryStatus.Failed))
                .Prepend(completed)
                .ToArray())
        {
            LatestCompleted = completed
        };
        var insertion = new FakeTextInjectionService();
        var service = new LastTranscriptionActionService(history, insertion);

        await service.PasteLastAsync(LastTranscriptionTextKind.Final, CancellationToken.None);

        Assert.True(history.GetLatestCompletedCalled);
        Assert.Equal("completed", insertion.InsertedText);
    }

    [Fact]
    public async Task PasteLastAsync_PreparesTargetAfterSelectingTextAndBeforeInsertion()
    {
        var events = new List<string>();
        var item = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "completed",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero);
        var insertion = new FakeTextInjectionService(events);
        var service = new LastTranscriptionActionService(new FakeHistoryStore([item]), insertion);

        await service.PasteLastAsync(
            LastTranscriptionTextKind.Final,
            CancellationToken.None,
            _ =>
            {
                events.Add("prepare");
                return Task.CompletedTask;
            });

        Assert.Equal(["prepare", "insert"], events);
    }

    [Fact]
    public async Task PasteLastAsync_DoesNotPrepareTargetWhenNoCompletedTranscriptionExists()
    {
        var prepared = false;
        var service = new LastTranscriptionActionService(new FakeHistoryStore([]), new FakeTextInjectionService());

        await service.PasteLastAsync(
            LastTranscriptionTextKind.Final,
            CancellationToken.None,
            _ =>
            {
                prepared = true;
                return Task.CompletedTask;
            });

        Assert.False(prepared);
    }

    private sealed class FakeHistoryStore(IReadOnlyList<TranscriptionHistoryItem> items) : IHistoryStore
    {
        public TranscriptionHistoryItem? LatestCompleted { get; init; }
        public bool GetLatestCompletedCalled { get; private set; }

        public Task SaveAsync(TranscriptionHistoryItem item, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<TranscriptionHistoryItem>> ListRecentAsync(
            int limit,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(items.Take(limit).ToArray() as IReadOnlyList<TranscriptionHistoryItem>);
        }

        public Task<IReadOnlyList<TranscriptionHistoryItem>> SearchAsync(
            string query,
            int limit,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(items.Take(limit).ToArray() as IReadOnlyList<TranscriptionHistoryItem>);
        }

        public Task<HistoryPage> ListPageAsync(
            string? query,
            HistoryPageCursor? cursor,
            int pageSize,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HistoryPage(items.Take(pageSize).ToArray(), NextCursor: null, HasMore: false));
        }

        public Task<TranscriptionHistoryItem?> GetLatestCompletedAsync(CancellationToken cancellationToken)
        {
            GetLatestCompletedCalled = true;
            return Task.FromResult(
                LatestCompleted ??
                items.FirstOrDefault(item => item.Status == TranscriptionHistoryStatus.Completed));
        }

        public Task<TranscriptionHistoryItem?> GetLatestCompletedWithAudioAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(
                items.FirstOrDefault(item =>
                    item.Status == TranscriptionHistoryStatus.Completed
                    && !string.IsNullOrWhiteSpace(item.AudioFilePath)));
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<IReadOnlyList<TranscriptionHistoryItem>> ListOlderThanAsync(
            DateTimeOffset cutoff,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TranscriptionHistoryItem>>(
                items.Where(item => item.CreatedAt.UtcDateTime.Ticks < cutoff.UtcDateTime.Ticks).ToArray());

        public Task<int> ClearAudioFilePathAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken) =>
            Task.FromResult(0);
    }

    private sealed class FakeTextInjectionService(List<string>? events = null) : ITextInjectionService
    {
        public string? InsertedText { get; private set; }

        public Task InsertAsync(string text, CancellationToken cancellationToken)
        {
            InsertedText = text;
            events?.Add("insert");
            return Task.CompletedTask;
        }
    }
}
