using VoiceInk.Windows.Core.Models;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using System.Diagnostics;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Models;

public sealed class WhisperModelWarmupCoordinatorTests
{
    private const string EnglishModelPath = @"C:\Models\ggml-base.en.bin";
    private const string MultilingualModelPath = @"C:\Models\ggml-base.bin";

    [Fact]
    public void TryStart_SkipsWhenPrewarmIsDisabled()
    {
        var service = new RecordingWarmupService();
        var coordinator = new WhisperModelWarmupCoordinator(service, _ => true);
        var settings = LocalSettings() with { PrewarmModelOnWake = false };

        var result = coordinator.TryStart(settings, "startup", CancellationToken.None);

        Assert.False(result.Started);
        Assert.Equal(WhisperModelWarmupStatus.Skipped, result.Status);
        Assert.Contains("disabled", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(service.Requests);
        Assert.Equal(WhisperModelWarmupStatus.Skipped, coordinator.State.Status);
    }

    [Fact]
    public void TryStart_SkipsCloudTranscriptionProviders()
    {
        var service = new RecordingWarmupService();
        var coordinator = new WhisperModelWarmupCoordinator(service, _ => true);
        var settings = LocalSettings() with
        {
            TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
            CloudTranscriptionEndpoint = "https://api.example.test/v1/audio/transcriptions",
            CloudTranscriptionModel = "whisper-large-v3"
        };

        var result = coordinator.TryStart(settings, "startup", CancellationToken.None);

        Assert.False(result.Started);
        Assert.Equal(WhisperModelWarmupStatus.Skipped, result.Status);
        Assert.Contains("local", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(service.Requests);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void TryStart_SkipsMissingModelPath(string modelPath)
    {
        var service = new RecordingWarmupService();
        var coordinator = new WhisperModelWarmupCoordinator(service, _ => true);
        var settings = LocalSettings() with { ModelPath = modelPath };

        var result = coordinator.TryStart(settings, "startup", CancellationToken.None);

        Assert.False(result.Started);
        Assert.Equal(WhisperModelWarmupStatus.Skipped, result.Status);
        Assert.Contains("model path", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(service.Requests);
    }

    [Fact]
    public void TryStart_SkipsWhenModelFileProbeFails()
    {
        var service = new RecordingWarmupService();
        var coordinator = new WhisperModelWarmupCoordinator(service, path => !path.EndsWith(".missing", StringComparison.OrdinalIgnoreCase));
        var settings = LocalSettings() with { ModelPath = @"C:\Models\ggml-base.en.missing" };

        var result = coordinator.TryStart(settings, "startup", CancellationToken.None);

        Assert.False(result.Started);
        Assert.Equal(WhisperModelWarmupStatus.Skipped, result.Status);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(service.Requests);
    }

    [Fact]
    public async Task TryStart_NormalizesLocalWhisperLanguageBeforeWarmup()
    {
        var service = new RecordingWarmupService();
        var coordinator = new WhisperModelWarmupCoordinator(service, _ => true);
        var settings = LocalSettings() with { Language = "auto" };

        var result = coordinator.TryStart(settings, "manual", CancellationToken.None);

        Assert.True(result.Started);
        Assert.NotNull(result.WarmupTask);
        await result.WarmupTask;
        var request = Assert.Single(service.Requests);
        Assert.Equal(EnglishModelPath, request.ModelPath);
        Assert.Equal("en", request.Language);
        Assert.Equal(TranscriptionProviderKind.LocalWhisper, request.Provider);
        Assert.Equal(WhisperModelWarmupStatus.Succeeded, coordinator.State.Status);
        Assert.Equal("manual", coordinator.State.Trigger);
        Assert.Equal(EnglishModelPath, coordinator.State.ModelPath);
    }

    [Fact]
    public async Task TryStart_DoesNotStartDuplicateWarmupWhileOneIsRunning()
    {
        var service = new BlockingWarmupService();
        var coordinator = new WhisperModelWarmupCoordinator(service, _ => true);
        var settings = LocalSettings() with { ModelPath = MultilingualModelPath, Language = "auto" };

        var first = coordinator.TryStart(settings, "startup", CancellationToken.None);
        await service.WaitUntilStartedAsync();
        var second = coordinator.TryStart(settings, "resume", CancellationToken.None);

        Assert.True(first.Started);
        Assert.NotNull(first.WarmupTask);
        Assert.False(second.Started);
        Assert.Equal(WhisperModelWarmupStatus.Warming, second.Status);
        Assert.Contains("already", second.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(service.Requests);

        service.Complete();
        await first.WarmupTask!;
    }

    [Fact]
    public async Task TryStart_KeepsWarmupTaskPendingWhileServiceIsStillRunning()
    {
        var service = new BlockingWarmupService();
        var coordinator = new WhisperModelWarmupCoordinator(service, _ => true);

        var result = coordinator.TryStart(LocalSettings(), "manual", CancellationToken.None);
        await service.WaitUntilStartedAsync();

        Assert.True(result.Started);
        Assert.NotNull(result.WarmupTask);
        Assert.False(result.WarmupTask.IsCompleted);
        Assert.Equal(WhisperModelWarmupStatus.Warming, coordinator.State.Status);

        service.Complete();
        await result.WarmupTask;
        Assert.Equal(WhisperModelWarmupStatus.Succeeded, coordinator.State.Status);
    }

    [Fact]
    public async Task TryStart_ReturnsQuicklyWhenWarmupServiceBlocksSynchronously()
    {
        var service = new SynchronousBlockingWarmupService(TimeSpan.FromMilliseconds(500));
        var coordinator = new WhisperModelWarmupCoordinator(service, _ => true);
        var stopwatch = Stopwatch.StartNew();

        var result = coordinator.TryStart(LocalSettings(), "manual", CancellationToken.None);

        stopwatch.Stop();
        Assert.True(result.Started);
        Assert.NotNull(result.WarmupTask);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromMilliseconds(250));
        await result.WarmupTask;
        Assert.Equal(WhisperModelWarmupStatus.Succeeded, coordinator.State.Status);
    }

    [Fact]
    public async Task TryStart_RecordsFailureStateWithoutThrowingToCaller()
    {
        var service = new ThrowingWarmupService(new InvalidOperationException("native load failed"));
        var coordinator = new WhisperModelWarmupCoordinator(service, _ => true);

        var result = coordinator.TryStart(LocalSettings(), "startup", CancellationToken.None);

        Assert.True(result.Started);
        Assert.NotNull(result.WarmupTask);
        await result.WarmupTask;
        Assert.Equal(WhisperModelWarmupStatus.Failed, coordinator.State.Status);
        Assert.Contains("native load failed", coordinator.State.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static AppSettings LocalSettings() =>
        new()
        {
            ModelPath = EnglishModelPath,
            Language = "fr",
            ImportedWhisperModels =
            [
                new LocalWhisperModel(
                    EnglishModelPath,
                    "ggml-base.en",
                    DateTimeOffset.Parse("2026-05-25T12:00:00Z"))
            ],
            TranscriptionProvider = TranscriptionProviderKind.LocalWhisper
        };

    private sealed class RecordingWarmupService : IWhisperModelWarmupService
    {
        public List<TranscriptionOptions> Requests { get; } = [];

        public Task WarmupAsync(TranscriptionOptions options, CancellationToken cancellationToken)
        {
            Requests.Add(options);
            return Task.CompletedTask;
        }
    }

    private sealed class BlockingWarmupService : IWhisperModelWarmupService
    {
        private readonly TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public List<TranscriptionOptions> Requests { get; } = [];

        public async Task WarmupAsync(TranscriptionOptions options, CancellationToken cancellationToken)
        {
            Requests.Add(options);
            started.TrySetResult();
            await completion.Task.WaitAsync(cancellationToken);
        }

        public Task WaitUntilStartedAsync() =>
            started.Task.WaitAsync(TimeSpan.FromSeconds(5));

        public void Complete() => completion.TrySetResult();
    }

    private sealed class ThrowingWarmupService(Exception exception) : IWhisperModelWarmupService
    {
        public Task WarmupAsync(TranscriptionOptions options, CancellationToken cancellationToken) =>
            Task.FromException(exception);
    }

    private sealed class SynchronousBlockingWarmupService(TimeSpan delay) : IWhisperModelWarmupService
    {
        public Task WarmupAsync(TranscriptionOptions options, CancellationToken cancellationToken)
        {
            Thread.Sleep(delay);
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }
}
