using System.Runtime.CompilerServices;
using System.Threading.Channels;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class SonioxLiveTranscriptionPreviewServiceTests
{
    [Fact]
    public async Task TryStartAsync_ReturnsNullWhenProviderIsNotSoniox()
    {
        var service = new SonioxLiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "soniox-secret" },
            () => new FakeStreamingWebSocket());

        var session = await service.TryStartAsync(Settings(providerId: "deepgram"), _ => { }, CancellationToken.None);

        Assert.Null(session);
    }

    [Fact]
    public async Task StartedSession_SendsConfigAudioAndEmptyFinalFrame()
    {
        var socket = new FakeStreamingWebSocket();
        var partials = new List<string>();
        var service = new SonioxLiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "soniox-secret" },
            () => socket);

        var session = await service.TryStartAsync(Settings(language: "en"), partials.Add, CancellationToken.None);

        Assert.NotNull(session);
        Assert.Equal("wss://stt-rt.soniox.com/transcribe-websocket", socket.ConnectUri?.ToString());
        Assert.Empty(socket.AuthorizationHeader ?? string.Empty);
        Assert.Single(socket.SentText);
        Assert.Contains("\"api_key\":\"soniox-secret\"", socket.SentText[0]);
        Assert.Contains("\"model\":\"stt-rt-preview\"", socket.SentText[0]);
        Assert.Contains("\"audio_format\":\"s16le\"", socket.SentText[0]);
        Assert.Contains("\"num_channels\":1", socket.SentText[0]);
        Assert.Contains("\"sample_rate\":16000", socket.SentText[0]);
        Assert.Contains("\"language_hints\":[\"en\"]", socket.SentText[0]);

        session.EnqueueAudio(new AudioChunk([10, 11, 12], 16000, 1));
        await socket.WaitForSentBinaryAsync();
        Assert.Equal([10, 11, 12], socket.SentBinary[0]);

        socket.PublishText("""{"tokens":[{"text":"hello","is_final":true},{"text":" liv","is_final":false}],"total_audio_proc_ms":300}""");
        await WaitUntilAsync(() => partials.Contains("hello liv"));

        socket.PublishText("""{"tokens":[{"text":"hello","is_final":true},{"text":" live","is_final":true}],"final_audio_proc_ms":700}""");
        await WaitUntilAsync(() => partials.Contains("hello live"));

        await session.CompleteAsync(CancellationToken.None);
        await session.DisposeAsync();
        Assert.Contains(Array.Empty<byte>(), socket.SentBinary);
        Assert.True(socket.WasClosed);
    }

    private static AppSettings Settings(string providerId = "soniox", string language = "auto") =>
        new()
        {
            ShowLiveTranscriptPreview = true,
            TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
            CloudTranscriptionProviderId = providerId,
            CloudTranscriptionModel = "stt-async-v4",
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

        public Uri? ConnectUri { get; private set; }
        public string? AuthorizationHeader { get; private set; }
        public List<byte[]> SentBinary { get; } = [];
        public List<string> SentText { get; } = [];
        public bool WasClosed { get; private set; }
        public bool WasAborted { get; private set; }
        public bool WasDisposed { get; private set; }

        public Task ConnectAsync(Uri uri, string authorizationHeader, CancellationToken cancellationToken)
        {
            ConnectUri = uri;
            AuthorizationHeader = authorizationHeader;
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

        public void PublishText(string json) => incoming.Writer.TryWrite(json);

        public Task WaitForSentBinaryAsync() => sentBinary.Task.WaitAsync(TimeSpan.FromSeconds(2));
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
