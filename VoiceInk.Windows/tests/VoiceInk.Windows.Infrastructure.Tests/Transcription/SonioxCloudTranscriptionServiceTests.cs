using System.Net;
using System.Text.Json;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class SonioxCloudTranscriptionServiceTests
{
    [Fact]
    public async Task TranscribeAsync_UploadsCreatesPollsFetchesTranscriptAndCleansUp()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"id":"file-123"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"id":"transcription-123"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"status":"processing"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"status":"completed"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"tokens":[{"text":"Hello"},{"text":" "},{"text":"Soniox"}]}"""),
            _ => JsonResponse(HttpStatusCode.OK, "{}"),
            _ => JsonResponse(HttpStatusCode.OK, "{}"));
        var secrets = new FakeSecretStore { Secret = "soniox-test-secret" };
        var service = new SonioxCloudTranscriptionService(
            new HttpClient(handler),
            secrets,
            pollDelay: TimeSpan.Zero);

        var result = await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(language: "en"),
            CancellationToken.None);

        Assert.Equal("Hello Soniox", result.Text);
        Assert.Equal("soniox", result.ProviderName);
        Assert.Equal("VoiceInk.Windows.Transcription.OpenAICompatible.Soniox.ApiKey", secrets.LastReadName);
        Assert.Equal(7, handler.Requests.Count);
        Assert.All(handler.Requests, request =>
        {
            if (request.RequestUri?.Host == "api.soniox.com")
            {
                Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
                Assert.Equal("soniox-test-secret", request.Headers.Authorization?.Parameter);
            }
        });

        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
        Assert.Equal("https://api.soniox.com/v1/files", handler.Requests[0].RequestUri?.ToString());
        Assert.Contains("sample audio bytes", handler.Bodies[0]);

        Assert.Equal(HttpMethod.Post, handler.Requests[1].Method);
        Assert.Equal("https://api.soniox.com/v1/transcriptions", handler.Requests[1].RequestUri?.ToString());
        using var createJson = JsonDocument.Parse(handler.Bodies[1]);
        Assert.Equal("stt-async-v4", createJson.RootElement.GetProperty("model").GetString());
        Assert.Equal("file-123", createJson.RootElement.GetProperty("file_id").GetString());
        Assert.Equal("en", createJson.RootElement.GetProperty("language_hints")[0].GetString());

        Assert.Equal("https://api.soniox.com/v1/transcriptions/transcription-123", handler.Requests[2].RequestUri?.ToString());
        Assert.Equal("https://api.soniox.com/v1/transcriptions/transcription-123", handler.Requests[3].RequestUri?.ToString());
        Assert.Equal("https://api.soniox.com/v1/transcriptions/transcription-123/transcript", handler.Requests[4].RequestUri?.ToString());
        Assert.Equal(HttpMethod.Delete, handler.Requests[5].Method);
        Assert.Equal("https://api.soniox.com/v1/transcriptions/transcription-123", handler.Requests[5].RequestUri?.ToString());
        Assert.Equal(HttpMethod.Delete, handler.Requests[6].Method);
        Assert.Equal("https://api.soniox.com/v1/files/file-123", handler.Requests[6].RequestUri?.ToString());
    }

    [Fact]
    public async Task TranscribeAsync_OmitsAutomaticLanguageHints()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"id":"file-123"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"id":"transcription-123"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"status":"completed"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"tokens":[{"text":"Auto"}]}"""),
            _ => JsonResponse(HttpStatusCode.OK, "{}"),
            _ => JsonResponse(HttpStatusCode.OK, "{}"));
        var service = new SonioxCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "soniox-test-secret" },
            pollDelay: TimeSpan.Zero);

        await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(language: "auto"),
            CancellationToken.None);

        using var createJson = JsonDocument.Parse(handler.Bodies[1]);
        Assert.False(createJson.RootElement.TryGetProperty("language_hints", out _));
    }

    [Fact]
    public async Task TranscribeAsync_MissingApiKeyFailsBeforeHttp()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, "{}"));
        var service = new SonioxCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore(),
            pollDelay: TimeSpan.Zero);

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
            _ => JsonResponse(HttpStatusCode.BadRequest, """{"message":"soniox-test-secret bad request"}"""));
        var service = new SonioxCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "soniox-test-secret" },
            pollDelay: TimeSpan.Zero);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Equal("Soniox transcription provider returned HTTP 400.", ex.Message);
        Assert.DoesNotContain("soniox-test-secret", ex.Message);
        Assert.DoesNotContain("bad request", ex.Message);
    }

    [Fact]
    public async Task TranscribeAsync_ErrorStatusReturnsSanitizedErrorAndCleansUp()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"id":"file-123"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"id":"transcription-123"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"status":"error","error_message":"soniox-test-secret failed"}"""),
            _ => JsonResponse(HttpStatusCode.OK, "{}"),
            _ => JsonResponse(HttpStatusCode.OK, "{}"));
        var service = new SonioxCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "soniox-test-secret" },
            pollDelay: TimeSpan.Zero);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Equal("Soniox transcription provider failed.", ex.Message);
        Assert.DoesNotContain("soniox-test-secret", ex.Message);
        Assert.Equal(HttpMethod.Delete, handler.Requests[^2].Method);
        Assert.Equal(HttpMethod.Delete, handler.Requests[^1].Method);
    }

    private static AudioCaptureResult Audio(string filePath) =>
        new(filePath, TimeSpan.FromSeconds(2), SampleRate: 16000, ChannelCount: 1);

    private static TranscriptionOptions Options(string language = "auto") =>
        new(
            ModelPath: string.Empty,
            language,
            Prompt: string.Empty,
            TranscriptionProviderKind.OpenAICompatible,
            "https://api.soniox.com/v1/transcriptions",
            "stt-async-v4",
            CloudProviderId: "soniox");

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json) =>
        new(statusCode)
        {
            Content = new StringContent(json)
        };

    private sealed class TempAudioFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"voiceink-soniox-{Guid.NewGuid():N}.wav");

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
