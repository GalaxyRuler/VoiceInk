using System.Net;
using System.Text.Json;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class XaiCloudTranscriptionServiceTests
{
    [Fact]
    public async Task TranscribeAsync_SendsMultipartRequestWithBearerToken()
    {
        using var audioFile = new TempAudioFile();
        var handler = new RecordingHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, """{"text":"Hello Grok"}"""));
        var secrets = new FakeSecretStore { Secret = "xai-test-secret" };
        var service = new XaiCloudTranscriptionService(new HttpClient(handler), secrets);

        var result = await service.TranscribeAsync(Audio(audioFile.Path), Options(language: "en"), CancellationToken.None);

        Assert.Equal("Hello Grok", result.Text);
        Assert.Equal("xai", result.ProviderName);
        Assert.Equal("VoiceInk.Windows.Transcription.OpenAICompatible.xAI.ApiKey", secrets.LastReadName);
        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://api.x.ai/v1/stt", request.RequestUri?.ToString());
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal("xai-test-secret", request.Headers.Authorization?.Parameter);
        Assert.Contains("sample audio bytes", handler.Bodies[0]);
        Assert.Contains("name=model", handler.Bodies[0]);
        Assert.Contains("grok-stt", handler.Bodies[0]);
        Assert.Contains("name=format", handler.Bodies[0]);
        Assert.Contains("true", handler.Bodies[0]);
        Assert.Contains("name=language", handler.Bodies[0]);
        Assert.Contains("en", handler.Bodies[0]);
    }

    [Fact]
    public async Task TranscribeAsync_OmitsAutomaticLanguage()
    {
        using var audioFile = new TempAudioFile();
        var handler = new RecordingHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, """{"text":"Auto"}"""));
        var service = new XaiCloudTranscriptionService(new HttpClient(handler), new FakeSecretStore { Secret = "xai-test-secret" });

        await service.TranscribeAsync(Audio(audioFile.Path), Options(language: "auto"), CancellationToken.None);

        Assert.DoesNotContain("name=language", handler.Bodies[0]);
    }

    [Fact]
    public async Task TranscribeAsync_MissingApiKeyFailsBeforeHttp()
    {
        using var audioFile = new TempAudioFile();
        var handler = new RecordingHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, "{}"));
        var service = new XaiCloudTranscriptionService(new HttpClient(handler), new FakeSecretStore());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Contains("not configured", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task TranscribeAsync_HttpErrorDoesNotLeakResponseBodyOrApiKey()
    {
        using var audioFile = new TempAudioFile();
        var handler = new RecordingHttpMessageHandler(_ => JsonResponse(HttpStatusCode.BadRequest, """{"error":"xai-test-secret bad"}"""));
        var service = new XaiCloudTranscriptionService(new HttpClient(handler), new FakeSecretStore { Secret = "xai-test-secret" });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TranscribeAsync(Audio(audioFile.Path), Options(), CancellationToken.None));

        Assert.Equal("xAI transcription provider returned HTTP 400.", ex.Message);
        Assert.DoesNotContain("xai-test-secret", ex.Message);
    }

    private static AudioCaptureResult Audio(string filePath) =>
        new(filePath, TimeSpan.FromSeconds(2), SampleRate: 16000, ChannelCount: 1);

    private static TranscriptionOptions Options(string language = "auto") =>
        new(string.Empty, language, string.Empty, TranscriptionProviderKind.OpenAICompatible, "https://api.x.ai/v1/stt", "grok-stt", "xai");

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json) =>
        new(statusCode) { Content = new StringContent(json) };

    private sealed class TempAudioFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"voiceink-xai-{Guid.NewGuid():N}.wav");
        public byte[] Bytes { get; } = "sample audio bytes"u8.ToArray();
        public TempAudioFile() => File.WriteAllBytes(Path, Bytes);
        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }

    private sealed class RecordingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];
        public List<string> Bodies { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            Bodies.Add(request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken));
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
        public Task SaveSecretAsync(string name, string secret, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task DeleteSecretAsync(string name, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> HasSecretAsync(string name, CancellationToken cancellationToken) => Task.FromResult(!string.IsNullOrWhiteSpace(Secret));
    }
}
