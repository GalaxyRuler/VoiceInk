using System.Net;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class CloudTranscriptionProviderProbeServiceTests
{
    [Fact]
    public async Task ProbeAsync_DeepgramUsesAuthTokenEndpointAndTokenHeader()
    {
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var service = new CloudTranscriptionProviderProbeService(new HttpClient(handler), new FakeSecretStore { Secret = "deepgram-secret" });

        var result = await service.ProbeAsync(Settings("deepgram"), CancellationToken.None);

        Assert.True(result.IsSuccessful);
        Assert.Equal("https://api.deepgram.com/v1/auth/token", handler.LastRequest?.RequestUri?.ToString());
        Assert.Equal("Token", handler.LastRequest?.Headers.Authorization?.Scheme);
        Assert.Equal("deepgram-secret", handler.LastRequest?.Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task ProbeAsync_GroqUsesOpenAiCompatibleModelsEndpointAndBearerHeader()
    {
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var service = new CloudTranscriptionProviderProbeService(new HttpClient(handler), new FakeSecretStore { Secret = "groq-secret" });

        var result = await service.ProbeAsync(Settings("groq"), CancellationToken.None);

        Assert.True(result.IsSuccessful);
        Assert.Equal("https://api.groq.com/openai/v1/models", handler.LastRequest?.RequestUri?.ToString());
        Assert.Equal("Bearer", handler.LastRequest?.Headers.Authorization?.Scheme);
        Assert.Equal("groq-secret", handler.LastRequest?.Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task ProbeAsync_CustomProviderDerivesModelsEndpointFromConfiguredTranscriptionEndpoint()
    {
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var service = new CloudTranscriptionProviderProbeService(new HttpClient(handler), new FakeSecretStore { Secret = "custom-secret" });

        var result = await service.ProbeAsync(
            Settings(
                "custom",
                endpoint: "https://api.example.test/v1/audio/transcriptions"),
            CancellationToken.None);

        Assert.True(result.IsSuccessful);
        Assert.Equal("https://api.example.test/v1/models", handler.LastRequest?.RequestUri?.ToString());
        Assert.Equal("Bearer", handler.LastRequest?.Headers.Authorization?.Scheme);
        Assert.Equal("custom-secret", handler.LastRequest?.Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task ProbeAsync_MissingKeyDoesNotSendHttp()
    {
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var service = new CloudTranscriptionProviderProbeService(new HttpClient(handler), new FakeSecretStore());

        var result = await service.ProbeAsync(Settings("deepgram"), CancellationToken.None);

        Assert.False(result.IsSuccessful);
        Assert.Contains("No Deepgram API key", result.Message);
        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public async Task ProbeAsync_HttpErrorDoesNotLeakBodyOrSecret()
    {
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("bad custom-secret")
        });
        var service = new CloudTranscriptionProviderProbeService(new HttpClient(handler), new FakeSecretStore { Secret = "custom-secret" });

        var result = await service.ProbeAsync(
            Settings(
                "custom",
                endpoint: "https://api.example.test/v1/audio/transcriptions"),
            CancellationToken.None);

        Assert.False(result.IsSuccessful);
        Assert.Contains("rejected", result.Message);
        Assert.DoesNotContain("custom-secret", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("bad", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProbeAsync_UnsupportedProviderReturnsClearMessageWithoutHttp()
    {
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var service = new CloudTranscriptionProviderProbeService(new HttpClient(handler), new FakeSecretStore { Secret = "soniox-secret" });

        var result = await service.ProbeAsync(Settings("soniox"), CancellationToken.None);

        Assert.False(result.IsSuccessful);
        Assert.Contains("not available", result.Message);
        Assert.Null(handler.LastRequest);
    }

    private static AppSettings Settings(string providerId, string endpoint = "") =>
        new()
        {
            TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
            CloudTranscriptionProviderId = providerId,
            CloudTranscriptionEndpoint = string.IsNullOrWhiteSpace(endpoint)
                ? TranscriptionProviderPresetCatalog.Resolve(providerId).Endpoint
                : endpoint,
            CloudTranscriptionModel = TranscriptionProviderPresetCatalog.Resolve(providerId).DefaultModel
        };

    private sealed class FakeSecretStore : ISecretStore
    {
        public string? Secret { get; init; }

        public Task<string?> ReadSecretAsync(string name, CancellationToken cancellationToken) =>
            Task.FromResult(Secret);

        public Task SaveSecretAsync(string name, string secret, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task DeleteSecretAsync(string name, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<bool> HasSecretAsync(string name, CancellationToken cancellationToken) =>
            Task.FromResult(!string.IsNullOrWhiteSpace(Secret));
    }

    private sealed class RecordingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            await Task.Yield();
            return response(request);
        }
    }
}
