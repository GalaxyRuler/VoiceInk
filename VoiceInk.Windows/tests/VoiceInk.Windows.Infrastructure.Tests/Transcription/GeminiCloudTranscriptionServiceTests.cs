using System.Net;
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

    private static TranscriptionOptions Options(string language = "auto") =>
        new(
            ModelPath: string.Empty,
            language,
            Prompt: string.Empty,
            TranscriptionProviderKind.OpenAICompatible,
            "https://generativelanguage.googleapis.com/v1beta/models",
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

    private sealed class RecordingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> response)
        : HttpMessageHandler
    {
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
