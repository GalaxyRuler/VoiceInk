using System.Net;
using System.Text.Json;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class SpeechmaticsCloudTranscriptionServiceTests
{
    [Fact]
    public async Task TranscribeAsync_CreatesPollsFetchesTextTranscriptAndCleansUp()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.Created, """{"id":"job-123"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"job":{"status":"running"}}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"job":{"status":"done"}}"""),
            _ => TextResponse(HttpStatusCode.OK, "Hello Speechmatics"),
            _ => JsonResponse(HttpStatusCode.OK, "{}"));
        var secrets = new FakeSecretStore { Secret = "speechmatics-test-secret" };
        var service = new SpeechmaticsCloudTranscriptionService(
            new HttpClient(handler),
            secrets,
            pollDelay: TimeSpan.Zero);

        var result = await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(language: "en"),
            CancellationToken.None);

        Assert.Equal("Hello Speechmatics", result.Text);
        Assert.Equal("speechmatics", result.ProviderName);
        Assert.Equal("VoiceInk.Windows.Transcription.OpenAICompatible.Speechmatics.ApiKey", secrets.LastReadName);
        Assert.Equal(5, handler.Requests.Count);
        Assert.All(handler.Requests, request =>
        {
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("speechmatics-test-secret", request.Headers.Authorization?.Parameter);
        });

        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
        Assert.Equal("https://eu1.asr.api.speechmatics.com/v2/jobs", handler.Requests[0].RequestUri?.ToString());
        Assert.Contains("sample audio bytes", handler.Bodies[0]);
        Assert.Contains("data_file", handler.Bodies[0]);
        Assert.Contains("config", handler.Bodies[0]);
        using var config = JsonDocument.Parse(ExtractMultipartJson(handler.Bodies[0], "config"));
        var transcriptionConfig = config.RootElement.GetProperty("transcription_config");
        Assert.Equal("transcription", config.RootElement.GetProperty("type").GetString());
        Assert.Equal("en", transcriptionConfig.GetProperty("language").GetString());
        Assert.Equal("enhanced", transcriptionConfig.GetProperty("operating_point").GetString());

        Assert.Equal("https://eu1.asr.api.speechmatics.com/v2/jobs/job-123", handler.Requests[1].RequestUri?.ToString());
        Assert.Equal("https://eu1.asr.api.speechmatics.com/v2/jobs/job-123", handler.Requests[2].RequestUri?.ToString());
        Assert.Equal("https://eu1.asr.api.speechmatics.com/v2/jobs/job-123/transcript", handler.Requests[3].RequestUri?.ToString());
        Assert.Contains("text/plain", handler.Requests[3].Headers.Accept.ToString());
        Assert.Equal(HttpMethod.Delete, handler.Requests[4].Method);
        Assert.Equal("https://eu1.asr.api.speechmatics.com/v2/jobs/job-123", handler.Requests[4].RequestUri?.ToString());
    }

    [Fact]
    public async Task TranscribeAsync_SendsAutomaticLanguage()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.Created, """{"id":"job-123"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"job":{"status":"done"}}"""),
            _ => TextResponse(HttpStatusCode.OK, "Auto"),
            _ => JsonResponse(HttpStatusCode.OK, "{}"));
        var service = new SpeechmaticsCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "speechmatics-test-secret" },
            pollDelay: TimeSpan.Zero);

        await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(language: "auto"),
            CancellationToken.None);

        using var config = JsonDocument.Parse(ExtractMultipartJson(handler.Bodies[0], "config"));
        Assert.Equal(
            "auto",
            config.RootElement
                .GetProperty("transcription_config")
                .GetProperty("language")
                .GetString());
    }

    [Fact]
    public async Task TranscribeAsync_MissingApiKeyFailsBeforeHttp()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, "{}"));
        var service = new SpeechmaticsCloudTranscriptionService(
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
            _ => JsonResponse(HttpStatusCode.BadRequest, """{"detail":"speechmatics-test-secret bad request"}"""));
        var service = new SpeechmaticsCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "speechmatics-test-secret" },
            pollDelay: TimeSpan.Zero);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Equal("Speechmatics transcription provider returned HTTP 400.", ex.Message);
        Assert.DoesNotContain("speechmatics-test-secret", ex.Message);
        Assert.DoesNotContain("bad request", ex.Message);
    }

    [Fact]
    public async Task TranscribeAsync_RejectedStatusReturnsSanitizedErrorAndCleansUp()
    {
        using var audioFile = new TempAudioFile();
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.Created, """{"id":"job-123"}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"job":{"status":"rejected","errors":[{"message":"speechmatics-test-secret failed"}]}}"""),
            _ => JsonResponse(HttpStatusCode.OK, "{}"));
        var service = new SpeechmaticsCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "speechmatics-test-secret" },
            pollDelay: TimeSpan.Zero);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Equal("Speechmatics transcription provider failed.", ex.Message);
        Assert.DoesNotContain("speechmatics-test-secret", ex.Message);
        Assert.Equal(HttpMethod.Delete, handler.Requests[^1].Method);
    }

    private static string ExtractMultipartJson(string body, string name)
    {
        var markerIndex = body.IndexOf($"name=\"{name}\"", StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            markerIndex = body.IndexOf($"name={name}", StringComparison.Ordinal);
        }

        Assert.True(markerIndex >= 0, $"Multipart body should contain {name} part.");
        var valueStart = body.IndexOf("\r\n\r\n", markerIndex, StringComparison.Ordinal);
        Assert.True(valueStart >= 0, $"Multipart body should contain {name} content.");
        valueStart += 4;
        var valueEnd = body.IndexOf("\r\n--", valueStart, StringComparison.Ordinal);
        Assert.True(valueEnd >= 0, $"Multipart body should terminate {name} content.");
        return body[valueStart..valueEnd];
    }

    private static AudioCaptureResult Audio(string filePath) =>
        new(filePath, TimeSpan.FromSeconds(2), SampleRate: 16000, ChannelCount: 1);

    private static TranscriptionOptions Options(string language = "auto") =>
        new(
            ModelPath: string.Empty,
            language,
            Prompt: string.Empty,
            TranscriptionProviderKind.OpenAICompatible,
            "https://eu1.asr.api.speechmatics.com/v2/jobs",
            "speechmatics-enhanced",
            CloudProviderId: "speechmatics");

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json) =>
        new(statusCode)
        {
            Content = new StringContent(json)
        };

    private static HttpResponseMessage TextResponse(HttpStatusCode statusCode, string text) =>
        new(statusCode)
        {
            Content = new StringContent(text)
        };

    private sealed class TempAudioFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"voiceink-speechmatics-{Guid.NewGuid():N}.wav");

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
