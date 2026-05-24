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

    private sealed class FakeHistoryStore(IReadOnlyList<TranscriptionHistoryItem> items) : IHistoryStore
    {
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
    }

    private sealed class FakeTextInjectionService : ITextInjectionService
    {
        public string? InsertedText { get; private set; }

        public Task InsertAsync(string text, CancellationToken cancellationToken)
        {
            InsertedText = text;
            return Task.CompletedTask;
        }
    }
}
