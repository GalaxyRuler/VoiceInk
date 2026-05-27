using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class ElevenLabsLiveTranscriptionPreviewServiceTests
{
    [Fact]
    public async Task TryStartAsync_ReturnsNullWhenProviderIsNotElevenLabs()
    {
        var service = new ElevenLabsLiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "elevenlabs-test-secret" },
            () => new FakeStreamingWebSocket());

        var session = await service.TryStartAsync(Settings(providerId: "deepgram"), _ => { }, CancellationToken.None);

        Assert.Null(session);
    }

    [Fact]
    public async Task TryStartAsync_ReturnsNullWhenApiKeyIsMissing()
    {
        var service = new ElevenLabsLiveTranscriptionPreviewService(
            new FakeSecretStore(),
            () => new FakeStreamingWebSocket());

        var session = await service.TryStartAsync(Settings(), _ => { }, CancellationToken.None);

        Assert.Null(session);
    }

    [Fact]
    public async Task StartedSession_SendsJsonAudioChunksEmitsTranscriptAndCommits()
    {
        var socket = new FakeStreamingWebSocket();
        var partials = new List<string>();
        var service = new ElevenLabsLiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "elevenlabs-test-secret" },
            () => socket);

        var session = await service.TryStartAsync(Settings(), partials.Add, CancellationToken.None);

        Assert.NotNull(session);
        Assert.Equal("elevenlabs-test-secret", socket.Headers["xi-api-key"]);
        Assert.Equal("wss://api.elevenlabs.io/v1/speech-to-text/realtime", socket.ConnectUri?.GetLeftPart(UriPartial.Path));
        Assert.Contains("model_id=scribe_v2_realtime", socket.ConnectUri?.Query);
        Assert.Contains("audio_format=pcm_16000", socket.ConnectUri?.Query);
        Assert.Contains("commit_strategy=manual", socket.ConnectUri?.Query);
        Assert.Contains("language_code=en", socket.ConnectUri?.Query);

        session.EnqueueAudio(new AudioChunk([1, 2, 3, 4], 16000, 1));
        await socket.WaitForSentTextAsync();
        var firstChunk = JsonDocument.Parse(socket.SentText[0]).RootElement;
        Assert.Equal("input_audio_chunk", firstChunk.GetProperty("message_type").GetString());
        Assert.Equal(Convert.ToBase64String([1, 2, 3, 4]), firstChunk.GetProperty("audio_base_64").GetString());
        Assert.False(firstChunk.GetProperty("commit").GetBoolean());
        Assert.Equal(16000, firstChunk.GetProperty("sample_rate").GetInt32());

        socket.PublishText("""{"message_type":"partial_transcript","text":"hello elev"}""");
        await WaitUntilAsync(() => partials.Contains("hello elev"));
        socket.PublishText("""{"message_type":"committed_transcript","text":"hello eleven"}""");
        await WaitUntilAsync(() => partials.Contains("hello eleven"));

        await session.CompleteAsync(CancellationToken.None);
        await session.DisposeAsync();
        var finalChunk = JsonDocument.Parse(socket.SentText[^1]).RootElement;
        Assert.Equal("input_audio_chunk", finalChunk.GetProperty("message_type").GetString());
        Assert.True(finalChunk.GetProperty("commit").GetBoolean());
        Assert.True(socket.WasClosed);
    }

    private static AppSettings Settings(string providerId = "elevenlabs") =>
        new()
        {
            ShowLiveTranscriptPreview = true,
            TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
            CloudTranscriptionProviderId = providerId,
            CloudTranscriptionEndpoint = "https://api.elevenlabs.io/v1/speech-to-text",
            CloudTranscriptionModel = "scribe_v2",
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
        private readonly TaskCompletionSource<object?> sentText =
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
            return Task.CompletedTask;
        }

        public Task SendTextAsync(string text, CancellationToken cancellationToken)
        {
            SentText.Add(text);
            sentText.TrySetResult(null);
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

        public Task WaitForSentTextAsync() => sentText.Task.WaitAsync(TimeSpan.FromSeconds(2));
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
