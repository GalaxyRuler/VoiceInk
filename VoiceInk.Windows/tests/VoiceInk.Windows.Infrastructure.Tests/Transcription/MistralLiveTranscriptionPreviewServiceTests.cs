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

public sealed class MistralLiveTranscriptionPreviewServiceTests
{
    [Fact]
    public async Task TryStartAsync_ReturnsNullWhenProviderIsNotMistral()
    {
        var service = new MistralLiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "mistral-test-secret" },
            () => new FakeStreamingWebSocket());

        var session = await service.TryStartAsync(Settings(providerId: "deepgram"), _ => { }, CancellationToken.None);

        Assert.Null(session);
    }

    [Fact]
    public async Task TryStartAsync_ReturnsNullWhenApiKeyIsMissing()
    {
        var service = new MistralLiveTranscriptionPreviewService(
            new FakeSecretStore(),
            () => new FakeStreamingWebSocket());

        var session = await service.TryStartAsync(Settings(), _ => { }, CancellationToken.None);

        Assert.Null(session);
    }

    [Fact]
    public async Task StartedSession_SendsJsonAudioChunksEmitsTranscriptAndCompletes()
    {
        var socket = new FakeStreamingWebSocket();
        var partials = new List<string>();
        var service = new MistralLiveTranscriptionPreviewService(
            new FakeSecretStore { Secret = "mistral-test-secret" },
            () => socket);

        var session = await service.TryStartAsync(Settings(), partials.Add, CancellationToken.None);

        Assert.NotNull(session);
        Assert.Equal("Bearer mistral-test-secret", socket.Headers["Authorization"]);
        Assert.Equal("wss://api.mistral.ai/v1/audio/transcriptions/realtime", socket.ConnectUri?.GetLeftPart(UriPartial.Path));
        Assert.Contains("model=voxtral-mini-transcribe-realtime-2602", socket.ConnectUri?.Query);

        session.EnqueueAudio(new AudioChunk([1, 2, 3, 4], 16000, 1));
        await socket.WaitForSentTextCountAsync(2);

        var sessionUpdate = JsonDocument.Parse(socket.SentText[0]).RootElement;
        Assert.Equal("session.update", sessionUpdate.GetProperty("type").GetString());
        var audioFormat = sessionUpdate.GetProperty("session").GetProperty("audio_format");
        Assert.Equal("pcm_s16le", audioFormat.GetProperty("encoding").GetString());
        Assert.Equal(16000, audioFormat.GetProperty("sample_rate").GetInt32());

        var firstChunk = JsonDocument.Parse(socket.SentText[1]).RootElement;
        Assert.Equal("input_audio.append", firstChunk.GetProperty("type").GetString());
        Assert.Equal(Convert.ToBase64String([1, 2, 3, 4]), firstChunk.GetProperty("audio").GetString());

        socket.PublishText("""{"type":"transcription.text.delta","text":"hello "}""");
        await WaitUntilAsync(() => partials.Contains("hello "));
        socket.PublishText("""{"type":"transcription.text.delta","text":"mistral"}""");
        await WaitUntilAsync(() => partials.Contains("hello mistral"));
        socket.PublishText("""{"type":"transcription.done","text":"hello mistral final","model":"voxtral-mini-transcribe-realtime-2602","usage":{"prompt_tokens":0,"completion_tokens":0,"total_tokens":0},"language":"en"}""");
        await WaitUntilAsync(() => partials.Contains("hello mistral final"));

        await session.CompleteAsync(CancellationToken.None);
        await session.DisposeAsync();

        Assert.Contains(socket.SentText, item => JsonDocument.Parse(item).RootElement.GetProperty("type").GetString() == "input_audio.flush");
        Assert.Contains(socket.SentText, item => JsonDocument.Parse(item).RootElement.GetProperty("type").GetString() == "input_audio.end");
        Assert.True(socket.WasClosed);
    }

    private static AppSettings Settings(string providerId = "mistral") =>
        new()
        {
            ShowLiveTranscriptPreview = true,
            TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
            CloudTranscriptionProviderId = providerId,
            CloudTranscriptionEndpoint = "https://api.mistral.ai/v1/audio/transcriptions",
            CloudTranscriptionModel = "voxtral-mini-latest",
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

        public async Task WaitForSentTextCountAsync(int count)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            while (SentText.Count < count)
            {
                await sentText.Task.WaitAsync(timeout.Token);
                if (SentText.Count < count)
                {
                    await Task.Delay(10, timeout.Token);
                }
            }
        }
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
