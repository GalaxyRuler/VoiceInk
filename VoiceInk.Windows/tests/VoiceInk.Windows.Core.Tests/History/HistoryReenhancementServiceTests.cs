using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.History;

public sealed class HistoryReenhancementServiceTests
{
    [Fact]
    public async Task ReenhanceAsync_EnhancesOriginalTextAndSavesNewHistoryItem()
    {
        var enhancement = new FakeTextEnhancementService("Fresh enhanced text.");
        var history = new FakeHistoryStore();
        var dictionary = new FakeDictionaryStore
        {
            Vocabulary =
            [
                new VocabularyWord(Guid.NewGuid(), "VoiceInk", DateTimeOffset.UtcNow)
            ]
        };
        var service = new HistoryReenhancementService(
            history,
            new FakeSettingsStore(ConfiguredSettings()),
            new TextEnhancementPipeline(enhancement),
            dictionary);
        var source = HistoryItem() with
        {
            Text = "old enhanced text",
            OriginalText = "raw transcript text",
            EnhancedText = "old enhanced text",
            PromptName = "Old prompt",
            EnhancementProviderName = "old-provider",
            EnhancementModelName = "old-model",
            EnhancementDuration = TimeSpan.FromMilliseconds(10),
            AudioFilePath = "C:\\Recordings\\source.wav",
            PowerModeName = "Notes",
            PowerModeEmoji = "N"
        };

        var result = await service.ReenhanceAsync(source, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Re-enhanced transcription saved", result.Message);
        var saved = Assert.Single(history.Items);
        Assert.Same(saved, result.Item);
        Assert.NotEqual(source.Id, saved.Id);
        Assert.True(saved.CreatedAt > source.CreatedAt);
        Assert.Equal("Fresh enhanced text.", saved.Text);
        Assert.Equal("raw transcript text", saved.OriginalText);
        Assert.Equal("Fresh enhanced text.", saved.EnhancedText);
        Assert.Equal(source.ProviderName, saved.ProviderName);
        Assert.Equal(source.AudioDuration, saved.AudioDuration);
        Assert.Equal(source.TranscriptionDuration, saved.TranscriptionDuration);
        Assert.Equal(source.Language, saved.Language);
        Assert.Equal(source.ModelPath, saved.ModelPath);
        Assert.Equal(source.AudioFilePath, saved.AudioFilePath);
        Assert.Equal(source.PowerModeName, saved.PowerModeName);
        Assert.Equal(source.PowerModeEmoji, saved.PowerModeEmoji);
        Assert.Contains("raw transcript text", enhancement.LastRequest?.UserMessage);
        Assert.Contains("VoiceInk", enhancement.LastRequest?.SystemMessage);
        Assert.Equal("openai-compatible", saved.EnhancementProviderName);
        Assert.Equal("test-model", saved.EnhancementModelName);
        Assert.Equal(TimeSpan.FromMilliseconds(42), saved.EnhancementDuration);
        Assert.NotNull(saved.AiRequestSystemMessage);
        Assert.Contains("raw transcript text", saved.AiRequestUserMessage);
    }

    [Fact]
    public async Task ReenhanceAsync_UsesFinalTextWhenOriginalTextIsBlank()
    {
        var enhancement = new FakeTextEnhancementService("Enhanced fallback.");
        var history = new FakeHistoryStore();
        var service = new HistoryReenhancementService(
            history,
            new FakeSettingsStore(ConfiguredSettings()),
            new TextEnhancementPipeline(enhancement));
        var source = HistoryItem() with
        {
            Text = "final text",
            OriginalText = " "
        };

        var result = await service.ReenhanceAsync(source, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Contains("final text", enhancement.LastRequest?.UserMessage);
        Assert.Equal("final text", Assert.Single(history.Items).OriginalText);
    }

    [Fact]
    public async Task ReenhanceAsync_ReturnsFailureWhenEnhancementDisabled()
    {
        var enhancement = new FakeTextEnhancementService("unused");
        var history = new FakeHistoryStore();
        var service = new HistoryReenhancementService(
            history,
            new FakeSettingsStore(ConfiguredSettings() with { IsEnhancementEnabled = false }),
            new TextEnhancementPipeline(enhancement));

        var result = await service.ReenhanceAsync(HistoryItem(), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("AI enhancement is disabled", result.Message);
        Assert.Equal(0, enhancement.CallCount);
        Assert.Empty(history.Items);
    }

    [Fact]
    public async Task ReenhanceAsync_ReturnsFailureWhenProviderConfigurationMissing()
    {
        var enhancement = new FakeTextEnhancementService("unused");
        var history = new FakeHistoryStore();
        var service = new HistoryReenhancementService(
            history,
            new FakeSettingsStore(ConfiguredSettings() with { EnhancementEndpoint = string.Empty }),
            new TextEnhancementPipeline(enhancement));

        var result = await service.ReenhanceAsync(HistoryItem(), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("AI enhancement provider is not configured.", result.Message);
        Assert.Equal(0, enhancement.CallCount);
        Assert.Empty(history.Items);
    }

    [Theory]
    [InlineData(TranscriptionHistoryStatus.Failed)]
    [InlineData(TranscriptionHistoryStatus.Canceled)]
    public async Task ReenhanceAsync_ReturnsFailureWhenSourceIsNotCompleted(TranscriptionHistoryStatus status)
    {
        var history = new FakeHistoryStore();
        var service = new HistoryReenhancementService(
            history,
            new FakeSettingsStore(ConfiguredSettings()),
            new TextEnhancementPipeline(new FakeTextEnhancementService("unused")));

        var result = await service.ReenhanceAsync(HistoryItem() with { Status = status }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Only completed transcriptions can be re-enhanced", result.Message);
        Assert.Empty(history.Items);
    }

    private static TranscriptionHistoryItem HistoryItem() =>
        new(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(-5),
            "source text",
            "local-whisper",
            TimeSpan.FromSeconds(5),
            TimeSpan.FromMilliseconds(500),
            originalText: "source text",
            status: TranscriptionHistoryStatus.Completed,
            language: "en",
            modelPath: "ggml-base.en.bin");

    private static AppSettings ConfiguredSettings() =>
        new()
        {
            IsEnhancementEnabled = true,
            EnhancementEndpoint = "https://example.test/v1/chat/completions",
            EnhancementModel = "test-model",
            EnhancementTimeoutSeconds = 7,
            EnhancementRetryOnTimeout = true,
            SkipShortEnhancement = false
        };

    private sealed class FakeTextEnhancementService(string text) : ITextEnhancementService
    {
        public int CallCount { get; private set; }
        public TextEnhancementRequest? LastRequest { get; private set; }

        public Task<TextEnhancementResult> EnhanceAsync(
            TextEnhancementRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequest = request;
            return Task.FromResult(new TextEnhancementResult(
                text,
                "openai-compatible",
                request.Model,
                TimeSpan.FromMilliseconds(42)));
        }
    }

    private sealed class FakeHistoryStore : IHistoryStore
    {
        public List<TranscriptionHistoryItem> Items { get; } = [];

        public Task SaveAsync(TranscriptionHistoryItem item, CancellationToken cancellationToken)
        {
            Items.Add(item);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TranscriptionHistoryItem>> ListRecentAsync(
            int limit,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TranscriptionHistoryItem>>(Items.Take(limit).ToArray());

        public Task<IReadOnlyList<TranscriptionHistoryItem>> SearchAsync(
            string query,
            int limit,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TranscriptionHistoryItem>>(Items.Take(limit).ToArray());

        public Task<TranscriptionHistoryItem?> GetLatestCompletedAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Items.FirstOrDefault(item => item.Status == TranscriptionHistoryStatus.Completed));

        public Task<TranscriptionHistoryItem?> GetLatestCompletedWithAudioAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Items.FirstOrDefault(item =>
                item.Status == TranscriptionHistoryStatus.Completed
                && !string.IsNullOrWhiteSpace(item.AudioFilePath)));

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Items.RemoveAll(item => item.Id == id) > 0);

        public Task<IReadOnlyList<TranscriptionHistoryItem>> ListOlderThanAsync(
            DateTimeOffset cutoff,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TranscriptionHistoryItem>>(
                Items
                    .Where(item => item.CreatedAt.UtcDateTime.Ticks < cutoff.UtcDateTime.Ticks)
                    .ToArray());

        public Task<int> ClearAudioFilePathAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken)
        {
            var idSet = ids.ToHashSet();
            var count = 0;
            for (var index = 0; index < Items.Count; index++)
            {
                if (!idSet.Contains(Items[index].Id) || Items[index].AudioFilePath is null)
                {
                    continue;
                }

                Items[index] = Items[index] with { AudioFilePath = null };
                count++;
            }

            return Task.FromResult(count);
        }
    }

    private sealed class FakeSettingsStore(AppSettings settings) : ISettingsStore
    {
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken) =>
            Task.FromResult(settings);

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FakeDictionaryStore : IDictionaryStore
    {
        public IReadOnlyList<VocabularyWord> Vocabulary { get; init; } = [];
        public IReadOnlyList<WordReplacement> Replacements { get; init; } = [];

        public Task<IReadOnlyList<VocabularyWord>> ListVocabularyAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Vocabulary);

        public Task<IReadOnlyList<WordReplacement>> ListReplacementsAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Replacements);
    }
}
