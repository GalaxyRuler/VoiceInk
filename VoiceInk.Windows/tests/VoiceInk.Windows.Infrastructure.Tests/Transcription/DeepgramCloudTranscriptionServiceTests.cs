using System.Net;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class DeepgramCloudTranscriptionServiceTests
{
    [Fact]
    public async Task TranscribeAsync_SendsRawWavRequestWithDeepgramToken()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(
                HttpStatusCode.OK,
                """{"results":{"channels":[{"alternatives":[{"transcript":"Hello from Deepgram"}]}]}}"""));
        var secrets = new FakeSecretStore { Secret = "dg-test-secret" };
        var service = new DeepgramCloudTranscriptionService(new HttpClient(handler), secrets);

        var result = await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(language: "en"),
            CancellationToken.None);

        Assert.Equal("Hello from Deepgram", result.Text);
        Assert.Equal("deepgram", result.ProviderName);
        Assert.Equal("VoiceInk.Windows.Transcription.OpenAICompatible.Deepgram.ApiKey", secrets.LastReadName);
        Assert.Single(handler.Requests);
        var request = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal(
            "https://api.deepgram.com/v1/listen?model=nova-3&smart_format=true&language=en",
            request.RequestUri?.ToString());
        Assert.Equal("Token", request.Headers.Authorization?.Scheme);
        Assert.Equal("dg-test-secret", request.Headers.Authorization?.Parameter);
        Assert.Equal("audio/wav", request.Content?.Headers.ContentType?.MediaType);
        Assert.Equal(audioFile.Bytes, handler.Bodies[0]);
    }

    [Fact]
    public async Task TranscribeAsync_OmitsAutomaticLanguage()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(
                HttpStatusCode.OK,
                """{"results":{"channels":[{"alternatives":[{"transcript":"Auto language"}]}]}}"""));
        var service = new DeepgramCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "dg-test-secret" });

        await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(language: "auto"),
            CancellationToken.None);

        Assert.Equal(
            "https://api.deepgram.com/v1/listen?model=nova-3&smart_format=true",
            handler.Requests[0].RequestUri?.ToString());
    }

    [Fact]
    public async Task TranscribeAsync_PreservesAdvancedEndpointQueryOptionsWithoutDuplicateDefaults()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(
                HttpStatusCode.OK,
                """{"results":{"channels":[{"alternatives":[{"transcript":"Advanced query"}]}]}}"""));
        var service = new DeepgramCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "dg-test-secret" });

        await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(
                endpoint: "https://api.deepgram.com/v1/listen?smart_format=false&language=es&diarize_model=latest&paragraphs=true&utterances=true",
                language: "en"),
            CancellationToken.None);

        Assert.Equal(
            "https://api.deepgram.com/v1/listen?smart_format=false&language=es&diarize_model=latest&paragraphs=true&utterances=true&model=nova-3",
            handler.Requests[0].RequestUri?.ToString());
    }

    [Theory]
    [InlineData("", "nova-3", "Cloud transcription endpoint is invalid.")]
    [InlineData("https://api.deepgram.com/v1/listen", "", "Cloud transcription model is required.")]
    public async Task TranscribeAsync_InvalidConfigurationFailsBeforeHttp(
        string endpoint,
        string model,
        string expectedMessage)
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(
                HttpStatusCode.OK,
                """{"results":{"channels":[{"alternatives":[{"transcript":"unused"}]}]}}"""));
        var service = new DeepgramCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "dg-test-secret" });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(
                Audio(audioFile.Path),
                Options(endpoint: endpoint, model: model),
                CancellationToken.None));

        Assert.Equal(expectedMessage, ex.Message);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task TranscribeAsync_MissingApiKeyFailsBeforeHttp()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(
                HttpStatusCode.OK,
                """{"results":{"channels":[{"alternatives":[{"transcript":"unused"}]}]}}"""));
        var service = new DeepgramCloudTranscriptionService(new HttpClient(handler), new FakeSecretStore());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Contains("not configured", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task TranscribeAsync_HttpErrorDoesNotLeakResponseBodyOrApiKey()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.BadRequest, """{"err":"dg-test-secret bad request"}"""));
        var service = new DeepgramCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "dg-test-secret" });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Equal("Deepgram transcription provider returned HTTP 400.", ex.Message);
        Assert.DoesNotContain("dg-test-secret", ex.Message);
        Assert.DoesNotContain("bad request", ex.Message);
    }

    [Fact]
    public async Task TranscribeAsync_InvalidJsonReturnsSanitizedError()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"unexpected":"shape"}"""));
        var service = new DeepgramCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "dg-test-secret" });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Equal("Deepgram transcription provider returned an unexpected response.", ex.Message);
    }

    [Theory]
    [InlineData("""{"results":{"channels":[]}}""")]
    [InlineData("""{"results":{"channels":[{"alternatives":[]}]}}""")]
    [InlineData("""{"results":{"channels":[{"alternatives":[{}]}]}}""")]
    public async Task TranscribeAsync_UnexpectedDeepgramJsonShapesReturnSanitizedError(string json)
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, json));
        var service = new DeepgramCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "dg-test-secret" });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Equal("Deepgram transcription provider returned an unexpected response.", ex.Message);
    }

    private static AudioCaptureResult Audio(string filePath) =>
        new(filePath, TimeSpan.FromSeconds(2), SampleRate: 16000, ChannelCount: 1);

    private static TranscriptionOptions Options(
        string endpoint = "https://api.deepgram.com/v1/listen",
        string model = "nova-3",
        string language = "auto") =>
        new(
            ModelPath: string.Empty,
            language,
            Prompt: string.Empty,
            TranscriptionProviderKind.OpenAICompatible,
            endpoint,
            model,
            CloudProviderId: "deepgram");

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json) =>
        new(statusCode)
        {
            Content = new StringContent(json)
        };

    private sealed class TempAudioFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"voiceink-deepgram-{Guid.NewGuid():N}.wav");

        public byte[] Bytes { get; } = "sample audio bytes"u8.ToArray();

        public TempAudioFile()
        {
            File.WriteAllBytes(Path, Bytes);
        }

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }

    private sealed class QueueHttpMessageHandler(params Func<HttpRequestMessage, HttpResponseMessage>[] responses)
        : HttpMessageHandler
    {
        private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> responses = new(responses);

        public List<HttpRequestMessage> Requests { get; } = [];
        public List<byte[]> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            Bodies.Add(request.Content is null
                ? []
                : await request.Content.ReadAsByteArrayAsync(cancellationToken));

            return responses.Count > 0
                ? responses.Dequeue()(request)
                : new HttpResponseMessage(HttpStatusCode.OK);
        }
    }

    private sealed class FakeSecretStore : ISecretStore
    {
        public string? Secret { get; init; }
        public string? LastReadName { get; private set; }
        public List<string> ReadNames { get; } = [];

        public Task<string?> ReadSecretAsync(string name, CancellationToken cancellationToken)
        {
            LastReadName = name;
            ReadNames.Add(name);
            return Task.FromResult(Secret);
        }

        public Task SaveSecretAsync(string name, string secret, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task DeleteSecretAsync(string name, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<bool> HasSecretAsync(string name, CancellationToken cancellationToken) =>
            Task.FromResult(!string.IsNullOrWhiteSpace(Secret));
    }
}
