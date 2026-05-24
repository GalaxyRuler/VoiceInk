using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Text;
using VoiceInk.Windows.Core.Transcription;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.History;

public sealed class HistoryRetryServiceTests
{
    [Fact]
    public async Task RetryAsync_ReturnsFailureWhenAudioPathMissing()
    {
        var service = new HistoryRetryService(
            new FakeTranscriptionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings { ModelPath = "ggml-base.en.bin" }));
        var source = HistoryItem(audioFilePath: null);

        var result = await service.RetryAsync(source, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Audio file not found", result.Message);
    }

    [Fact]
    public async Task RetryAsync_ReturnsFailureWhenAudioFileDoesNotExist()
    {
        var service = new HistoryRetryService(
            new FakeTranscriptionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings { ModelPath = "ggml-base.en.bin" }));
        var source = HistoryItem(Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.wav"));

        var result = await service.RetryAsync(source, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Audio file not found", result.Message);
    }

    [Fact]
    public async Task RetryAsync_ReturnsFailureWhenModelPathMissing()
    {
        using var audio = new TempAudioFile();
        var transcription = new FakeTranscriptionService();
        var history = new FakeHistoryStore();
        var service = new HistoryRetryService(
            transcription,
            history,
            new FakeSettingsStore(new AppSettings()));

        var result = await service.RetryAsync(HistoryItem(audio.Path), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Local whisper model path is required.", result.Message);
        Assert.Equal(0, transcription.CallCount);
        Assert.Empty(history.Items);
    }

    [Fact]
    public async Task RetryAsync_ReturnsFailureWhenCloudProviderConfigurationMissing()
    {
        using var audio = new TempAudioFile();
        var transcription = new FakeTranscriptionService();
        var history = new FakeHistoryStore();
        var service = new HistoryRetryService(
            transcription,
            history,
            new FakeSettingsStore(new AppSettings
            {
                TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
                CloudTranscriptionEndpoint = "https://api.example.test/v1/audio/transcriptions"
            }));

        var result = await service.RetryAsync(HistoryItem(audio.Path), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Cloud transcription endpoint and model are required.", result.Message);
        Assert.Equal(0, transcription.CallCount);
        Assert.Empty(history.Items);
    }

    [Fact]
    public async Task RetryAsync_UsesCloudProviderSettingsWithoutLocalModelPath()
    {
        using var audio = new TempAudioFile();
        var transcription = new FakeTranscriptionService(new TranscriptionResult(
            " hello cloud ",
            TimeSpan.FromMilliseconds(150),
            "openai-compatible"));
        var history = new FakeHistoryStore();
        var dictionary = new FakeDictionaryStore
        {
            Vocabulary =
            [
                new VocabularyWord(Guid.NewGuid(), "VoiceInk", DateTimeOffset.UtcNow)
            ]
        };
        var service = new HistoryRetryService(
            transcription,
            history,
            new FakeSettingsStore(new AppSettings
            {
                TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
                CloudTranscriptionEndpoint = "https://api.example.test/v1/audio/transcriptions",
                CloudTranscriptionModel = "gpt-4o-transcribe",
                Language = "en"
            }),
            dictionary);

        var result = await service.RetryAsync(HistoryItem(audio.Path), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(transcription.LastOptions);
        Assert.Equal(TranscriptionProviderKind.OpenAICompatible, transcription.LastOptions.Provider);
        Assert.Equal("https://api.example.test/v1/audio/transcriptions", transcription.LastOptions.CloudEndpoint);
        Assert.Equal("gpt-4o-transcribe", transcription.LastOptions.CloudModel);
        Assert.Equal("en", transcription.LastOptions.Language);
        Assert.Equal("Important Vocabulary: VoiceInk", transcription.LastOptions.Prompt);
        Assert.Equal("openai-compatible", result.Item?.ProviderName);
        Assert.Equal("gpt-4o-transcribe", result.Item?.ModelPath);
    }

    [Fact]
    public async Task RetryAsync_TranscribesExistingAudioAndSavesNewHistoryItem()
    {
        using var audio = new TempAudioFile();
        var transcription = new FakeTranscriptionService(new TranscriptionResult(
            " hello world ",
            TimeSpan.FromMilliseconds(150),
            "local-whisper"));
        var history = new FakeHistoryStore();
        var service = new HistoryRetryService(
            transcription,
            history,
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin",
                Language = "en"
            }));
        var source = HistoryItem(audio.Path) with
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            AudioDuration = TimeSpan.FromSeconds(4)
        };

        var result = await service.RetryAsync(source, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Retry transcription saved", result.Message);
        Assert.NotNull(result.Item);
        Assert.Same(result.Item, Assert.Single(history.Items));
        Assert.NotEqual(source.Id, result.Item.Id);
        Assert.Equal("hello world", result.Item.Text);
        Assert.Equal(" hello world ", result.Item.OriginalText);
        Assert.Equal("local-whisper", result.Item.ProviderName);
        Assert.Equal(TimeSpan.FromSeconds(4), result.Item.AudioDuration);
        Assert.Equal(TimeSpan.FromMilliseconds(150), result.Item.TranscriptionDuration);
        Assert.Equal("ggml-base.en.bin", result.Item.ModelPath);
        Assert.Equal("en", result.Item.Language);
        Assert.Equal(audio.Path, result.Item.AudioFilePath);
        Assert.Equal(audio.Path, transcription.LastAudio?.FilePath);
        Assert.Equal("ggml-base.en.bin", transcription.LastOptions?.ModelPath);
        Assert.Equal("en", transcription.LastOptions?.Language);
    }

    [Fact]
    public async Task RetryAsync_AppliesDictionaryAndCleanupSettings()
    {
        using var audio = new TempAudioFile();
        var transcription = new FakeTranscriptionService(new TranscriptionResult(
            "Voice ink!",
            TimeSpan.FromMilliseconds(150),
            "local-whisper"));
        var history = new FakeHistoryStore();
        var dictionary = new FakeDictionaryStore
        {
            Vocabulary =
            [
                new VocabularyWord(Guid.NewGuid(), "VoiceInk", DateTimeOffset.UtcNow)
            ],
            Replacements =
            [
                new WordReplacement(Guid.NewGuid(), "Voice ink", "VoiceInk!", DateTimeOffset.UtcNow)
            ]
        };
        var service = new HistoryRetryService(
            transcription,
            history,
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin",
                Language = "en",
                RemoveFillerWords = false,
                PunctuationCleanupMode = PunctuationCleanupMode.RemoveAll,
                LowercaseTranscription = true
            }),
            dictionary);

        var result = await service.RetryAsync(HistoryItem(audio.Path), CancellationToken.None);

        Assert.True(result.Success);
        var saved = Assert.Single(history.Items);
        Assert.Equal("voiceink", saved.Text);
        Assert.Equal("Voice ink!", saved.OriginalText);
        Assert.Equal("Important Vocabulary: VoiceInk", transcription.LastOptions?.Prompt);
    }

