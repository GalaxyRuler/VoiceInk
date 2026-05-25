using System.Runtime.CompilerServices;
using System.Threading.Channels;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class AssemblyAILiveTranscriptionPreviewServiceTests
{
    [Fact]
    public async Task TryStartAsync_ReturnsNullWhenProviderIsNotAssemblyAi()
    {
        var service = new AssemblyAILiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "aai-test-secret" },
            () => new FakeStreamingWebSocket());

        var session = await service.TryStartAsync(
            Settings(providerId: "deepgram"),
            _ => { },
            CancellationToken.None);

        Assert.Null(session);
    }

    [Fact]
    public async Task TryStartAsync_ReturnsNullWhenApiKeyIsMissing()
    {
        var service = new AssemblyAILiveTranscriptionPreviewService(
            new FakeSecretStore(),
            () => new FakeStreamingWebSocket());

        var session = await service.TryStartAsync(
            Settings(),
            _ => { },
            CancellationToken.None);

        Assert.Null(session);
    }

    [Fact]
    public async Task StartedSession_SendsQueuedAudioAndEmitsTranscriptTurns()
    {
        var socket = new FakeStreamingWebSocket();
        var partials = new List<string>();
        var service = new AssemblyAILiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "aai-test-secret" },
            () => socket);

        var session = await service.TryStartAsync(
            Settings(language: "en"),
            partials.Add,
            CancellationToken.None);

        Assert.NotNull(session);
        Assert.Equal("aai-test-secret", socket.AuthorizationHeader);
        Assert.NotNull(socket.ConnectUri);
        Assert.Equal("wss", socket.ConnectUri.Scheme);
        Assert.Contains("speech_model=u3-rt-pro", socket.ConnectUri.Query);

        session.EnqueueAudio(new AudioChunk([4, 5, 6], 16000, 1));
        await socket.WaitForSentBinaryAsync();
        Assert.Equal([4, 5, 6], socket.SentBinary[0]);

        socket.PublishText("""{"type":"Turn","transcript":"hello liv","end_of_turn":false}""");
        await WaitUntilAsync(() => partials.Contains("hello liv"));

        socket.PublishText("""{"type":"Turn","transcript":"hello live","end_of_turn":true}""");
        await WaitUntilAsync(() => partials.Contains("hello live"));

        await session.CompleteAsync(CancellationToken.None);
        await session.DisposeAsync();
        Assert.Contains("""{"type":"Terminate"}""", socket.SentText);
        Assert.True(socket.WasClosed);
    }

    [Fact]
    public async Task TryStartAsync_ConnectTimeoutCleansUpSocketAndDoesNotHangStartup()
    {
        var socket = new FakeStreamingWebSocket
        {
            ConnectTask = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously).Task
        };
        var service = new AssemblyAILiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "aai-test-secret" },
            () => socket,
            connectionTimeout: TimeSpan.FromMilliseconds(25));

        var ex = await Assert.ThrowsAsync<TimeoutException>(
            () => service.TryStartAsync(Settings(), _ => { }, CancellationToken.None));

        Assert.Equal("AssemblyAI live transcript preview connection timed out.", ex.Message);
        Assert.True(socket.WasAborted);
        Assert.True(socket.WasDisposed);
    }

    private static AppSettings Settings(
        string providerId = "assemblyai",
        string language = "auto") =>
        new()
        {
            ShowLiveTranscriptPreview = true,
            TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
            CloudTranscriptionProviderId = providerId,
            CloudTranscriptionEndpoint = "https://streaming.assemblyai.com/v3/ws",
            CloudTranscriptionModel = "universal-3-pro",
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

        public Task? ConnectTask { get; init; }
        public Uri? ConnectUri { get; private set; }
        public string? AuthorizationHeader { get; private set; }
        public List<byte[]> SentBinary { get; } = [];
        public List<string> SentText { get; } = [];
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
            if (ConnectTask is not null)
            {
                await ConnectTask.WaitAsync(cancellationToken);
            }
        }

        public Task SendBinaryAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken)
        {
            SentBinary.Add(bytes.ToArray());
            sentBinary.TrySetResult(null);
            return Task.CompletedTask;
        }

        public Task SendTextAsync(string text, CancellationToken cancellationToken)
        {
            SentText.Add(text);
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
