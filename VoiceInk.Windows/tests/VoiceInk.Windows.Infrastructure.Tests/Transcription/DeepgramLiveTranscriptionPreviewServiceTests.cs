using System.Runtime.CompilerServices;
using System.Threading.Channels;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class DeepgramLiveTranscriptionPreviewServiceTests
{
    [Fact]
    public async Task TryStartAsync_ReturnsNullWhenPreviewDisabled()
    {
        var service = new DeepgramLiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "dg-test-secret" },
            () => new FakeStreamingWebSocket());

        var session = await service.TryStartAsync(
            Settings(showLiveTranscriptPreview: false),
            _ => { },
            CancellationToken.None);

        Assert.Null(session);
    }

    [Fact]
    public async Task TryStartAsync_ReturnsNullWhenProviderIsNotDeepgram()
    {
        var service = new DeepgramLiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "dg-test-secret" },
            () => new FakeStreamingWebSocket());

        var session = await service.TryStartAsync(
            Settings(providerId: "groq"),
            _ => { },
            CancellationToken.None);

        Assert.Null(session);
    }

    [Fact]
    public async Task TryStartAsync_ReturnsNullWhenApiKeyIsMissing()
    {
        var service = new DeepgramLiveTranscriptionPreviewService(
            new FakeSecretStore(),
            () => new FakeStreamingWebSocket());

        var session = await service.TryStartAsync(
            Settings(),
            _ => { },
            CancellationToken.None);

        Assert.Null(session);
    }

    [Fact]
    public async Task StartedSession_SendsQueuedAudioAndEmitsPartialTranscripts()
    {
        var socket = new FakeStreamingWebSocket();
        var partials = new List<string>();
        var service = new DeepgramLiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "dg-test-secret" },
            () => socket);

        var session = await service.TryStartAsync(
            Settings(language: "en"),
            partials.Add,
            CancellationToken.None);

        Assert.NotNull(session);
        Assert.Equal("Token dg-test-secret", socket.AuthorizationHeader);
        Assert.NotNull(socket.ConnectUri);
        Assert.Equal("wss", socket.ConnectUri.Scheme);

        session.EnqueueAudio(new AudioChunk([1, 2, 3], 16000, 1));
        await socket.WaitForSentBinaryAsync();
        Assert.Equal([1, 2, 3], socket.SentBinary[0]);

        socket.PublishText("""{"type":"Results","is_final":false,"channel":{"alternatives":[{"transcript":"hello live"}]}}""");
        await WaitUntilAsync(() => partials.Contains("hello live"));

        await session.CompleteAsync(CancellationToken.None);
        await session.DisposeAsync();
        Assert.True(socket.WasClosed);
    }

    [Fact]
    public async Task TryStartAsync_ConnectTimeoutCleansUpSocketAndDoesNotHangStartup()
    {
        var socket = new FakeStreamingWebSocket
        {
            ConnectTask = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously).Task
        };
        var service = new DeepgramLiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "dg-test-secret" },
            () => socket,
            connectionTimeout: TimeSpan.FromMilliseconds(25));

        var startedAt = DateTimeOffset.UtcNow;
        var ex = await Assert.ThrowsAsync<TimeoutException>(
            () => service.TryStartAsync(Settings(), _ => { }, CancellationToken.None));

        Assert.Equal("Deepgram live transcript preview connection timed out.", ex.Message);
        Assert.True(DateTimeOffset.UtcNow - startedAt < TimeSpan.FromSeconds(1));
        Assert.True(socket.WasAborted);
        Assert.True(socket.WasDisposed);
    }

    [Fact]
    public async Task TryStartAsync_FailedConnectCleansUpSocketBeforeRethrowing()
    {
        var socket = new FakeStreamingWebSocket
        {
            ConnectException = new InvalidOperationException("connect failed")
        };
        var service = new DeepgramLiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "dg-test-secret" },
            () => socket);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TryStartAsync(Settings(), _ => { }, CancellationToken.None));

        Assert.Equal("connect failed", ex.Message);
        Assert.True(socket.WasAborted);
        Assert.True(socket.WasDisposed);
    }

    [Fact]
    public async Task CompleteAsync_ReturnsAndAbortsSocketWhenSendDoesNotComplete()
    {
        var socket = new FakeStreamingWebSocket { HangSend = true };
        var service = new DeepgramLiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "dg-test-secret" },
            () => socket,
            cleanupTimeout: TimeSpan.FromMilliseconds(25));

        var session = await service.TryStartAsync(Settings(), _ => { }, CancellationToken.None);

        Assert.NotNull(session);
        session.EnqueueAudio(new AudioChunk([1, 2, 3], 16000, 1));
        await socket.WaitForSendStartedAsync();
        await session.CompleteAsync(CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(1));

        Assert.True(socket.WasAborted);
        await session.DisposeAsync();
    }

    private static AppSettings Settings(
        bool showLiveTranscriptPreview = true,
        string providerId = "deepgram",
        string language = "auto") =>
        new()
        {
            ShowLiveTranscriptPreview = showLiveTranscriptPreview,
            TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
            CloudTranscriptionProviderId = providerId,
            CloudTranscriptionEndpoint = "https://api.deepgram.com/v1/listen",
            CloudTranscriptionModel = "nova-3",
            Language = language
        };

    private static async Task WaitUntilAsync(Func<bool> predicate)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        while (!predicate())
        {
            await Task.Delay(10, timeout.Token);
        }
    }

    private sealed class FakeStreamingWebSocket : IStreamingWebSocket
    {
        private readonly Channel<string> incoming = Channel.CreateUnbounded<string>();
        private readonly TaskCompletionSource<object?> sentBinary =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<object?> sendStarted =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task? ConnectTask { get; init; }
        public Exception? ConnectException { get; init; }
        public bool HangSend { get; init; }
        public Uri? ConnectUri { get; private set; }
        public string? AuthorizationHeader { get; private set; }
        public List<byte[]> SentBinary { get; } = [];
        public bool WasClosed { get; private set; }
        public bool WasAborted { get; private set; }
        public bool WasDisposed { get; private set; }

        public async Task ConnectAsync(
            Uri uri,
            string authorizationHeader,
            CancellationToken cancellationToken)
        {
            ConnectUri = uri;
            AuthorizationHeader = authorizationHeader;
            if (ConnectException is not null)
            {
                throw ConnectException;
            }

            if (ConnectTask is not null)
            {
                await ConnectTask.WaitAsync(cancellationToken);
            }
        }

        public Task SendBinaryAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken)
        {
            SentBinary.Add(bytes.ToArray());
            sendStarted.TrySetResult(null);
            if (HangSend)
            {
                return new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously).Task;
            }

            sentBinary.TrySetResult(null);
            return Task.CompletedTask;
        }

        public async IAsyncEnumerable<string> ReceiveTextMessagesAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (await incoming.Reader.WaitToReadAsync(cancellationToken))
            {
                while (incoming.Reader.TryRead(out var message))
                {
                    yield return message;
                }
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            WasClosed = true;
            incoming.Writer.TryComplete();
            return Task.CompletedTask;
        }

        public void Abort()
        {
            WasAborted = true;
            incoming.Writer.TryComplete();
        }

        public ValueTask DisposeAsync()
        {
            WasDisposed = true;
            incoming.Writer.TryComplete();
            return ValueTask.CompletedTask;
        }

        public void PublishText(string json)
        {
            incoming.Writer.TryWrite(json);
        }

        public Task WaitForSentBinaryAsync() =>
            sentBinary.Task.WaitAsync(TimeSpan.FromSeconds(2));

        public Task WaitForSendStartedAsync() =>
            sendStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
    }

    private sealed class FakeSecretStore : ISecretStore
    {
        public string? Secret { get; init; }

        public Task<string?> ReadSecretAsync(string name, CancellationToken cancellationToken) =>
            Task.FromResult(Secret);

        public Task SaveSecretAsync(string name, string secret, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task DeleteSecretAsync(string name, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<bool> HasSecretAsync(string name, CancellationToken cancellationToken) =>
            Task.FromResult(!string.IsNullOrWhiteSpace(Secret));
    }
}
