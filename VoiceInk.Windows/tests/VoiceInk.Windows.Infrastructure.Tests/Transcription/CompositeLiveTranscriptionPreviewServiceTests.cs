using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class CompositeLiveTranscriptionPreviewServiceTests
{
    [Fact]
    public async Task TryStartAsync_ReturnsFirstProviderSessionThatCanStart()
    {
        var first = new FakeLiveTranscriptionPreviewService(null);
        var expected = new FakeLiveTranscriptionPreviewSession();
        var second = new FakeLiveTranscriptionPreviewService(expected);
        var service = new CompositeLiveTranscriptionPreviewService([first, second]);
        var settings = new AppSettings { ShowLiveTranscriptPreview = true };

        var session = await service.TryStartAsync(settings, _ => { }, CancellationToken.None);

        Assert.Same(expected, session);
        Assert.Equal(1, first.StartCount);
        Assert.Equal(1, second.StartCount);
    }

    [Fact]
    public async Task TryStartAsync_ReturnsNullWhenNoProviderCanStart()
    {
        var service = new CompositeLiveTranscriptionPreviewService(
            [
                new FakeLiveTranscriptionPreviewService(null),
                new FakeLiveTranscriptionPreviewService(null)
            ]);

        var session = await service.TryStartAsync(
            new AppSettings { ShowLiveTranscriptPreview = true },
            _ => { },
            CancellationToken.None);

        Assert.Null(session);
    }

    private sealed class FakeLiveTranscriptionPreviewService(
        ILiveTranscriptionPreviewSession? session) : ILiveTranscriptionPreviewService
    {
        public int StartCount { get; private set; }

        public Task<ILiveTranscriptionPreviewSession?> TryStartAsync(
            AppSettings settings,
            Action<string> partialTranscriptUpdated,
            CancellationToken cancellationToken)
        {
            StartCount++;
            return Task.FromResult(session);
        }
    }

    private sealed class FakeLiveTranscriptionPreviewSession : ILiveTranscriptionPreviewSession
    {
        public void EnqueueAudio(AudioChunk chunk)
        {
        }

        public Task CompleteAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public ValueTask DisposeAsync() =>
            ValueTask.CompletedTask;
    }
}
