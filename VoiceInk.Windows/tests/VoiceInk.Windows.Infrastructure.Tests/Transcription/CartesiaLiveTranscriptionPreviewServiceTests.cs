using System.Runtime.CompilerServices;
using System.Threading.Channels;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class CartesiaLiveTranscriptionPreviewServiceTests
{
    [Fact]
    public async Task TryStartAsync_ReturnsNullWhenProviderIsNotCartesia()
    {
        var service = new CartesiaLiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "cartesia-test-secret" },
            () => new FakeStreamingWebSocket());

        var session = await service.TryStartAsync(Settings(providerId: "deepgram"), _ => { }, CancellationToken.None);

        Assert.Null(session);
    }

    [Fact]
    public async Task StartedSession_SendsQueuedAudioEmitsTranscriptAndFinalizes()
    {
        var socket = new FakeStreamingWebSocket();
        var partials = new List<string>();
        var service = new CartesiaLiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "cartesia-test-secret" },
            () => socket);

        var session = await service.TryStartAsync(Settings(), partials.Add, CancellationToken.None);

        Assert.NotNull(session);
        Assert.Equal("cartesia-test-secret", socket.Headers["X-API-Key"]);
        Assert.Equal("2026-03-01", socket.Headers["Cartesia-Version"]);
        Assert.Contains("model=ink-whisper", socket.ConnectUri?.Query);

        session.EnqueueAudio(new AudioChunk([7, 8, 9], 16000, 1));
        await socket.WaitForSentBinaryAsync();
        Assert.Equal([7, 8, 9], socket.SentBinary[0]);

        socket.PublishText("""{"type":"transcript","is_final":false,"text":"hello cart"}""");
        await WaitUntilAsync(() => partials.Contains("hello cart"));
        socket.PublishText("""{"type":"transcript","is_final":true,"text":"hello cartesia"}""");
        await WaitUntilAsync(() => partials.Contains("hello cartesia"));

        await session.CompleteAsync(CancellationToken.None);
        await session.DisposeAsync();
        Assert.Contains("finalize", socket.SentText);
        Assert.Contains("close", socket.SentText);
        Assert.True(socket.WasClosed);
    }

    private static AppSettings Settings(string providerId = "cartesia") =>
        new()
        {
            ShowLiveTranscriptPreview = true,
            TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
            CloudTranscriptionProviderId = providerId,
            CloudTranscriptionEndpoint = "https://api.cartesia.ai/stt",
            CloudTranscriptionModel = "ink-whisper",
            Language = "en"
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

        public Uri? ConnectUri { get; private set; }
        public Dictionary<string, string> Headers { get; } = [];
        public List<byte[]> SentBinary { get; } = [];
        public List<string> SentText { get; } = [];
        public bool WasClosed { get; private set; }

        public Task ConnectAsync(Uri uri, string authorizationHeader, CancellationToken cancellationToken)
        {
            ConnectUri = uri;
            Headers["Authorization"] = authorizationHeader;
            return Task.CompletedTask;
        }

        public Task ConnectAsync(Uri uri, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
        {
            ConnectUri = uri;
            foreach (var header in headers)
            {
                Headers[header.Key] = header.Value;
            }

            return Task.CompletedTask;
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

        public async IAsyncEnumerable<string> ReceiveTextMessagesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
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

        public void Abort() => incoming.Writer.TryComplete();

        public ValueTask DisposeAsync()
        {
            incoming.Writer.TryComplete();
            return ValueTask.CompletedTask;
        }

        public void PublishText(string json) => incoming.Writer.TryWrite(json);

        public Task WaitForSentBinaryAsync() => sentBinary.Task.WaitAsync(TimeSpan.FromSeconds(2));
    }

    private sealed class FakeSecretStore : ISecretStore
    {
        public string? Secret { get; init; }
        public Task<string?> ReadSecretAsync(string name, CancellationToken cancellationToken) => Task.FromResult(Secret);
        public Task SaveSecretAsync(string name, string secret, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task DeleteSecretAsync(string name, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> HasSecretAsync(string name, CancellationToken cancellationToken) => Task.FromResult(!string.IsNullOrWhiteSpace(Secret));
    }
}