    [Fact]
    public async Task RetryLatestAsync_ReturnsFailureWhenNoCompletedTranscriptionExists()
    {
        var service = new HistoryRetryService(
            new FakeTranscriptionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings { ModelPath = "ggml-base.en.bin" }));

        var result = await service.RetryLatestAsync(CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("No transcription available", result.Message);
    }

    [Fact]
    public async Task RetryLatestAsync_RetriesLatestCompletedHistoryItem()
    {
        using var audio = new TempAudioFile();
        var history = new FakeHistoryStore();
        var source = HistoryItem(audio.Path) with
        {
            Status = TranscriptionHistoryStatus.Completed,
            AudioDuration = TimeSpan.FromSeconds(7)
        };
        history.Items.Add(source);
        var transcription = new FakeTranscriptionService(new TranscriptionResult(
            " retried text ",
            TimeSpan.FromMilliseconds(50),
            "local-whisper"));
        var service = new HistoryRetryService(
            transcription,
            history,
            new FakeSettingsStore(new AppSettings { ModelPath = "ggml-base.en.bin" }));

        var result = await service.RetryLatestAsync(CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Retry transcription saved", result.Message);
        Assert.NotNull(result.Item);
        Assert.Equal("retried text", result.Item.Text);
        Assert.Equal(audio.Path, result.Item.AudioFilePath);
        Assert.Equal(TimeSpan.FromSeconds(7), transcription.LastAudio?.Duration);
    }

    [Fact]
    public async Task RetryLatestAsync_SkipsNewerCompletedHistoryItemsWithoutAudioPath()
    {
        using var audio = new TempAudioFile();
        var history = new FakeHistoryStore();
        history.Items.Add(HistoryItem(audioFilePath: null) with
        {
            Text = "newer text-only transcription",
            Status = TranscriptionHistoryStatus.Completed
        });
        history.Items.Add(HistoryItem(audio.Path) with
        {
            Text = "older retryable transcription",
            Status = TranscriptionHistoryStatus.Completed
        });
        var service = new HistoryRetryService(
            new FakeTranscriptionService(new TranscriptionResult(
                "retryable text",
                TimeSpan.FromMilliseconds(50),
                "local-whisper")),
            history,
            new FakeSettingsStore(new AppSettings { ModelPath = "ggml-base.en.bin" }));

        var result = await service.RetryLatestAsync(CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(audio.Path, result.Item?.AudioFilePath);
    }

    private static TranscriptionHistoryItem HistoryItem(string? audioFilePath) =>
        new(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "source text",
            "local-whisper",
            TimeSpan.FromSeconds(1),
            TimeSpan.FromMilliseconds(100),
            audioFilePath: audioFilePath);

    private sealed class TempAudioFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"voiceink-retry-{Guid.NewGuid():N}.wav");

        public TempAudioFile()
        {
            File.WriteAllBytes(Path, [0, 1, 2, 3]);
        }

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }

    private sealed class FakeTranscriptionService(
        TranscriptionResult? result = null) : ITranscriptionService
    {
        public int CallCount { get; private set; }
        public AudioCaptureResult? LastAudio { get; private set; }
        public TranscriptionOptions? LastOptions { get; private set; }

        public Task<TranscriptionResult> TranscribeAsync(
            AudioCaptureResult audio,
            TranscriptionOptions options,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastAudio = audio;
            LastOptions = options;
            return Task.FromResult(result ?? new TranscriptionResult("retry text", TimeSpan.Zero, "local-whisper"));
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
