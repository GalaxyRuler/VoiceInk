using System.Runtime.CompilerServices;
using System.Threading.Channels;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class SpeechmaticsLiveTranscriptionPreviewServiceTests
{
    [Fact]
    public async Task TryStartAsync_ReturnsNullWhenProviderIsNotSpeechmatics()
    {
        var service = new SpeechmaticsLiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "speechmatics-secret" },
            () => new FakeStreamingWebSocket());

        var session = await service.TryStartAsync(Settings(providerId: "deepgram"), _ => { }, CancellationToken.None);

        Assert.Null(session);
    }

    [Fact]
    public async Task StartedSession_SendsStartRecognitionAudioAndEndOfStream()
    {
        var socket = new FakeStreamingWebSocket();
        var partials = new List<string>();
        var service = new SpeechmaticsLiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "speechmatics-secret" },
            () => socket);

        var session = await service.TryStartAsync(Settings(language: "en"), partials.Add, CancellationToken.None);

        Assert.NotNull(session);
        Assert.Equal("Bearer speechmatics-secret", socket.AuthorizationHeader);
        Assert.Equal("wss://eu2.rt.speechmatics.com/v2", socket.ConnectUri?.ToString());
        Assert.Contains("StartRecognition", socket.SentText[0]);
        Assert.Contains("\"language\":\"en\"", socket.SentText[0]);
        Assert.Contains("\"encoding\":\"pcm_s16le\"", socket.SentText[0]);

        session.EnqueueAudio(new AudioChunk([7, 8, 9], 16000, 1));
        await socket.WaitForSentBinaryAsync();
        Assert.Equal([7, 8, 9], socket.SentBinary[0]);

        socket.PublishText("""{"message":"AddPartialTranscript","results":[{"type":"word","alternatives":[{"content":"hello"}]},{"type":"word","alternatives":[{"content":"liv"}]}]}""");
        await WaitUntilAsync(() => partials.Contains("hello liv"));

        socket.PublishText("""{"message":"AddTranscript","results":[{"type":"word","alternatives":[{"content":"hello"}]},{"type":"word","alternatives":[{"content":"live"}]},{"type":"punctuation","alternatives":[{"content":"."}]}]}""");
        await WaitUntilAsync(() => partials.Contains("hello live."));

        await session.CompleteAsync(CancellationToken.None);
        await session.DisposeAsync();
        Assert.Contains("""{"message":"EndOfStream"}""", socket.SentText);
        Assert.True(socket.WasClosed);
    }

    private static AppSettings Settings(string providerId = "speechmatics", string language = "auto") =>
        new()
        {
            ShowLiveTranscriptPreview = true,
            TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
            CloudTranscriptionProviderId = providerId,
            CloudTranscriptionModel = "enhanced",
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
