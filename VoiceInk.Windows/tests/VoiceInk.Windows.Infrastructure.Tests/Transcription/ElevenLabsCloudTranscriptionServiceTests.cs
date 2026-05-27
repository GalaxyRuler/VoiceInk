using System.Net;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class ElevenLabsCloudTranscriptionServiceTests
{
    [Fact]
    public async Task TranscribeAsync_SendsMultipartRequestWithXiApiKey()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"text":"Hello from Scribe"}"""));
        var secrets = new FakeSecretStore { Secret = "el-test-secret" };
        var service = new ElevenLabsCloudTranscriptionService(new HttpClient(handler), secrets);

        var result = await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(language: "en"),
            CancellationToken.None);

        Assert.Equal("Hello from Scribe", result.Text);
        Assert.Equal("elevenlabs", result.ProviderName);
        Assert.Equal("VoiceInk.Windows.Transcription.OpenAICompatible.ElevenLabs.ApiKey", secrets.LastReadName);
        Assert.Single(handler.Requests);
        var request = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://api.elevenlabs.io/v1/speech-to-text", request.RequestUri?.ToString());
        Assert.False(request.Headers.Contains("Authorization"));
        Assert.Equal(["el-test-secret"], request.Headers.GetValues("xi-api-key"));
        Assert.StartsWith("multipart/form-data", request.Content?.Headers.ContentType?.MediaType);

        var body = handler.Bodies[0];
        Assert.Contains("name=file", body);
        Assert.Contains($"filename={Path.GetFileName(audioFile.Path)}", body);
        Assert.Contains("sample audio bytes", body);
        Assert.Contains("name=model_id", body);
        Assert.Contains("scribe_v2", body);
        Assert.Contains("name=language_code", body);
        Assert.Contains("en", body);
    }

    [Fact]
    public async Task TranscribeAsync_OmitsAutomaticLanguage()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"text":"Auto language"}"""));
        var service = new ElevenLabsCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "el-test-secret" });

        await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(language: "auto"),
            CancellationToken.None);

        Assert.DoesNotContain("name=language_code", handler.Bodies[0]);
    }

    [Fact]
    public async Task TranscribeAsync_MapsEndpointQueryOptionsToMultipartFields()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"text":"Diarized transcript"}"""));
        var service = new ElevenLabsCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "el-test-secret" });

        await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(endpoint: "https://api.elevenlabs.io/v1/speech-to-text?diarize=true&tag_audio_events=false"),
            CancellationToken.None);

        Assert.Equal("https://api.elevenlabs.io/v1/speech-to-text", handler.Requests[0].RequestUri?.ToString());
        Assert.Contains("name=diarize", handler.Bodies[0]);
        Assert.Contains("true", handler.Bodies[0]);
        Assert.Contains("name=tag_audio_events", handler.Bodies[0]);
        Assert.Contains("false", handler.Bodies[0]);
    }

    [Fact]
    public async Task TranscribeAsync_MissingApiKeyFailsBeforeHttp()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"text":"unused"}"""));
        var service = new ElevenLabsCloudTranscriptionService(new HttpClient(handler), new FakeSecretStore());

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
            _ => JsonResponse(HttpStatusCode.BadRequest, """{"detail":"el-test-secret bad request"}"""));
        var service = new ElevenLabsCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "el-test-secret" });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Equal("ElevenLabs transcription provider returned HTTP 400.", ex.Message);
        Assert.DoesNotContain("el-test-secret", ex.Message);
        Assert.DoesNotContain("bad request", ex.Message);
    }

    [Fact]
    public async Task TranscribeAsync_InvalidJsonReturnsSanitizedError()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"unexpected":"shape"}"""));
        var service = new ElevenLabsCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "el-test-secret" });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Equal("ElevenLabs transcription provider returned no text.", ex.Message);
    }

    private static AudioCaptureResult Audio(string filePath) =>
        new(filePath, TimeSpan.FromSeconds(2), SampleRate: 16000, ChannelCount: 1);

    private static TranscriptionOptions Options(
        string language = "auto",
        string endpoint = "https://api.elevenlabs.io/v1/speech-to-text") =>
        new(
            ModelPath: string.Empty,
            language,
            Prompt: string.Empty,
            TranscriptionProviderKind.OpenAICompatible,
            endpoint,
            "scribe_v2",
            CloudProviderId: "elevenlabs");

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json) =>
        new(statusCode)
        {
            Content = new StringContent(json)
        };

    private sealed class TempAudioFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"voiceink-elevenlabs-{Guid.NewGuid():N}.wav");

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
        public List<string> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            Bodies.Add(request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken));

            return responses.Count > 0
                ? responses.Dequeue()(request)
                : new HttpResponseMessage(HttpStatusCode.OK);
        }
    }

    private sealed class FakeSecretStore : ISecretStore
    {
        public string? Secret { get; init; }
        public string? LastReadName { get; private set; }

        public Task<string?> ReadSecretAsync(string name, CancellationToken cancellationToken)
        {
            LastReadName = name;
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
