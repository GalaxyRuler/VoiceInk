using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Dictation;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Metrics;
using VoiceInk.Windows.Core.PowerMode;
using VoiceInk.Windows.Core.Recording;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Text;
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
        Assert.True(controller.LastStopInsertedText);
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
    public async Task StopAsync_SavesAudioFilePathInHistory()
    {
        var audio = new AudioCaptureResult(@"C:\Recordings\sample.wav", TimeSpan.FromSeconds(2), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult("hello", TimeSpan.FromMilliseconds(150), "local-whisper"));
        var history = new FakeHistoryStore();
        var controller = new DictationController(
            capture,
            transcription,
            new FakeTextInjectionService(),
            history,
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }));

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        var saved = Assert.Single(history.Items);
        Assert.Equal(@"C:\Recordings\sample.wav", saved.AudioFilePath);
    }

    [Fact]
    public async Task StopAsync_RecordsSessionMetricForCompletedHistory()
    {
        var audio = new AudioCaptureResult(@"C:\Recordings\sample.wav", TimeSpan.FromSeconds(4), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var history = new FakeHistoryStore();
        var metrics = new FakeSessionMetricStore();
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("hello metrics", TimeSpan.FromSeconds(1), "local-whisper")),
            new FakeTextInjectionService(),
            history,
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }),
            sessionMetricStore: metrics);

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        var saved = Assert.Single(history.Items);
        var metric = Assert.Single(metrics.Saved);
        Assert.Equal(saved.Id, metric.TranscriptionId);
        Assert.Equal("recorder", metric.Source);
        Assert.Equal(2, metric.WordCount);
        Assert.Equal(TimeSpan.FromSeconds(4), metric.AudioDuration);
        Assert.Equal("ggml-base.en.bin", metric.TranscriptionModelName);
    }

    [Fact]
    public async Task StopAsync_NotifiesCaptureStopFeedbackBeforeTranscription()
    {
        var feedback = new FakeRecordingCaptureStopFeedback();
        var transcription = new FakeTranscriptionService(
            new TranscriptionResult("hello", TimeSpan.FromMilliseconds(150), "local-whisper"))
        {
            OnTranscribe = () => Assert.Equal(1, feedback.CallCount)
        };
        var controller = new DictationController(
            new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1)),
            transcription,
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings { ModelPath = "ggml-base.en.bin" }),
            recordingCaptureStopFeedback: feedback);

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal(1, feedback.CallCount);
    }

    [Fact]
    public async Task CancelAsync_StopsCaptureAndSavesCanceledHistoryWithoutTranscribingOrInserting()
    {
        var audio = new AudioCaptureResult(@"C:\Recordings\canceled.wav", TimeSpan.FromSeconds(3), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper"));
        var insertion = new FakeTextInjectionService();
        var history = new FakeHistoryStore();
        var controller = new DictationController(
            capture,
            transcription,
            insertion,
            history,
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin",
                Language = "en"
            }));

        await controller.StartAsync(CancellationToken.None);
        await controller.CancelAsync(CancellationToken.None);

        Assert.Equal(DictationState.Idle, controller.State);
        Assert.False(controller.LastStopInsertedText);
        Assert.False(capture.Started);
        Assert.Equal(1, capture.StopCount);
        Assert.Equal(0, transcription.CallCount);
        Assert.Equal(0, insertion.InsertCount);
        var saved = Assert.Single(history.Items);
        Assert.Equal(TranscriptionHistoryItem.CanceledTranscriptionText, saved.Text);
        Assert.Equal(TranscriptionHistoryItem.CanceledTranscriptionText, saved.OriginalText);
        Assert.Equal(TranscriptionHistoryStatus.Canceled, saved.Status);
        Assert.Equal("local-whisper", saved.ProviderName);
        Assert.Equal(TimeSpan.FromSeconds(3), saved.AudioDuration);
        Assert.Equal(TimeSpan.Zero, saved.TranscriptionDuration);
        Assert.Equal("en", saved.Language);
        Assert.Equal("ggml-base.en.bin", saved.ModelPath);
        Assert.Equal(@"C:\Recordings\canceled.wav", saved.AudioFilePath);
    }

    [Fact]
    public async Task CancelAsync_DoesNotRecordSessionMetric()
    {
        var audio = new AudioCaptureResult(@"C:\Recordings\canceled.wav", TimeSpan.FromSeconds(3), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var metrics = new FakeSessionMetricStore();
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper")),
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }),
            sessionMetricStore: metrics);

        await controller.StartAsync(CancellationToken.None);
        await controller.CancelAsync(CancellationToken.None);

        Assert.Empty(metrics.Saved);
    }

    [Fact]
    public async Task CancelAsync_DoesNothingWhenNotRecording()
    {
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var transcription = new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper"));
        var history = new FakeHistoryStore();
        var controller = new DictationController(
            capture,
            transcription,
            new FakeTextInjectionService(),
            history,
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }));

        await controller.CancelAsync(CancellationToken.None);

        Assert.Equal(DictationState.Idle, controller.State);
        Assert.Equal(0, capture.StopCount);
        Assert.Equal(0, transcription.CallCount);
        Assert.Empty(history.Items);
    }

    [Fact]
    public async Task UpdatePartialTranscript_AcceptsTextOnlyWhileRecordingAndClearsAfterStop()
    {
        DictationController? controller = null;
        var transcription = new FakeTranscriptionService(new TranscriptionResult("hello", TimeSpan.Zero, "local-whisper"))
        {
            OnTranscribe = () => Assert.Equal(string.Empty, controller?.PartialTranscript)
        };
        controller = new DictationController(
            new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1)),
            transcription,
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }));

        controller.UpdatePartialTranscript("ignored");
        Assert.Equal(string.Empty, controller.PartialTranscript);

        await controller.StartAsync(CancellationToken.None);
        controller.UpdatePartialTranscript("  live partial  ");
        Assert.Equal("live partial", controller.PartialTranscript);

        await controller.StopAsync(CancellationToken.None);
        Assert.Equal(string.Empty, controller.PartialTranscript);
    }

    [Fact]
    public async Task UpdatePartialTranscript_ClearsAfterCancel()
    {
        var controller = new DictationController(
            new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1)),
            new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper")),
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }));

        await controller.StartAsync(CancellationToken.None);
        controller.UpdatePartialTranscript("live partial");

        await controller.CancelAsync(CancellationToken.None);

        Assert.Equal(string.Empty, controller.PartialTranscript);
    }

    [Fact]
    public async Task UpdatePartialTranscript_IgnoresLateUpdatesAfterStopBegins()
    {
        var stopEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var stopGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var controller = new DictationController(
            new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1))
            {
                StopEntered = stopEntered,
                StopGate = stopGate.Task
            },
            new FakeTranscriptionService(new TranscriptionResult("hello", TimeSpan.Zero, "local-whisper")),
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }));

        await controller.StartAsync(CancellationToken.None);
        controller.UpdatePartialTranscript("live partial");

        var stop = controller.StopAsync(CancellationToken.None);
        await stopEntered.Task.WaitAsync(TimeSpan.FromSeconds(1));
        controller.UpdatePartialTranscript("late partial");
        Assert.Equal(string.Empty, controller.PartialTranscript);
        stopGate.SetResult();

        await stop;

        Assert.Equal(string.Empty, controller.PartialTranscript);
    }

    [Fact]
    public async Task StartAsync_WithLivePreviewService_EnqueuesPublishedAudioChunksAndAcceptsProviderPartials()
    {
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var preview = new FakeLiveTranscriptionPreviewService();
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("hello", TimeSpan.Zero, "deepgram")),
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
                CloudTranscriptionProviderId = "deepgram",
                CloudTranscriptionEndpoint = "https://api.deepgram.com/v1/listen",
                CloudTranscriptionModel = "nova-3",
                ShowLiveTranscriptPreview = true
            }),
            liveTranscriptionPreviewService: preview);

        await controller.StartAsync(CancellationToken.None);

        var session = Assert.Single(preview.Sessions);
        Assert.Equal("deepgram", preview.StartedSettings?.CloudTranscriptionProviderId);
        session.PublishPartial("  live provider text  ");
        Assert.Equal("live provider text", controller.PartialTranscript);

        capture.PublishAudioChunk([1, 2, 3, 4]);

        var chunk = Assert.Single(session.AudioChunks);
        Assert.Equal([1, 2, 3, 4], chunk.Pcm16Bytes);
        Assert.Equal(16000, chunk.SampleRate);
        Assert.Equal(1, chunk.ChannelCount);
    }

    [Fact]
    public async Task StopAsync_CompletesLivePreviewAndIgnoresLatePreviewPartials()
    {
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var preview = new FakeLiveTranscriptionPreviewService();
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("hello", TimeSpan.Zero, "deepgram")),
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
                CloudTranscriptionProviderId = "deepgram",
                CloudTranscriptionEndpoint = "https://api.deepgram.com/v1/listen",
                CloudTranscriptionModel = "nova-3",
                ShowLiveTranscriptPreview = true
            }),
            liveTranscriptionPreviewService: preview);

        await controller.StartAsync(CancellationToken.None);
        var session = Assert.Single(preview.Sessions);
        session.PublishPartial("before stop");
        Assert.Equal("before stop", controller.PartialTranscript);

        await controller.StopAsync(CancellationToken.None);
        session.PublishPartial("late partial");

        Assert.Equal(string.Empty, controller.PartialTranscript);
        Assert.Equal(1, session.CompleteCount);
        Assert.True(session.IsDisposed);
    }

    [Fact]
    public async Task CancelAsync_CompletesLivePreviewAndIgnoresLatePreviewPartials()
    {
        var preview = new FakeLiveTranscriptionPreviewService();
        var controller = new DictationController(
            new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1)),
            new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "deepgram")),
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
                CloudTranscriptionProviderId = "deepgram",
                CloudTranscriptionEndpoint = "https://api.deepgram.com/v1/listen",
                CloudTranscriptionModel = "nova-3",
                ShowLiveTranscriptPreview = true
            }),
            liveTranscriptionPreviewService: preview);

        await controller.StartAsync(CancellationToken.None);
        var session = Assert.Single(preview.Sessions);
        session.PublishPartial("before cancel");

        await controller.CancelAsync(CancellationToken.None);
        session.PublishPartial("late partial");

        Assert.Equal(string.Empty, controller.PartialTranscript);
        Assert.Equal(1, session.CompleteCount);
        Assert.True(session.IsDisposed);
    }

    [Fact]
    public async Task StartAsync_LivePreviewStartFailureDoesNotBlockRecording()
    {
        var preview = new FakeLiveTranscriptionPreviewService
        {
            ExceptionToThrow = new InvalidOperationException("stream unavailable")
        };
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("hello", TimeSpan.Zero, "deepgram")),
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
                CloudTranscriptionProviderId = "deepgram",
                CloudTranscriptionEndpoint = "https://api.deepgram.com/v1/listen",
                CloudTranscriptionModel = "nova-3",
                ShowLiveTranscriptPreview = true
            }),
            liveTranscriptionPreviewService: preview);

        await controller.StartAsync(CancellationToken.None);

        Assert.Equal(DictationState.Recording, controller.State);
        Assert.True(capture.Started);
        Assert.Equal("Live transcript preview unavailable: stream unavailable", controller.LastWarning);
    }

    [Fact]
    public async Task StopAsync_AppliesCleanupSettingsAndDictionaryReplacementsToInsertedAndHistoryText()
    {
        var audio = new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult("Um, voice ink!", TimeSpan.FromMilliseconds(150), "local-whisper"));
        var insertion = new FakeTextInjectionService();
        var history = new FakeHistoryStore();
        var settings = new FakeSettingsStore(new AppSettings
        {
            ModelPath = "C:\\Models\\ggml-base.en.bin",
            Language = "en",
            RemoveFillerWords = false,
            PunctuationCleanupMode = PunctuationCleanupMode.RemoveAll,
            LowercaseTranscription = true
        });
        var dictionary = new FakeDictionaryStore
        {
            Replacements =
            [
                new WordReplacement(Guid.NewGuid(), "voice ink", "VoiceInk!", DateTimeOffset.UtcNow)
            ]
        };
        var controller = new DictationController(capture, transcription, insertion, history, settings, dictionary);

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal("um voiceink ", insertion.InsertedText);
        var saved = Assert.Single(history.Items);
        Assert.Equal("Um, voice ink!", saved.OriginalText);
        Assert.Equal("um voiceink ", saved.Text);
        Assert.Equal(TranscriptionHistoryStatus.Completed, saved.Status);
        Assert.Equal("en", saved.Language);
        Assert.Equal("C:\\Models\\ggml-base.en.bin", saved.ModelPath);
    }

    [Fact]
    public async Task StopAsync_PassesVocabularyPromptToTranscriptionOptions()
    {
        var audio = new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult("VoiceInk", TimeSpan.FromMilliseconds(150), "local-whisper"));
        var dictionary = new FakeDictionaryStore
        {
            Vocabulary =
            [
                new VocabularyWord(Guid.NewGuid(), "Whisper", DateTimeOffset.UtcNow),
                new VocabularyWord(Guid.NewGuid(), "VoiceInk", DateTimeOffset.UtcNow)
            ]
        };
        var controller = new DictationController(
            capture,
            transcription,
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin",
                Language = "en"
            }),
            dictionary);

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.NotNull(transcription.LastOptions);
        Assert.Equal("Important Vocabulary: VoiceInk, Whisper", transcription.LastOptions.Prompt);
    }

    [Fact]
    public async Task StopAsync_WhenVadEnabledAndNoSpeechDetectedSkipsTranscription()
    {
        var audio = new AudioCaptureResult("silent.wav", TimeSpan.FromSeconds(2), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult("should not run", TimeSpan.Zero, "local-whisper"));
        var insertion = new FakeTextInjectionService();
        var history = new FakeHistoryStore();
        var voiceActivity = new FakeVoiceActivityDetector(new VoiceActivityResult(false, TimeSpan.Zero));
        var controller = new DictationController(
            capture,
            transcription,
            insertion,
            history,
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin",
                IsVadEnabled = true
            }),
            voiceActivityDetector: voiceActivity);

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal(1, voiceActivity.CallCount);
        Assert.Same(audio, voiceActivity.LastAudio);
        Assert.Equal(0, transcription.CallCount);
        Assert.Null(insertion.InsertedText);
        Assert.Empty(history.Items);
        Assert.Equal(DictationState.Idle, controller.State);
        Assert.Equal("No speech detected", controller.LastWarning);
    }

    [Fact]
    public async Task StopAsync_WhenVadDisabledDoesNotAnalyzeVoiceActivity()
    {
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var transcription = new FakeTranscriptionService(new TranscriptionResult("hello", TimeSpan.FromMilliseconds(1), "local-whisper"));
        var voiceActivity = new FakeVoiceActivityDetector(new VoiceActivityResult(false, TimeSpan.Zero));
        var controller = new DictationController(
            capture,
            transcription,
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin",
                IsVadEnabled = false
            }),
            voiceActivityDetector: voiceActivity);

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal(0, voiceActivity.CallCount);
        Assert.Equal(1, transcription.CallCount);
    }

    [Fact]
    public async Task StopAsync_PassesCloudTranscriptionOptionsAndSavesCloudModelMetadata()
    {
        var audio = new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult(
            "VoiceInk",
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
        var controller = new DictationController(
            capture,
            transcription,
            new FakeTextInjectionService(),
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

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.NotNull(transcription.LastOptions);
        Assert.Equal(TranscriptionProviderKind.OpenAICompatible, transcription.LastOptions.Provider);
        Assert.Equal("groq", transcription.LastOptions.CloudProviderId);
        Assert.Equal("https://api.groq.com/openai/v1/audio/transcriptions", transcription.LastOptions.CloudEndpoint);
        Assert.Equal("whisper-large-v3-turbo", transcription.LastOptions.CloudModel);
        Assert.Equal("en", transcription.LastOptions.Language);
        Assert.Equal("Important Vocabulary: VoiceInk", transcription.LastOptions.Prompt);
        var saved = Assert.Single(history.Items);
        Assert.Equal("groq", saved.ProviderName);
        Assert.Equal("whisper-large-v3-turbo", saved.ModelPath);
    }

    [Fact]
    public async Task StopAsync_InsertsEnhancedTextAndSavesEnhancementMetadata()
    {
        var audio = new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult("hello world", TimeSpan.FromMilliseconds(150), "local-whisper"));
        var insertion = new FakeTextInjectionService();
        var history = new FakeHistoryStore();
        var settings = new FakeSettingsStore(new AppSettings
        {
            ModelPath = "ggml-base.en.bin",
            EnhancementEndpoint = "https://example.test/v1/chat/completions",
            EnhancementModel = "test-model",
            IsEnhancementEnabled = true,
            SkipShortEnhancement = false
        });
        var enhancement = new FakeTextEnhancementService("Hello, world.");
        var controller = new DictationController(
            capture,
            transcription,
            insertion,
            history,
            settings,
            enhancementPipeline: new TextEnhancementPipeline(enhancement));

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal("Hello, world.", insertion.InsertedText);
        var saved = Assert.Single(history.Items);
        Assert.Equal("hello world ", saved.Text);
        Assert.Equal("hello world", saved.OriginalText);
        Assert.Equal("Hello, world.", saved.EnhancedText);
        Assert.Equal("Default", saved.PromptName);
        Assert.Equal("openai-compatible", saved.EnhancementProviderName);
        Assert.Equal("test-model", saved.EnhancementModelName);
        Assert.Equal(TimeSpan.FromMilliseconds(42), saved.EnhancementDuration);
        Assert.Contains("TRANSCRIPTION ENHANCER", saved.AiRequestSystemMessage);
        Assert.Contains("<TRANSCRIPT>", saved.AiRequestUserMessage);
    }

    [Fact]
    public async Task StopAsync_UsesRecordingStartTargetWithLatestSettingsAtStop()
    {
        var audio = new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult("hello", TimeSpan.FromMilliseconds(150), "local-whisper"));
        var insertion = new FakeTextInjectionService();
        var history = new FakeHistoryStore();
        var settings = new FakeSettingsStore(new AppSettings
        {
            ModelPath = "base.bin",
            Language = "auto",
            AppendTrailingSpace = false,
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Chat",
                    Emoji = "C",
                    ProcessNamePattern = "teams",
                    ModelPathOverride = "chat.bin",
                    LanguageOverride = "en",
                    AppendTrailingSpaceOverride = true
                }
            ]
        });
        var targetProvider = new FakePowerModeTargetProvider(new PowerModeTarget("Teams", "Project Chat", 300));
        var controller = new DictationController(
            capture,
            transcription,
            insertion,
            history,
            settings,
            powerModeTargetProvider: targetProvider);

        await controller.StartAsync(CancellationToken.None);
        settings.CurrentSettings = settings.CurrentSettings with
        {
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Other",
                    Emoji = "O",
                    ProcessNamePattern = "teams",
                    ModelPathOverride = "other.bin",
                    LanguageOverride = "fr",
                    AppendTrailingSpaceOverride = true
                }
            ]
        };
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal(1, targetProvider.CallCount);
        Assert.Equal("hello ", insertion.InsertedText);
        Assert.Equal("other.bin", transcription.LastOptions?.ModelPath);
        Assert.Equal("fr", transcription.LastOptions?.Language);
        var saved = Assert.Single(history.Items);
        Assert.Equal("Other", saved.PowerModeName);
        Assert.Equal("O", saved.PowerModeEmoji);
        Assert.Equal("other.bin", saved.ModelPath);
        Assert.Equal("fr", saved.Language);
    }

    [Fact]
    public async Task StopAsync_AutoSendsPowerModeKeyAfterSuccessfulInsertion()
    {
        var audio = new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var insertion = new FakeTextInjectionService();
        var autoSend = new FakePowerModeAutoSendService();
        var settings = new FakeSettingsStore(new AppSettings
        {
            ModelPath = "base.bin",
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Chat",
                    ProcessNamePattern = "teams",
                    AutoSendKey = PowerModeAutoSendKey.CommandEnter
                }
            ]
        });
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("hello", TimeSpan.FromMilliseconds(150), "local-whisper")),
            insertion,
            new FakeHistoryStore(),
            settings,
            powerModeTargetProvider: new FakePowerModeTargetProvider(new PowerModeTarget("Teams", "Chat", 300)),
            powerModeAutoSendService: autoSend);

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal("hello ", insertion.InsertedText);
        Assert.Equal(PowerModeAutoSendKey.CommandEnter, autoSend.LastKey);
        Assert.Equal(1, autoSend.CallCount);
    }

    [Fact]
    public async Task StopAsync_SkipsAutoSendWhenInsertionFails()
    {
        var audio = new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var autoSend = new FakePowerModeAutoSendService();
        var settings = new FakeSettingsStore(new AppSettings
        {
            ModelPath = "base.bin",
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Chat",
                    ProcessNamePattern = "teams",
                    AutoSendKey = PowerModeAutoSendKey.Enter
                }
            ]
        });
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("hello", TimeSpan.FromMilliseconds(150), "local-whisper")),
            new FakeTextInjectionService { ExceptionToThrow = new InvalidOperationException("paste failed") },
            new FakeHistoryStore(),
            settings,
            powerModeTargetProvider: new FakePowerModeTargetProvider(new PowerModeTarget("Teams", "Chat", 300)),
            powerModeAutoSendService: autoSend);

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal(0, autoSend.CallCount);
        Assert.Equal("paste failed", controller.LastError);
    }

    [Fact]
    public async Task StopAsync_UsesPromptSelectedDuringRecording()
    {
        var customPromptId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var prompts = EnhancementPromptCatalog.CreateDefaultPrompts()
            .Concat(
            [
                new EnhancementPrompt(
                    customPromptId,
                    "Standup",
                    "Format as a standup update.",
                    "list.bullet",
                    "Daily update",
                    IsPredefined: false,
                    TriggerWords: [],
                    UseSystemInstructions: true)
            ])
            .ToArray();
        var audio = new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var insertion = new FakeTextInjectionService();
        var history = new FakeHistoryStore();
        var settings = new FakeSettingsStore(new AppSettings
        {
            ModelPath = "ggml-base.en.bin",
            EnhancementEndpoint = "https://example.test/v1/chat/completions",
            EnhancementModel = "test-model",
            IsEnhancementEnabled = true,
            SelectedEnhancementPromptId = EnhancementPromptCatalog.DefaultPromptId,
            SkipShortEnhancement = false
        });
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("hello world", TimeSpan.FromMilliseconds(150), "local-whisper")),
            insertion,
            history,
            settings,
            enhancementPipeline: new TextEnhancementPipeline(
                new FakeTextEnhancementService("standup text"),
                prompts));

        await controller.StartAsync(CancellationToken.None);
        settings.CurrentSettings = settings.CurrentSettings with
        {
            SelectedEnhancementPromptId = customPromptId
        };
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal("standup text", insertion.InsertedText);
        var saved = Assert.Single(history.Items);
        Assert.Equal("Standup", saved.PromptName);
    }

    [Fact]
    public async Task CancelAsync_SavesRecordingStartPowerModeMetadata()
    {
        var audio = new AudioCaptureResult(@"C:\Recordings\canceled.wav", TimeSpan.FromSeconds(3), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var history = new FakeHistoryStore();
        var settings = new FakeSettingsStore(new AppSettings
        {
            ModelPath = "base.bin",
            Language = "auto",
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Docs",
                    Emoji = "D",
                    ProcessNamePattern = "winword",
                    LanguageOverride = "en"
                }
            ]
        });
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper")),
            new FakeTextInjectionService(),
            history,
            settings,
            powerModeTargetProvider: new FakePowerModeTargetProvider(new PowerModeTarget("WINWORD", "Document", 301)));

        await controller.StartAsync(CancellationToken.None);
        await controller.CancelAsync(CancellationToken.None);

        var saved = Assert.Single(history.Items);
        Assert.Equal(TranscriptionHistoryStatus.Canceled, saved.Status);
        Assert.Equal("Docs", saved.PowerModeName);
        Assert.Equal("D", saved.PowerModeEmoji);
        Assert.Equal("en", saved.Language);
    }

    [Fact]
    public async Task StartAsync_WhenPowerModeTargetProviderFailsFallsBackToBaseSettings()
    {
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var transcription = new FakeTranscriptionService(new TranscriptionResult("hello", TimeSpan.Zero, "local-whisper"));
        var controller = new DictationController(
            capture,
            transcription,
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "base.bin",
                Language = "auto",
                PowerModeRules =
                [
                    new PowerModeRule
                    {
                        Name = "Default",
                        Emoji = "*",
                        IsDefault = true,
                        ModelPathOverride = "default.bin"
                    }
                ]
            }),
            powerModeTargetProvider: new FakePowerModeTargetProvider(exception: new InvalidOperationException("foreground unavailable")));

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal("base.bin", transcription.LastOptions?.ModelPath);
        Assert.Equal("auto", transcription.LastOptions?.Language);
    }

    [Fact]
    public async Task StopAsync_WhenEnhancementFailsInsertsOriginalAndSavesWarning()
    {
        var audio = new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult("hello", TimeSpan.FromMilliseconds(150), "local-whisper"));
        var insertion = new FakeTextInjectionService();
        var history = new FakeHistoryStore();
        var settings = new FakeSettingsStore(new AppSettings
        {
            ModelPath = "ggml-base.en.bin",
            EnhancementEndpoint = "https://example.test/v1/chat/completions",
            EnhancementModel = "test-model",
            IsEnhancementEnabled = true,
            SkipShortEnhancement = false
        });
        var enhancement = new FakeTextEnhancementService("unused")
        {
            Exception = new InvalidOperationException("provider unavailable")
        };
        var controller = new DictationController(
            capture,
            transcription,
            insertion,
            history,
            settings,
            enhancementPipeline: new TextEnhancementPipeline(enhancement));

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal("hello ", insertion.InsertedText);
        Assert.Contains("Enhancement failed: provider unavailable", controller.LastWarning);
        var saved = Assert.Single(history.Items);
        Assert.Equal("hello ", saved.Text);
        Assert.Null(saved.EnhancedText);
        Assert.Contains("Enhancement failed: provider unavailable", saved.ErrorMessage);
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
    public async Task StartAsync_StartsRecordingForCloudProviderWithoutLocalModelPath()
    {
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var transcription = new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "openai-compatible"));
        var controller = new DictationController(
            capture,
            transcription,
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
                CloudTranscriptionEndpoint = "https://api.example.test/v1/audio/transcriptions",
                CloudTranscriptionModel = "gpt-4o-transcribe"
            }));

        await controller.StartAsync(CancellationToken.None);

        Assert.Equal(DictationState.Recording, controller.State);
        Assert.True(capture.Started);
        Assert.Null(controller.LastError);
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

        Assert.Equal("hello ", insertion.InsertedText);
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
        var history = new FakeHistoryStore();
        var controller = new DictationController(
            capture,
            transcription,
            new FakeTextInjectionService(),
            history,
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }));

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal(DictationState.Error, controller.State);
        Assert.Equal("model failed", controller.LastError);
        var failed = Assert.Single(history.Items);
        Assert.Equal(TranscriptionHistoryStatus.Failed, failed.Status);
        Assert.Equal("Transcription Failed: model failed", failed.Text);
        Assert.Equal("model failed", failed.ErrorMessage);
        Assert.Equal(audio.FilePath, failed.AudioFilePath);
        Assert.Equal(audio.Duration, failed.AudioDuration);
        Assert.Equal("local-whisper", failed.ProviderName);
        Assert.Equal("ggml-base.en.bin", failed.ModelPath);
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
    public async Task StartAsync_CaptureStartCancellationFailureSetsErrorWhenCallerTokenIsNotCanceled()
    {
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1))
        {
            StartExceptionToThrow = new OperationCanceledException("capture start canceled")
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
        Assert.Equal("capture start canceled", controller.LastError);
        Assert.Equal(1, capture.StartCount);
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
    public async Task StopAsync_CaptureStopCancellationFailureSetsErrorWhenCallerTokenIsNotCanceled()
    {
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1))
        {
            StopExceptionToThrow = new OperationCanceledException("capture stop canceled")
        };
        var transcription = new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper"));
        var insertion = new FakeTextInjectionService();
        var history = new FakeHistoryStore();
        var controller = new DictationController(
            capture,
            transcription,
            insertion,
            history,
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }));

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal(DictationState.Error, controller.State);
        Assert.Equal("capture stop canceled", controller.LastError);
        Assert.Equal(0, transcription.CallCount);
        Assert.Equal(0, insertion.InsertCount);
        Assert.Empty(history.Items);
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
        Assert.False(controller.LastStopInsertedText);
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
        var vocabularyEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var vocabularyGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var dictionary = new FakeDictionaryStore
        {
            VocabularyEntered = vocabularyEntered,
            VocabularyGate = vocabularyGate.Task
        };
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper")),
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }),
            dictionary);

        await controller.StartAsync(CancellationToken.None);

        var stop = controller.StopAsync(cts.Token);
        await vocabularyEntered.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => stop);
        Assert.False(capture.Started);
        Assert.Equal(DictationState.Idle, controller.State);
    }

    [Fact]
    public async Task StopAsync_CancellationDuringTranscriptionSavesCanceledHistoryAndNoMetric()
    {
        using var cts = new CancellationTokenSource();
        var transcribeEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var transcribeGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var audio = new AudioCaptureResult(@"C:\Recordings\processing-canceled.wav", TimeSpan.FromSeconds(5), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper"))
        {
            OnTranscribe = () => transcribeEntered.SetResult(),
            TranscribeGate = transcribeGate.Task
        };
        var insertion = new FakeTextInjectionService();
        var history = new FakeHistoryStore();
        var metrics = new FakeSessionMetricStore();
        var controller = new DictationController(
            capture,
            transcription,
            insertion,
            history,
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin",
                Language = "en"
            }),
            sessionMetricStore: metrics);

        await controller.StartAsync(CancellationToken.None);

        var stop = controller.StopAsync(cts.Token);
        await transcribeEntered.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => stop);

        Assert.Equal(DictationState.Idle, controller.State);
        Assert.False(capture.Started);
        Assert.Equal(0, insertion.InsertCount);
        Assert.Empty(metrics.Saved);
        var saved = Assert.Single(history.Items);
        Assert.Equal(TranscriptionHistoryStatus.Canceled, saved.Status);
        Assert.Equal(TranscriptionHistoryItem.CanceledTranscriptionText, saved.Text);
        Assert.Equal(TranscriptionHistoryItem.CanceledTranscriptionText, saved.OriginalText);
        Assert.Equal(@"C:\Recordings\processing-canceled.wav", saved.AudioFilePath);
        Assert.Equal(TimeSpan.FromSeconds(5), saved.AudioDuration);
        Assert.Equal(TimeSpan.Zero, saved.TranscriptionDuration);
        Assert.Equal("local-whisper", saved.ProviderName);
        Assert.Equal("en", saved.Language);
        Assert.Equal("ggml-base.en.bin", saved.ModelPath);
    }

    [Fact]
    public async Task StopAsync_CancellationAfterInsertionDoesNotSaveCanceledHistory()
    {
        using var cts = new CancellationTokenSource();
        var historyEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var historyGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var capture = new FakeAudioCaptureService(
            new AudioCaptureResult(@"C:\Recordings\inserted-before-cancel.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var insertion = new FakeTextInjectionService();
        var history = new FakeHistoryStore
        {
            SaveEntered = historyEntered,
            SaveGate = historyGate.Task
        };
        var controller = new DictationController(
            capture,
            new FakeTranscriptionService(new TranscriptionResult("hello", TimeSpan.FromMilliseconds(150), "local-whisper")),
            insertion,
            history,
            new FakeSettingsStore(new AppSettings
            {
                ModelPath = "ggml-base.en.bin"
            }));

        await controller.StartAsync(CancellationToken.None);

        var stop = controller.StopAsync(cts.Token);
        await historyEntered.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await cts.CancelAsync();
        historyGate.SetResult();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => stop);

        Assert.Equal("hello ", insertion.InsertedText);
        Assert.Equal(DictationState.Idle, controller.State);
        Assert.Empty(history.Items);
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

    private sealed class FakeAudioCaptureService(AudioCaptureResult result) : IAudioCaptureService, IAudioChunkPublisher
    {
        public bool Started { get; private set; }
        public int StartCount { get; private set; }
        public int StopCount { get; private set; }
        public Exception? StartExceptionToThrow { get; init; }
        public Exception? StopExceptionToThrow { get; init; }
        public TaskCompletionSource? StopEntered { get; init; }
        public Task? StopGate { get; init; }
        public event EventHandler<AudioChunk>? AudioChunkAvailable;

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

        public void PublishAudioChunk(byte[] bytes)
        {
            AudioChunkAvailable?.Invoke(this, new AudioChunk(bytes, 16000, 1));
        }
    }

    private sealed class FakeTranscriptionService(TranscriptionResult result) : ITranscriptionService
    {
        public int CallCount { get; private set; }
        public Exception? ExceptionToThrow { get; init; }
        public Task? TranscribeGate { get; init; }
        public Action? OnTranscribe { get; init; }
        public TranscriptionOptions? LastOptions { get; private set; }

        public async Task<TranscriptionResult> TranscribeAsync(
            AudioCaptureResult audio,
            TranscriptionOptions options,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastOptions = options;
            OnTranscribe?.Invoke();
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

    private sealed class FakeVoiceActivityDetector(VoiceActivityResult result) : IVoiceActivityDetector
    {
        public int CallCount { get; private set; }
        public AudioCaptureResult? LastAudio { get; private set; }

        public Task<VoiceActivityResult> AnalyzeAsync(
            AudioCaptureResult audio,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastAudio = audio;
            return Task.FromResult(result);
        }
    }

    private sealed class FakeRecordingCaptureStopFeedback : IRecordingCaptureStopFeedback
    {
        public int CallCount { get; private set; }

        public Task CaptureStoppedAsync(CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.CompletedTask;
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

    private sealed class FakePowerModeAutoSendService : IPowerModeAutoSendService
    {
        public int CallCount { get; private set; }
        public PowerModeAutoSendKey? LastKey { get; private set; }

        public Task SendAsync(PowerModeAutoSendKey key, CancellationToken cancellationToken)
        {
            CallCount++;
            LastKey = key;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeHistoryStore : IHistoryStore
    {
        public List<TranscriptionHistoryItem> Items { get; } = [];
        public Exception? ExceptionToThrow { get; init; }
        public TaskCompletionSource? SaveEntered { get; init; }
        public Task? SaveGate { get; init; }

        public async Task SaveAsync(TranscriptionHistoryItem item, CancellationToken cancellationToken)
        {
            SaveEntered?.SetResult();
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            if (SaveGate is not null)
            {
                await SaveGate.WaitAsync(cancellationToken);
            }

            Items.Add(item);
        }

        public Task<IReadOnlyList<TranscriptionHistoryItem>> ListRecentAsync(int limit, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<TranscriptionHistoryItem>>(Items.Take(limit).ToList());
        }

        public Task<IReadOnlyList<TranscriptionHistoryItem>> SearchAsync(
            string query,
            int limit,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<TranscriptionHistoryItem>>(Items.Take(limit).ToList());
        }

        public Task<HistoryPage> ListPageAsync(
            string? query,
            HistoryPageCursor? cursor,
            int pageSize,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HistoryPage(Items.Take(pageSize).ToArray(), NextCursor: null, HasMore: false));
        }

        public Task<TranscriptionHistoryItem?> GetLatestCompletedAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Items.FirstOrDefault(item => item.Status == TranscriptionHistoryStatus.Completed));
        }

        public Task<TranscriptionHistoryItem?> GetLatestCompletedWithAudioAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Items.FirstOrDefault(item =>
                item.Status == TranscriptionHistoryStatus.Completed
                && !string.IsNullOrWhiteSpace(item.AudioFilePath)));
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Items.RemoveAll(item => item.Id == id) > 0);
        }

        public Task<IReadOnlyList<TranscriptionHistoryItem>> ListOlderThanAsync(
            DateTimeOffset cutoff,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<TranscriptionHistoryItem>>(
                Items
                    .Where(item => item.CreatedAt.UtcDateTime.Ticks < cutoff.UtcDateTime.Ticks)
                    .ToArray());
        }

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

    private sealed class FakeSessionMetricStore : ISessionMetricStore
    {
        public List<SessionMetric> Saved { get; } = [];

        public Task SaveAsync(SessionMetric metric, CancellationToken cancellationToken)
        {
            Saved.Add(metric);
            return Task.CompletedTask;
        }

        public Task ClearAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<bool> HasTranscriptionAsync(Guid transcriptionId, CancellationToken cancellationToken) =>
            Task.FromResult(Saved.Any(metric => metric.TranscriptionId == transcriptionId));

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

    private sealed class FakeDictionaryStore : IDictionaryStore
    {
        public IReadOnlyList<VocabularyWord> Vocabulary { get; init; } = [];
        public IReadOnlyList<WordReplacement> Replacements { get; init; } = [];
        public TaskCompletionSource? VocabularyEntered { get; init; }
        public Task? VocabularyGate { get; init; }

        public async Task<IReadOnlyList<VocabularyWord>> ListVocabularyAsync(CancellationToken cancellationToken)
        {
            VocabularyEntered?.SetResult();
            if (VocabularyGate is not null)
            {
                await VocabularyGate.WaitAsync(cancellationToken);
            }

            return Vocabulary;
        }

        public Task<IReadOnlyList<WordReplacement>> ListReplacementsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Replacements);
        }
    }

    private sealed class FakeTextEnhancementService(string text) : ITextEnhancementService
    {
        public Exception? Exception { get; init; }

        public Task<TextEnhancementResult> EnhanceAsync(
            TextEnhancementRequest request,
            CancellationToken cancellationToken)
        {
            if (Exception is not null)
            {
                throw Exception;
            }

            return Task.FromResult(new TextEnhancementResult(
                text,
                "openai-compatible",
                request.Model,
                TimeSpan.FromMilliseconds(42)));
        }
    }

    private sealed class FakePowerModeTargetProvider(
        PowerModeTarget? target = null,
        Exception? exception = null) : IPowerModeTargetProvider
    {
        public int CallCount { get; private set; }

        public Task<PowerModeTarget?> GetCurrentTargetAsync(CancellationToken cancellationToken)
        {
            CallCount++;
            if (exception is not null)
            {
                throw exception;
            }

            return Task.FromResult(target);
        }
    }

    private sealed class FakeLiveTranscriptionPreviewService : ILiveTranscriptionPreviewService
    {
        public AppSettings? StartedSettings { get; private set; }
        public List<FakeLiveTranscriptionPreviewSession> Sessions { get; } = [];
        public Exception? ExceptionToThrow { get; init; }

        public Task<ILiveTranscriptionPreviewSession?> TryStartAsync(
            AppSettings settings,
            Action<string> partialTranscriptUpdated,
            CancellationToken cancellationToken)
        {
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            StartedSettings = settings;
            var session = new FakeLiveTranscriptionPreviewSession(partialTranscriptUpdated);
            Sessions.Add(session);
            return Task.FromResult<ILiveTranscriptionPreviewSession?>(session);
        }
    }

    private sealed class FakeLiveTranscriptionPreviewSession(
        Action<string> partialTranscriptUpdated) : ILiveTranscriptionPreviewSession
    {
        public List<AudioChunk> AudioChunks { get; } = [];
        public int CompleteCount { get; private set; }
        public bool IsDisposed { get; private set; }

        public void EnqueueAudio(AudioChunk chunk)
        {
            AudioChunks.Add(chunk);
        }

        public Task CompleteAsync(CancellationToken cancellationToken)
        {
            CompleteCount++;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return ValueTask.CompletedTask;
        }

        public void PublishPartial(string text)
        {
            partialTranscriptUpdated(text);
        }
    }
}
