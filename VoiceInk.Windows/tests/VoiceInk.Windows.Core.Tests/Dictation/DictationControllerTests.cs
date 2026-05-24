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
            Language = "en",
            AppendTrailingSpace = true
        });

        var controller = new DictationController(capture, transcription, insertion, history, settings);

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal(DictationState.Idle, controller.State);
        Assert.Equal("hello world ", insertion.InsertedText);
        Assert.NotNull(transcription.LastOptions);
        Assert.Equal("ggml-base.en.bin", transcription.LastOptions.ModelPath);
        Assert.Equal("en", transcription.LastOptions.Language);
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
        Assert.Equal("Local whisper model path is required.", controller.LastError);
        Assert.False(capture.Started);
        Assert.Equal(0, transcription.CallCount);
    }

    [Fact]
    public async Task StartAsync_DoesNothingWhenAlreadyInError()
    {
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var transcription = new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper"));
        var settings = new FakeSettingsStore(new AppSettings());
        var controller = new DictationController(
            capture,
            transcription,
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            settings);

        await controller.StartAsync(CancellationToken.None);
        settings.CurrentSettings = settings.CurrentSettings with { ModelPath = "ggml-base.en.bin" };
        await controller.StartAsync(CancellationToken.None);

        Assert.Equal(DictationState.Error, controller.State);
        Assert.Equal("Local whisper model path is required.", controller.LastError);
        Assert.False(capture.Started);
        Assert.Equal(0, capture.StartCount);
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
    public async Task StartAsync_ConcurrentCallsOnlyStartCaptureOnce()
    {
        var loadGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var settings = new FakeSettingsStore(new AppSettings
        {
            ModelPath = "ggml-base.en.bin"
        })
        {
            LoadGate = loadGate.Task
        };
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper")),
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            settings);

        var firstStart = controller.StartAsync(CancellationToken.None);
        var secondStart = controller.StartAsync(CancellationToken.None);

        await Task.Delay(50);
        loadGate.SetResult();
        await Task.WhenAll(firstStart, secondStart);

        Assert.Equal(1, capture.StartCount);
        Assert.Equal(DictationState.Recording, controller.State);
    }

    [Fact]
    public async Task StartAsync_DuringStopInProgressDoesNotStartAfterStopFinishes()
    {
        var transcribeGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var audio = new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult("hello", TimeSpan.FromMilliseconds(150), "local-whisper"))
        {
            TranscribeGate = transcribeGate.Task
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

        var stop = controller.StopAsync(CancellationToken.None);
        await Task.Delay(50);

        var startDuringStop = controller.StartAsync(CancellationToken.None);

        await Task.Delay(50);
        transcribeGate.SetResult();
        await Task.WhenAll(stop, startDuringStop);

        Assert.Equal(1, capture.StartCount);
        Assert.Equal(1, capture.StopCount);
        Assert.Equal(DictationState.Idle, controller.State);
    }

    [Fact]
    public async Task StopAsync_DuringStartInProgressDoesNotStopAfterStartFinishes()
    {
        var loadGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var settings = new FakeSettingsStore(new AppSettings
        {
            ModelPath = "ggml-base.en.bin"
        })
        {
            LoadGate = loadGate.Task
        };
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper")),
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            settings);

        var start = controller.StartAsync(CancellationToken.None);
        await Task.Delay(50);

        var stopDuringStart = controller.StopAsync(CancellationToken.None);

        await Task.Delay(50);
        loadGate.SetResult();
        await Task.WhenAll(start, stopDuringStart);

        Assert.Equal(1, capture.StartCount);
        Assert.Equal(0, capture.StopCount);
        Assert.Equal(DictationState.Recording, controller.State);
    }

    [Fact]
    public async Task StopAsync_ConcurrentCallsOnlyStopAndInsertOnce()
    {
        var stopGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var audio = new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1);
        var capture = new FakeAudioCaptureService(audio)
        {
            StopGate = stopGate.Task
        };
        var transcription = new FakeTranscriptionService(new TranscriptionResult("hello", TimeSpan.FromMilliseconds(150), "local-whisper"));
        var insertion = new FakeTextInjectionService();
        var history = new FakeHistoryStore();
        var settings = new FakeSettingsStore(new AppSettings
        {
            ModelPath = "ggml-base.en.bin"
        });
        var controller = new DictationController(capture, transcription, insertion, history, settings);

        await controller.StartAsync(CancellationToken.None);

        var firstStop = controller.StopAsync(CancellationToken.None);
        var secondStop = controller.StopAsync(CancellationToken.None);

        await Task.Delay(50);
        stopGate.SetResult();
        await Task.WhenAll(firstStop, secondStop);

        Assert.Equal(1, capture.StopCount);
        Assert.Equal(1, transcription.CallCount);
        Assert.Equal(1, insertion.InsertCount);
        Assert.Single(history.Items);
        Assert.Equal(DictationState.Idle, controller.State);
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

    [Fact]
    public async Task StartAsync_CaptureStartFailureSetsError()
    {
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1))
        {
            StartExceptionToThrow = new InvalidOperationException("microphone unavailable")
        };
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper")),
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }));

        await controller.StartAsync(CancellationToken.None);

        Assert.Equal(DictationState.Error, controller.State);
        Assert.Equal("microphone unavailable", controller.LastError);
    }

    [Fact]
    public async Task StopAsync_CaptureStopFailureSetsError()
    {
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1))
        {
            StopExceptionToThrow = new InvalidOperationException("capture stop failed")
        };
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper")),
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }));

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal(DictationState.Error, controller.State);
        Assert.Equal("capture stop failed", controller.LastError);
    }

    [Fact]
    public async Task StopAsync_TextInsertionFailureSetsError()
    {
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var insertion = new FakeTextInjectionService
        {
            ExceptionToThrow = new InvalidOperationException("insertion failed")
        };
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("hello", TimeSpan.Zero, "local-whisper")),
            insertion,
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }));

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal(DictationState.Error, controller.State);
        Assert.Equal("insertion failed", controller.LastError);
    }

    [Fact]
    public async Task StartAsync_CancellationDuringStartPropagatesAndLeavesIdle()
    {
        using var cts = new CancellationTokenSource();
        var loadGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper")),
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            })
            {
                LoadGate = loadGate.Task
            });

        var start = controller.StartAsync(cts.Token);
        await Task.Delay(50);
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => start);
        Assert.Equal(DictationState.Idle, controller.State);
        Assert.Equal(0, capture.StartCount);
    }

    [Fact]
    public async Task StopAsync_CancellationAfterCaptureStopPropagatesAndLeavesIdle()
    {
        using var cts = new CancellationTokenSource();
        var loadEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var loadGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var settings = new FakeSettingsStore(new AppSettings
        {
            ModelPath = "ggml-base.en.bin"
        });
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper")),
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            settings);

        await controller.StartAsync(CancellationToken.None);
        settings.LoadEntered = loadEntered;
        settings.LoadGate = loadGate.Task;

        var stop = controller.StopAsync(cts.Token);
        await loadEntered.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => stop);
        Assert.False(capture.Started);
        Assert.Equal(DictationState.Idle, controller.State);
    }

    [Fact]
    public async Task StopAsync_CancellationDuringCaptureStopStillReleasesCaptureBeforePropagating()
    {
        using var cts = new CancellationTokenSource();
        var stopEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var stopGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1))
        {
            StopEntered = stopEntered,
            StopGate = stopGate.Task
        };
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper")),
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }));

        await controller.StartAsync(CancellationToken.None);

        var stop = controller.StopAsync(cts.Token);
        await stopEntered.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await cts.CancelAsync();
        stopGate.SetResult();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => stop);
        Assert.False(capture.Started);
        Assert.Equal(DictationState.Idle, controller.State);
    }

    private sealed class FakeAudioCaptureService(AudioCaptureResult result) : IAudioCaptureService
    {
        public bool Started { get; private set; }
        public int StartCount { get; private set; }
        public int StopCount { get; private set; }
        public Exception? StartExceptionToThrow { get; init; }
        public Exception? StopExceptionToThrow { get; init; }
        public TaskCompletionSource? StopEntered { get; init; }
        public Task? StopGate { get; init; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StartCount++;
            if (StartExceptionToThrow is not null)
            {
                throw StartExceptionToThrow;
            }

            Started = true;
            return Task.CompletedTask;
        }

        public async Task<AudioCaptureResult> StopAsync(CancellationToken cancellationToken)
        {
            StopCount++;
            Assert.True(Started);
            StopEntered?.SetResult();
            if (StopExceptionToThrow is not null)
            {
                throw StopExceptionToThrow;
            }

            if (StopGate is not null)
            {
                await StopGate.WaitAsync(cancellationToken);
            }

            Started = false;
            return result;
        }
    }

    private sealed class FakeTranscriptionService(TranscriptionResult result) : ITranscriptionService
    {
        public int CallCount { get; private set; }
        public Exception? ExceptionToThrow { get; init; }
        public Task? TranscribeGate { get; init; }
        public TranscriptionOptions? LastOptions { get; private set; }

        public async Task<TranscriptionResult> TranscribeAsync(
            AudioCaptureResult audio,
            TranscriptionOptions options,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastOptions = options;
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            if (TranscribeGate is not null)
            {
                await TranscribeGate.WaitAsync(cancellationToken);
            }

            return result;
        }
    }

    private sealed class FakeTextInjectionService : ITextInjectionService
    {
        public string? InsertedText { get; private set; }
        public int InsertCount { get; private set; }
        public Exception? ExceptionToThrow { get; init; }

        public Task InsertAsync(string text, CancellationToken cancellationToken)
        {
            InsertCount++;
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

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
        public AppSettings CurrentSettings { get; set; } = settings;
        public TaskCompletionSource? LoadEntered { get; set; }
        public Task? LoadGate { get; set; }

        public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken)
        {
            LoadEntered?.SetResult();
            if (LoadGate is not null)
            {
                await LoadGate.WaitAsync(cancellationToken);
            }

            return CurrentSettings;
        }

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
