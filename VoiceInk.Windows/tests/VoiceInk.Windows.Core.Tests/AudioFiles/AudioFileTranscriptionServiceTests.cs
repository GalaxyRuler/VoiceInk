using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.AudioFiles;
using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Text;
using VoiceInk.Windows.Core.Transcription;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.AudioFiles;

public sealed class AudioFileTranscriptionServiceTests
{
    [Fact]
    public async Task TranscribeAsync_ReturnsFailureWhenModelPathMissing()
    {
        var importer = new FakeAudioFileImportService();
        var transcription = new FakeTranscriptionService();
        var history = new FakeHistoryStore();
        var service = new AudioFileTranscriptionService(
            importer,
            transcription,
            history,
            new FakeSettingsStore(new AppSettings()));

        var result = await service.TranscribeAsync("source.wav", "recordings", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Local whisper model path is required.", result.Message);
        Assert.Equal(0, importer.CallCount);
        Assert.Equal(0, transcription.CallCount);
        Assert.Empty(history.Items);
    }

    [Fact]
    public async Task TranscribeAsync_ReturnsFailureWhenCloudProviderConfigurationMissing()
    {
        var importer = new FakeAudioFileImportService();
        var transcription = new FakeTranscriptionService();
        var history = new FakeHistoryStore();
        var service = new AudioFileTranscriptionService(
            importer,
            transcription,
            history,
            new FakeSettingsStore(new AppSettings
            {
                TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
                CloudTranscriptionEndpoint = "https://api.example.test/v1/audio/transcriptions"
            }));

        var result = await service.TranscribeAsync("source.wav", "recordings", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Cloud transcription endpoint and model are required.", result.Message);
        Assert.Equal(0, importer.CallCount);
        Assert.Equal(0, transcription.CallCount);
        Assert.Empty(history.Items);
    }

    [Fact]
    public async Task TranscribeAsync_UsesCloudProviderSettingsWithoutLocalModelPath()
    {
        var importedAudio = new AudioCaptureResult("recordings\\imported.wav", TimeSpan.FromSeconds(12), 44100, 2);
        var importer = new FakeAudioFileImportService(importedAudio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult(
            "VoiceInk",
            TimeSpan.FromMilliseconds(250),
            "openai-compatible"));
        var history = new FakeHistoryStore();
        var dictionary = new FakeDictionaryStore
        {
            Vocabulary = [new VocabularyWord(Guid.NewGuid(), "VoiceInk", DateTimeOffset.UtcNow)]
        };
        var service = new AudioFileTranscriptionService(
            importer,
            transcription,
            history,
            new FakeSettingsStore(new AppSettings
            {
                TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
                CloudTranscriptionProviderId = "groq",
                CloudTranscriptionEndpoint = "https://api.groq.com/openai/v1/audio/transcriptions",
                CloudTranscriptionModel = "whisper-large-v3-turbo",
                Language = "en"
            }),
            dictionary);

        var result = await service.TranscribeAsync("source.mp3", "recordings", CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(transcription.LastOptions);
        Assert.Equal(TranscriptionProviderKind.OpenAICompatible, transcription.LastOptions.Provider);
        Assert.Equal("groq", transcription.LastOptions.CloudProviderId);
        Assert.Equal("https://api.groq.com/openai/v1/audio/transcriptions", transcription.LastOptions.CloudEndpoint);
        Assert.Equal("whisper-large-v3-turbo", transcription.LastOptions.CloudModel);
        Assert.Equal("en", transcription.LastOptions.Language);
        Assert.Equal("Important Vocabulary: VoiceInk", transcription.LastOptions.Prompt);
        Assert.Equal("groq", result.Item?.ProviderName);
        Assert.Equal("whisper-large-v3-turbo", result.Item?.ModelPath);
    }

    [Fact]
    public async Task TranscribeAsync_ImportsTranscribesCleansAndSavesHistory()
    {
        var importedAudio = new AudioCaptureResult("recordings\\imported.wav", TimeSpan.FromSeconds(12), 44100, 2);
        var importer = new FakeAudioFileImportService(importedAudio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult(
            "Voice ink!",
            TimeSpan.FromMilliseconds(250),
            "local-whisper"));
        var history = new FakeHistoryStore();
        var dictionary = new FakeDictionaryStore
        {
            Vocabulary = [new VocabularyWord(Guid.NewGuid(), "VoiceInk", DateTimeOffset.UtcNow)],
            Replacements = [new WordReplacement(Guid.NewGuid(), "Voice ink", "VoiceInk!", DateTimeOffset.UtcNow)]
        };
        var service = new AudioFileTranscriptionService(
            importer,
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

        var result = await service.TranscribeAsync("source.mp3", "recordings", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("File transcription saved", result.Message);
        Assert.NotNull(result.Item);
        Assert.Same(result.Item, Assert.Single(history.Items));
        Assert.Equal("voiceink", result.Item.Text);
        Assert.Equal("Voice ink!", result.Item.OriginalText);
        Assert.Equal("local-whisper", result.Item.ProviderName);
        Assert.Equal(TimeSpan.FromSeconds(12), result.Item.AudioDuration);
        Assert.Equal(TimeSpan.FromMilliseconds(250), result.Item.TranscriptionDuration);
        Assert.Equal("ggml-base.en.bin", result.Item.ModelPath);
        Assert.Equal("en", result.Item.Language);
        Assert.Equal("recordings\\imported.wav", result.Item.AudioFilePath);
        Assert.Equal("source.mp3", importer.LastSourcePath);
        Assert.Equal("recordings", importer.LastRecordingsDirectory);
        Assert.Equal(importedAudio, transcription.LastAudio);
        Assert.Equal("Important Vocabulary: VoiceInk", transcription.LastOptions?.Prompt);
        Assert.Null(result.Item.PowerModeName);
        Assert.Null(result.Item.PowerModeEmoji);
    }

    [Fact]
    public async Task TranscribeAsync_ReturnsFailureWhenFinalTextIsEmpty()
    {
        var service = new AudioFileTranscriptionService(
            new FakeAudioFileImportService(),
            new FakeTranscriptionService(new TranscriptionResult("   ", TimeSpan.Zero, "local-whisper")),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings { ModelPath = "ggml-base.en.bin" }));

        var result = await service.TranscribeAsync("source.wav", "recordings", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("File transcription produced no text", result.Message);
    }

    [Fact]
    public async Task TranscribeAsync_ReturnsFailureWhenImportOrTranscriptionFails()
    {
        var service = new AudioFileTranscriptionService(
            new FakeAudioFileImportService { ExceptionToThrow = new InvalidOperationException("Unsupported media") },
            new FakeTranscriptionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings { ModelPath = "ggml-base.en.bin" }));

        var result = await service.TranscribeAsync("source.mov", "recordings", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Unsupported media", result.Message);
    }

    [Fact]
    public async Task TranscribeAsync_SavesEnhancedTextAndEnhancementMetadata()
    {
        var importedAudio = new AudioCaptureResult("recordings\\imported.wav", TimeSpan.FromSeconds(12), 16000, 1);
        var importer = new FakeAudioFileImportService(importedAudio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult(
            "file text",
            TimeSpan.FromMilliseconds(250),
            "local-whisper"));
        var history = new FakeHistoryStore();
        var enhancement = new FakeTextEnhancementService("Enhanced file text.");
        var service = new AudioFileTranscriptionService(
            importer,
            transcription,
            history,
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin",
                EnhancementEndpoint = "https://example.test/v1/chat/completions",
                EnhancementModel = "test-model",
                IsEnhancementEnabled = true,
                SkipShortEnhancement = false
            }),
            enhancementPipeline: new TextEnhancementPipeline(enhancement));

        var result = await service.TranscribeAsync("source.mp3", "recordings", CancellationToken.None);

        Assert.True(result.Success);
        var saved = Assert.Single(history.Items);
        Assert.Equal("file text", saved.Text);
        Assert.Equal("Enhanced file text.", saved.EnhancedText);
        Assert.Equal("Default", saved.PromptName);
        Assert.Equal("openai-compatible", saved.EnhancementProviderName);
        Assert.Equal("test-model", saved.EnhancementModelName);
        Assert.Equal(TimeSpan.FromMilliseconds(42), saved.EnhancementDuration);
        Assert.Contains("<TRANSCRIPT>", saved.AiRequestUserMessage);
    }

    private sealed class FakeAudioFileImportService(
        AudioCaptureResult? result = null) : IAudioFileImportService
    {
        public int CallCount { get; private set; }
        public string? LastSourcePath { get; private set; }
        public string? LastRecordingsDirectory { get; private set; }
        public Exception? ExceptionToThrow { get; init; }

        public Task<AudioCaptureResult> PrepareAsync(
            string sourcePath,
            string recordingsDirectory,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastSourcePath = sourcePath;
            LastRecordingsDirectory = recordingsDirectory;
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.FromResult(result ?? new AudioCaptureResult(
                Path.Combine(recordingsDirectory, "imported.wav"),
                TimeSpan.FromSeconds(1),
                16000,
                1));
        }
    }

    private sealed class FakeTranscriptionService(
        TranscriptionResult? result = null) : ITranscriptionService
    {
        public int CallCount { get; private set; }
        public AudioCaptureResult? LastAudio { get; private set; }
        public TranscriptionOptions? LastOptions { get; private set; }
        public Exception? ExceptionToThrow { get; init; }

        public Task<TranscriptionResult> TranscribeAsync(
            AudioCaptureResult audio,
            TranscriptionOptions options,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastAudio = audio;
            LastOptions = options;
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.FromResult(result ?? new TranscriptionResult("file text", TimeSpan.Zero, "local-whisper"));
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

        public Task<IReadOnlyList<TranscriptionHistoryItem>> ListRecentAsync(int limit, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TranscriptionHistoryItem>>(Items.Take(limit).ToArray());

        public Task<IReadOnlyList<TranscriptionHistoryItem>> SearchAsync(string query, int limit, CancellationToken cancellationToken) =>
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

    private sealed class FakeTextEnhancementService(string text) : ITextEnhancementService
    {
        public Task<TextEnhancementResult> EnhanceAsync(
            TextEnhancementRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new TextEnhancementResult(
                text,
                "openai-compatible",
                request.Model,
                TimeSpan.FromMilliseconds(42)));
    }
}
