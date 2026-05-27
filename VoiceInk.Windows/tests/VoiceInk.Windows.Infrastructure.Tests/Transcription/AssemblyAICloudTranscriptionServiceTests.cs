using System.Net;
using System.Text.Json;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class AssemblyAICloudTranscriptionServiceTests
{
    [Fact]
    public async Task TranscribeAsync_UploadsAudioSubmitsTranscriptAndPollsUntilComplete()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"upload_url":"https://cdn.assemblyai.test/upload.wav"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"id":"transcript-123","status":"queued"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"id":"transcript-123","status":"processing"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"id":"transcript-123","status":"completed","text":"Hello from AssemblyAI"}"""));
        var secrets = new FakeSecretStore { Secret = "aai-test-secret" };
        var service = new AssemblyAICloudTranscriptionService(
            new HttpClient(handler),
            secrets,
            pollDelay: TimeSpan.Zero);

        var result = await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(language: "en"),
            CancellationToken.None);

        Assert.Equal("Hello from AssemblyAI", result.Text);
        Assert.Equal("assemblyai", result.ProviderName);
        Assert.Equal("VoiceInk.Windows.Transcription.OpenAICompatible.AssemblyAI.ApiKey", secrets.LastReadName);
        Assert.Equal(4, handler.Requests.Count);
        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
        Assert.Equal("https://api.assemblyai.com/v2/upload", handler.Requests[0].RequestUri?.ToString());
        Assert.Equal("aai-test-secret", handler.Requests[0].Headers.Authorization?.Scheme);
        Assert.Equal(audioFile.Bytes, handler.Bodies[0]);

        Assert.Equal(HttpMethod.Post, handler.Requests[1].Method);
        Assert.Equal("https://api.assemblyai.com/v2/transcript", handler.Requests[1].RequestUri?.ToString());
        using var submitJson = JsonDocument.Parse(handler.Bodies[1]);
        Assert.Equal("https://cdn.assemblyai.test/upload.wav", submitJson.RootElement.GetProperty("audio_url").GetString());
        Assert.Equal("universal-3-pro", submitJson.RootElement.GetProperty("speech_model").GetString());
        Assert.Equal("en", submitJson.RootElement.GetProperty("language_code").GetString());

        Assert.Equal(HttpMethod.Get, handler.Requests[2].Method);
        Assert.Equal("https://api.assemblyai.com/v2/transcript/transcript-123", handler.Requests[2].RequestUri?.ToString());
        Assert.Equal(HttpMethod.Get, handler.Requests[3].Method);
    }

    [Fact]
    public async Task TranscribeAsync_OmitsAutomaticLanguage()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"upload_url":"https://cdn.assemblyai.test/upload.wav"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"id":"transcript-123","status":"completed","text":"Auto language"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"id":"transcript-123","status":"completed","text":"Auto language"}"""));
        var service = new AssemblyAICloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "aai-test-secret" },
            pollDelay: TimeSpan.Zero);

        await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(language: "auto"),
            CancellationToken.None);

        using var submitJson = JsonDocument.Parse(handler.Bodies[1]);
        Assert.False(submitJson.RootElement.TryGetProperty("language_code", out _));
    }

    [Fact]
    public async Task TranscribeAsync_MapsEndpointQueryOptionsToTranscriptPayload()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"upload_url":"https://cdn.assemblyai.test/upload.wav"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"id":"transcript-123","status":"completed","text":"Diarized"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"id":"transcript-123","status":"completed","text":"Diarized"}"""));
        var service = new AssemblyAICloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "aai-test-secret" },
            pollDelay: TimeSpan.Zero);

        await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(endpoint: "https://streaming.assemblyai.com/v3/ws?speaker_labels=true&format_text=false&speakers_expected=2"),
            CancellationToken.None);

        using var submitJson = JsonDocument.Parse(handler.Bodies[1]);
        Assert.True(submitJson.RootElement.GetProperty("speaker_labels").GetBoolean());
        Assert.False(submitJson.RootElement.GetProperty("format_text").GetBoolean());
        Assert.Equal(2, submitJson.RootElement.GetProperty("speakers_expected").GetInt32());
    }

    [Fact]
    public async Task TranscribeAsync_MissingApiKeyFailsBeforeHttp()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"upload_url":"unused"}"""));
        var service = new AssemblyAICloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore(),
            pollDelay: TimeSpan.Zero);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Contains("not configured", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task TranscribeAsync_SecretBearingEndpointQueryFailsBeforeHttp()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"upload_url":"unused"}"""));
        var service = new AssemblyAICloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "aai-test-secret" },
            pollDelay: TimeSpan.Zero);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(
                Audio(audioFile.Path),
                Options(endpoint: "https://streaming.assemblyai.com/v3/ws?api_key=sk-query-secret"),
                CancellationToken.None));

        Assert.Equal(TranscriptionConfiguration.CloudEndpointQuerySecretRejectedMessage, ex.Message);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task TranscribeAsync_HttpErrorDoesNotLeakResponseBodyOrApiKey()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.BadRequest, """{"error":"aai-test-secret bad request"}"""));
        var service = new AssemblyAICloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "aai-test-secret" },
            pollDelay: TimeSpan.Zero);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Equal("AssemblyAI transcription provider returned HTTP 400.", ex.Message);
        Assert.DoesNotContain("aai-test-secret", ex.Message);
        Assert.DoesNotContain("bad request", ex.Message);
    }

    [Fact]
    public async Task TranscribeAsync_ErrorStatusReturnsSanitizedError()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"upload_url":"https://cdn.assemblyai.test/upload.wav"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"id":"transcript-123","status":"error"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"id":"transcript-123","status":"error","error":"aai-test-secret failed"}"""));
        var service = new AssemblyAICloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "aai-test-secret" },
            pollDelay: TimeSpan.Zero);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Equal("AssemblyAI transcription provider failed.", ex.Message);
        Assert.DoesNotContain("aai-test-secret", ex.Message);
    }

    private static AudioCaptureResult Audio(string filePath) =>
        new(filePath, TimeSpan.FromSeconds(2), SampleRate: 16000, ChannelCount: 1);

    private static TranscriptionOptions Options(
        string model = "universal-3-pro",
        string language = "auto",
        string endpoint = "https://streaming.assemblyai.com/v3/ws") =>
        new(
            ModelPath: string.Empty,
            language,
            Prompt: string.Empty,
            TranscriptionProviderKind.OpenAICompatible,
            endpoint,
            model,
            CloudProviderId: "assemblyai");

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json) =>
        new(statusCode)
        {
            Content = new StringContent(json)
        };

    private sealed class TempAudioFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"voiceink-assemblyai-{Guid.NewGuid():N}.wav");

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
