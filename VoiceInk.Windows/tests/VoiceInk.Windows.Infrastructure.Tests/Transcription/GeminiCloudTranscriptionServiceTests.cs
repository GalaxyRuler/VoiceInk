using System.Net;
using System.Text;
using System.Text.Json;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class GeminiCloudTranscriptionServiceTests
{
    private const long InlineAudioLimitBytes = 20 * 1024 * 1024;

    [Fact]
    public async Task TranscribeAsync_SendsGenerateContentRequestWithInlineAudio()
    {
        using var audioFile = new TempAudioFile();
        var handler = new RecordingHttpMessageHandler(_ => JsonResponse(
            HttpStatusCode.OK,
            """{"candidates":[{"content":{"parts":[{"text":"Hello Gemini"}]}}]}"""));
        var secrets = new FakeSecretStore { Secret = "gemini-test-secret" };
        var service = new GeminiCloudTranscriptionService(new HttpClient(handler), secrets);

        var result = await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(),
            CancellationToken.None);

        Assert.Equal("Hello Gemini", result.Text);
        Assert.Equal("gemini", result.ProviderName);
        Assert.Equal("VoiceInk.Windows.Transcription.OpenAICompatible.Gemini.ApiKey", secrets.LastReadName);
        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal(
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent",
            request.RequestUri?.ToString());
        Assert.Equal("gemini-test-secret", request.Headers.GetValues("x-goog-api-key").Single());
        Assert.Null(request.Headers.Authorization);

        using var payload = JsonDocument.Parse(handler.Bodies[0]);
        var parts = payload.RootElement.GetProperty("contents")[0].GetProperty("parts");
        Assert.Contains(
            "Return only the transcript",
            parts[0].GetProperty("text").GetString(),
            StringComparison.Ordinal);
        var inlineData = parts[1].GetProperty("inline_data");
        Assert.Equal("audio/wav", inlineData.GetProperty("mime_type").GetString());
        Assert.Equal(Convert.ToBase64String(audioFile.Bytes), inlineData.GetProperty("data").GetString());
    }

    [Fact]
    public async Task TranscribeAsync_IncludesSpecificLanguageInstruction()
    {
        using var audioFile = new TempAudioFile();
        var handler = new RecordingHttpMessageHandler(_ => JsonResponse(
            HttpStatusCode.OK,
            """{"candidates":[{"content":{"parts":[{"text":"Bonjour"}]}}]}"""));
        var service = new GeminiCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "gemini-test-secret" });

        await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(language: "fr"),
            CancellationToken.None);

        using var payload = JsonDocument.Parse(handler.Bodies[0]);
        var prompt = payload.RootElement.GetProperty("contents")[0].GetProperty("parts")[0].GetProperty("text").GetString();
        Assert.Contains("language code fr", prompt, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TranscribeAsync_LargeAudioUsesFilesApiAndGenerateContentFileData()
    {
        using var audioFile = TempAudioFile.Large(InlineAudioLimitBytes + 1);
        var handler = new RecordingHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsoluteUri == "https://generativelanguage.googleapis.com/upload/v1beta/files")
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Headers.TryAddWithoutValidation("X-Goog-Upload-URL", "https://upload.example.test/gemini/upload-session");
                return response;
            }

            if (request.RequestUri?.AbsoluteUri == "https://upload.example.test/gemini/upload-session")
            {
                return JsonResponse(
                    HttpStatusCode.OK,
                    """{"file":{"uri":"files/gemini-audio-123","mimeType":"audio/wav"}}""");
            }

            return JsonResponse(
                HttpStatusCode.OK,
                """{"candidates":[{"content":{"parts":[{"text":"Long Gemini transcript"}]}}]}""");
        });
        var service = new GeminiCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "gemini-test-secret" });

        var result = await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(),
            CancellationToken.None);

        Assert.Equal("Long Gemini transcript", result.Text);
        Assert.Equal(3, handler.Requests.Count);

        var startUpload = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, startUpload.Method);
        Assert.Equal("https://generativelanguage.googleapis.com/upload/v1beta/files", startUpload.RequestUri?.ToString());
        Assert.Equal("gemini-test-secret", startUpload.Headers.GetValues("x-goog-api-key").Single());
        Assert.Equal("resumable", startUpload.Headers.GetValues("X-Goog-Upload-Protocol").Single());
        Assert.Equal("start", startUpload.Headers.GetValues("X-Goog-Upload-Command").Single());
        Assert.Equal(audioFile.Length.ToString(), startUpload.Headers.GetValues("X-Goog-Upload-Header-Content-Length").Single());
        Assert.Equal("audio/wav", startUpload.Headers.GetValues("X-Goog-Upload-Header-Content-Type").Single());
        using (var startPayload = JsonDocument.Parse(handler.Bodies[0]))
        {
            Assert.Equal(System.IO.Path.GetFileName(audioFile.Path), startPayload.RootElement.GetProperty("file").GetProperty("display_name").GetString());
        }

        var upload = handler.Requests[1];
        Assert.Equal(HttpMethod.Post, upload.Method);
        Assert.Equal("https://upload.example.test/gemini/upload-session", upload.RequestUri?.ToString());
        Assert.Equal("0", upload.Headers.GetValues("X-Goog-Upload-Offset").Single());
        Assert.Equal("upload, finalize", upload.Headers.GetValues("X-Goog-Upload-Command").Single());
        Assert.Equal(audioFile.Length, handler.BodyByteCounts[1]);

        var generate = handler.Requests[2];
        Assert.Equal(
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent",
            generate.RequestUri?.ToString());
        using var payload = JsonDocument.Parse(handler.Bodies[2]);
        var parts = payload.RootElement.GetProperty("contents")[0].GetProperty("parts");
        Assert.False(parts[1].TryGetProperty("inline_data", out _));
        var fileData = parts[1].GetProperty("file_data");
        Assert.Equal("audio/wav", fileData.GetProperty("mime_type").GetString());
        Assert.Equal("files/gemini-audio-123", fileData.GetProperty("file_uri").GetString());
    }

    [Fact]
    public async Task TranscribeAsync_MapsSafeEndpointQueryOptionsToGenerationConfig()
    {
        using var audioFile = new TempAudioFile();
        var handler = new RecordingHttpMessageHandler(_ => JsonResponse(
            HttpStatusCode.OK,
            """{"candidates":[{"content":{"parts":[{"text":"Tuned Gemini"}]}}]}"""));
        var service = new GeminiCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "gemini-test-secret" });

        await service.TranscribeAsync(
            Audio(audioFile.Path),
            Options(endpoint: "https://generativelanguage.googleapis.com/v1beta/models?temperature=0.2&topK=12&maxOutputTokens=800&model=ignored"),
            CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent",
            request.RequestUri?.ToString());
        using var payload = JsonDocument.Parse(handler.Bodies[0]);
        var config = payload.RootElement.GetProperty("generationConfig");
        Assert.Equal(0.2, config.GetProperty("temperature").GetDouble());
        Assert.Equal(12, config.GetProperty("topK").GetInt32());
        Assert.Equal(800, config.GetProperty("maxOutputTokens").GetInt32());
        Assert.False(config.TryGetProperty("model", out _));
        Assert.DoesNotContain("ignored", handler.Bodies[0]);
    }

    [Fact]
    public async Task TranscribeAsync_RejectsSecretEndpointQueryBeforeHttp()
    {
        using var audioFile = new TempAudioFile();
        var handler = new RecordingHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, "{}"));
        var service = new GeminiCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "gemini-test-secret" });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(
                Audio(audioFile.Path),
                Options(endpoint: "https://generativelanguage.googleapis.com/v1beta/models?token=leaked"),
                CancellationToken.None));

        Assert.Equal(TranscriptionConfiguration.CloudEndpointQuerySecretRejectedMessage, ex.Message);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task TranscribeAsync_FileUploadStartErrorDoesNotLeakResponseBodyOrApiKey()
    {
        using var audioFile = TempAudioFile.Large(InlineAudioLimitBytes + 1);
        var handler = new RecordingHttpMessageHandler(_ => JsonResponse(
            HttpStatusCode.Unauthorized,
            """{"error":{"message":"gemini-test-secret upload denied"}}"""));
        var service = new GeminiCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "gemini-test-secret" });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Equal("Gemini transcription provider returned HTTP 401.", ex.Message);
        Assert.DoesNotContain("gemini-test-secret", ex.Message);
        Assert.DoesNotContain("upload denied", ex.Message);
        Assert.Single(handler.Requests);
        Assert.Equal("https://generativelanguage.googleapis.com/upload/v1beta/files", handler.Requests[0].RequestUri?.ToString());
    }

    [Fact]
    public async Task TranscribeAsync_MissingApiKeyFailsBeforeHttp()
    {
        using var audioFile = new TempAudioFile();
        var handler = new RecordingHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, "{}"));
        var service = new GeminiCloudTranscriptionService(new HttpClient(handler), new FakeSecretStore());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Contains("not configured", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task TranscribeAsync_HttpErrorDoesNotLeakResponseBodyOrApiKey()
    {
        using var audioFile = new TempAudioFile();
        var handler = new RecordingHttpMessageHandler(_ => JsonResponse(
            HttpStatusCode.BadRequest,
            """{"error":{"message":"gemini-test-secret bad request"}}"""));
        var service = new GeminiCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "gemini-test-secret" });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Equal("Gemini transcription provider returned HTTP 400.", ex.Message);
        Assert.DoesNotContain("gemini-test-secret", ex.Message);
        Assert.DoesNotContain("bad request", ex.Message);
    }

    [Fact]
    public async Task TranscribeAsync_EmptyCandidateTextFails()
    {
        using var audioFile = new TempAudioFile();
        var handler = new RecordingHttpMessageHandler(_ => JsonResponse(
            HttpStatusCode.OK,
            """{"candidates":[{"content":{"parts":[{"text":"   "}]} }]}"""));
        var service = new GeminiCloudTranscriptionService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "gemini-test-secret" });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Equal("Gemini transcription provider returned no text.", ex.Message);
    }

    private static AudioCaptureResult Audio(string filePath) =>
        new(filePath, TimeSpan.FromSeconds(2), SampleRate: 16000, ChannelCount: 1);

    private static TranscriptionOptions Options(
        string language = "auto",
        string endpoint = "https://generativelanguage.googleapis.com/v1beta/models") =>
        new(
            ModelPath: string.Empty,
            language,
            Prompt: string.Empty,
            TranscriptionProviderKind.OpenAICompatible,
            endpoint,
            "gemini-2.5-flash",
            CloudProviderId: "gemini");

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json) =>
        new(statusCode)
        {
            Content = new StringContent(json)
        };

    private sealed class TempAudioFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"voiceink-gemini-{Guid.NewGuid():N}.wav");

        public byte[] Bytes { get; }

        public long Length => new FileInfo(Path).Length;

        public TempAudioFile()
        {
            Bytes = "sample audio bytes"u8.ToArray();
            File.WriteAllBytes(Path, Bytes);
        }

        private TempAudioFile(long length)
        {
            Bytes = [];
            using var stream = File.Create(Path);
            stream.SetLength(length);
        }

        public static TempAudioFile Large(long length) => new(length);

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }

    private sealed class RecordingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> response)
        : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];
        public List<string> Bodies { get; } = [];
        public List<long> BodyByteCounts { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (request.Content is null)
            {
                BodyByteCounts.Add(0);
                Bodies.Add(string.Empty);
            }
            else
            {
                var body = await request.Content.ReadAsByteArrayAsync(cancellationToken);
                BodyByteCounts.Add(body.Length);
                var contentType = request.Content.Headers.ContentType?.MediaType;
                Bodies.Add(string.Equals(contentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase)
                    ? string.Empty
                    : Encoding.UTF8.GetString(body));
            }

            return response(request);
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
