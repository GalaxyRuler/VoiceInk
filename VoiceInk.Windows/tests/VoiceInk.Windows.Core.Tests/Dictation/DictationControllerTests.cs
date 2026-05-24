using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Dictation;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Dictation;

public sealed class DictationControllerTests
{
    [Fact]
    public async Task StopAsync_TranscribesInsertsAndSavesHistory()
    {
        var audio = new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult(" hello world ", TimeSpan.FromMilliseconds(150), "local-whisper"));
        var insertion = new FakeTextInjectionService();
        var history = new FakeHistoryStore();
        var settings = new FakeSettingsStore(new AppSettings
        {
            ModelPath = "ggml-base.en.bin",
            AppendTrailingSpace = true
        });

        var controller = new DictationController(capture, transcription, insertion, history, settings);

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal(DictationState.Idle, controller.State);
        Assert.Equal("hello world ", insertion.InsertedText);
        var saved = Assert.Single(history.Items);
        Assert.Equal("hello world ", saved.Text);
        Assert.Equal("local-whisper", saved.ProviderName);
        Assert.Equal(TimeSpan.FromSeconds(2), saved.AudioDuration);
    }

    [Fact]
    public async Task StartAsync_DoesNotStartRecordingWhenModelPathIsMissing()
    {
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var transcription = new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper"));
        var controller = new DictationController(
            capture,
            transcription,
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings()));

        await controller.StartAsync(CancellationToken.None);

        Assert.Equal(DictationState.Error, controller.State);
        Assert.Equal("Select a local whisper model before dictating.", controller.LastError);
        Assert.False(capture.Started);
        Assert.Equal(0, transcription.CallCount);
    }

    [Fact]
    public async Task StopAsync_DoesNothingWhenNotRecording()
    {
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var transcription = new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper"));
        var controller = new DictationController(
            capture,
            transcription,
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }));

        await controller.StopAsync(CancellationToken.None);

        Assert.Equal(0, capture.StopCount);
        Assert.Equal(0, transcription.CallCount);
        Assert.Equal(DictationState.Idle, controller.State);
    }

    [Fact]
    public async Task StopAsync_DoesNothingAfterMissingModel()
    {
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var transcription = new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper"));
        var controller = new DictationController(
            capture,
            transcription,
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings()));

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.False(capture.Started);
        Assert.Equal(0, capture.StopCount);
        Assert.Equal(0, transcription.CallCount);
        Assert.Equal(DictationState.Error, controller.State);
    }

    [Fact]
    public async Task StartAsync_DoesNothingWhenAlreadyRecording()
    {
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var transcription = new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper"));
        var controller = new DictationController(
            capture,
            transcription,
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }));

        await controller.StartAsync(CancellationToken.None);
        await controller.StartAsync(CancellationToken.None);

        Assert.Equal(1, capture.StartCount);
        Assert.Equal(DictationState.Recording, controller.State);
    }

    [Fact]
    public async Task StopAsync_HistoryFailureLeavesControllerIdleWithWarning()
    {
        var audio = new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult("hello", TimeSpan.FromMilliseconds(150), "local-whisper"));
        var insertion = new FakeTextInjectionService();
        var history = new FakeHistoryStore
        {
            ExceptionToThrow = new InvalidOperationException("database unavailable")
        };
        var settings = new FakeSettingsStore(new AppSettings
        {
            ModelPath = "ggml-base.en.bin"
        });
        var controller = new DictationController(capture, transcription, insertion, history, settings);

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal("hello", insertion.InsertedText);
        Assert.Equal(DictationState.Idle, controller.State);
        Assert.Equal("History save failed: database unavailable", controller.LastWarning);
    }

    [Fact]
    public async Task StopAsync_TranscriptionFailureSetsError()
    {
        var audio = new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper"))
        {
            ExceptionToThrow = new InvalidOperationException("model failed")
        };
        var controller = new DictationController(
            capture,
            transcription,
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }));

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal(DictationState.Error, controller.State);
        Assert.Equal("model failed", controller.LastError);
    }

    private sealed class FakeAudioCaptureService(AudioCaptureResult result) : IAudioCaptureService
    {
        public bool Started { get; private set; }
        public int StartCount { get; private set; }
        public int StopCount { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StartCount++;
            Started = true;
            return Task.CompletedTask;
        }

        public Task<AudioCaptureResult> StopAsync(CancellationToken cancellationToken)
        {
            StopCount++;
            Assert.True(Started);
            return Task.FromResult(result);
        }
    }

    private sealed class FakeTranscriptionService(TranscriptionResult result) : ITranscriptionService
    {
        public int CallCount { get; private set; }
        public Exception? ExceptionToThrow { get; init; }

        public Task<TranscriptionResult> TranscribeAsync(
            AudioCaptureResult audio,
            TranscriptionOptions options,
            CancellationToken cancellationToken)
        {
            CallCount++;
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            Assert.Equal("ggml-base.en.bin", options.ModelPath);
            return Task.FromResult(result);
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

    private sealed class FakeHistoryStore : IHistoryStore
    {
        public List<TranscriptionHistoryItem> Items { get; } = [];
        public Exception? ExceptionToThrow { get; init; }

        public Task SaveAsync(TranscriptionHistoryItem item, CancellationToken cancellationToken)
        {
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            Items.Add(item);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TranscriptionHistoryItem>> ListRecentAsync(int limit, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<TranscriptionHistoryItem>>(Items.Take(limit).ToList());
        }
    }

    private sealed class FakeSettingsStore(AppSettings settings) : ISettingsStore
    {
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(settings);

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
