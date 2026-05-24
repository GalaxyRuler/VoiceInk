using System.Net;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class OpenAICompatibleCloudTranscriptionServiceTests
{
    [Fact]
    public async Task TranscribeAsync_SendsMultipartRequestWithBearerToken()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"text":"Hello from cloud"}"""));
        var secrets = new FakeSecretStore { Secret = "sk-test-secret" };
        var service = new OpenAICompatibleCloudTranscriptionService(new HttpClient(handler), secrets);

        var result = await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(language: "en", prompt: "Important Vocabulary: VoiceInk"),
            CancellationToken.None);

        Assert.Equal("Hello from cloud", result.Text);
        Assert.Equal("openai-compatible", result.ProviderName);
        Assert.Equal(OpenAICompatibleCloudTranscriptionService.SecretName, secrets.LastReadName);
        Assert.Single(handler.Requests);
        var request = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://api.example.test/v1/audio/transcriptions", request.RequestUri?.ToString());
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal("sk-test-secret", request.Headers.Authorization?.Parameter);
        Assert.StartsWith("multipart/form-data", request.Content?.Headers.ContentType?.MediaType);

        var body = handler.Bodies[0];
        Assert.Contains("name=file", body);
        Assert.Contains($"filename={Path.GetFileName(audioFile.Path)}", body);
        Assert.Contains("sample audio bytes", body);
        Assert.Contains("name=model", body);
        Assert.Contains("gpt-4o-transcribe", body);
        Assert.Contains("name=response_format", body);
        Assert.Contains("json", body);
        Assert.Contains("name=language", body);
        Assert.Contains("en", body);
        Assert.Contains("name=prompt", body);
        Assert.Contains("Important Vocabulary: VoiceInk", body);
    }

    [Fact]
    public async Task TranscribeAsync_OmitsLanguageAndPromptWhenBlankOrAutomatic()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"text":"No hints"}"""));
        var service = new OpenAICompatibleCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "sk-test-secret" });

        await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(language: "auto", prompt: " "),
            CancellationToken.None);

        var body = handler.Bodies[0];
        Assert.DoesNotContain("name=language", body);
        Assert.DoesNotContain("name=prompt", body);
    }

    [Theory]
    [InlineData("", "gpt-4o-transcribe", "Cloud transcription endpoint is invalid.")]
    [InlineData("https://api.example.test/v1/audio/transcriptions", "", "Cloud transcription model is required.")]
    public async Task TranscribeAsync_InvalidConfigurationFailsBeforeHttp(
        string endpoint,
        string model,
        string expectedMessage)
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"text":"unused"}"""));
        var service = new OpenAICompatibleCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "sk-test-secret" });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(
                Audio(audioFile.Path),
                Options(endpoint: endpoint, model: model),
                CancellationToken.None));

        Assert.Equal(expectedMessage, ex.Message);
        Assert.Empty(handler.Requests);
    }

    [Theory]
    [InlineData(
        "http://api.example.test/v1/audio/transcriptions",
        "Cloud transcription endpoint must use HTTPS unless it targets localhost.")]
    [InlineData(
        "https://sk-test-secret@api.example.test/v1/audio/transcriptions",
        "Cloud transcription endpoint must not contain credentials.")]
    [InlineData(
        "https://api.example.test/v1/audio/transcriptions?api_key=sk-test-secret",
        "Cloud transcription endpoint must not contain API keys or tokens in the query string.")]
    public async Task TranscribeAsync_InsecureOrCredentialBearingEndpointFailsBeforeHttp(
        string endpoint,
        string expectedMessage)
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"text":"unused"}"""));
        var service = new OpenAICompatibleCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "sk-test-secret" });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(
                Audio(audioFile.Path),
                Options(endpoint: endpoint),
                CancellationToken.None));

        Assert.Equal(expectedMessage, ex.Message);
        Assert.DoesNotContain("sk-test-secret", ex.Message);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task TranscribeAsync_AllowsHttpLoopbackEndpointForLocalDevelopment()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"text":"Local dev"}"""));
        var service = new OpenAICompatibleCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "sk-test-secret" });

        var result = await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(endpoint: "http://localhost:11434/v1/audio/transcriptions"),
            CancellationToken.None);

        Assert.Equal("Local dev", result.Text);
        Assert.Equal("http://localhost:11434/v1/audio/transcriptions", handler.Requests[0].RequestUri?.ToString());
    }

    [Fact]
    public async Task TranscribeAsync_ReadsProviderSpecificSecret()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"text":"Groq text"}"""));
        var secrets = new FakeSecretStore
        {
            Secrets =
            {
                ["VoiceInk.Windows.Transcription.OpenAICompatible.Custom.ApiKey"] = "custom-secret",
                ["VoiceInk.Windows.Transcription.OpenAICompatible.Groq.ApiKey"] = "gsk-test-secret"
            }
        };
        var service = new OpenAICompatibleCloudTranscriptionService(new HttpClient(handler), secrets);

        await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(
                endpoint: "https://api.groq.com/openai/v1/audio/transcriptions",
                model: "whisper-large-v3-turbo",
                providerId: "groq"),
            CancellationToken.None);

        Assert.Equal("VoiceInk.Windows.Transcription.OpenAICompatible.Groq.ApiKey", secrets.LastReadName);
        Assert.Equal(["VoiceInk.Windows.Transcription.OpenAICompatible.Groq.ApiKey"], secrets.ReadNames);
        Assert.Equal("gsk-test-secret", handler.Requests[0].Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task TranscribeAsync_CustomProviderFallsBackToLegacySecretName()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"text":"Custom text"}"""));
        var secrets = new FakeSecretStore
        {
            Secrets =
            {
                ["VoiceInk.Windows.Transcription.OpenAICompatible.ApiKey"] = "legacy-custom-secret"
            }
        };
        var service = new OpenAICompatibleCloudTranscriptionService(new HttpClient(handler), secrets);

        await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(providerId: "custom"),
            CancellationToken.None);

        Assert.Equal(
            [
                "VoiceInk.Windows.Transcription.OpenAICompatible.Custom.ApiKey",
                "VoiceInk.Windows.Transcription.OpenAICompatible.ApiKey"
            ],
            secrets.ReadNames);
        Assert.Equal("legacy-custom-secret", handler.Requests[0].Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task TranscribeAsync_MissingApiKeyFailsBeforeHttp()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"text":"unused"}"""));
        var service = new OpenAICompatibleCloudTranscriptionService(new HttpClient(handler), new FakeSecretStore());

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
            _ => JsonResponse(HttpStatusCode.BadRequest, """{"error":{"message":"bad request sk-test-secret"}}"""));
        var service = new OpenAICompatibleCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "sk-test-secret" });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Equal("Cloud transcription provider returned HTTP 400.", ex.Message);
        Assert.DoesNotContain("sk-test-secret", ex.Message);
        Assert.DoesNotContain("bad request", ex.Message);
    }

    [Fact]
    public async Task TranscribeAsync_InvalidJsonReturnsSanitizedError()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"unexpected":"shape"}"""));
        var service = new OpenAICompatibleCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "sk-test-secret" });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Equal("Cloud transcription provider returned an unexpected response.", ex.Message);
    }

    private static AudioCaptureResult Audio(string filePath) =>
        new(filePath, TimeSpan.FromSeconds(2), SampleRate: 16000, ChannelCount: 1);

    private static TranscriptionOptions Options(
        string endpoint = "https://api.example.test/v1/audio/transcriptions",
        string model = "gpt-4o-transcribe",
        string language = "auto",
        string prompt = "",
        string providerId = "custom") =>
        new(
            ModelPath: string.Empty,
            language,
            prompt,
            TranscriptionProviderKind.OpenAICompatible,
            endpoint,
            model,
            providerId);

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json) =>
        new(statusCode)
        {
            Content = new StringContent(json)
        };

    private sealed class TempAudioFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"voiceink-cloud-{Guid.NewGuid():N}.wav");

        public TempAudioFile()
        {
            File.WriteAllText(Path, "sample audio bytes");
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
        public Dictionary<string, string> Secrets { get; } = [];
        public List<string> ReadNames { get; } = [];

        public Task<string?> ReadSecretAsync(string name, CancellationToken cancellationToken)
        {
            LastReadName = name;
            ReadNames.Add(name);
            return Task.FromResult(Secrets.TryGetValue(name, out var value) ? value : Secret);
        }

        public Task SaveSecretAsync(string name, string secret, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task DeleteSecretAsync(string name, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<bool> HasSecretAsync(string name, CancellationToken cancellationToken) =>
            Task.FromResult(!string.IsNullOrWhiteSpace(Secret));
    }
}
