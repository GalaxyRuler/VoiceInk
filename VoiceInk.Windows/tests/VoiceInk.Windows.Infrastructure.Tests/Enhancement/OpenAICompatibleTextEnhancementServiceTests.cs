using System.Net;
using System.Text.Json;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Infrastructure.Enhancement;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Enhancement;

public sealed class OpenAICompatibleTextEnhancementServiceTests
{
    [Fact]
    public async Task EnhanceAsync_SendsChatCompletionsRequestWithBearerToken()
    {
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"choices":[{"message":{"content":"Enhanced text"}}]}"""));
        var secrets = new FakeSecretStore { Secret = "sk-test-secret" };
        var service = new OpenAICompatibleTextEnhancementService(new HttpClient(handler), secrets);

        var result = await service.EnhanceAsync(Request(), CancellationToken.None);

        Assert.Equal("Enhanced text", result.Text);
        Assert.Equal("openai-compatible", result.ProviderName);
        Assert.Equal("test-model", result.ModelName);
        Assert.Single(handler.Requests);
        var request = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://api.example.test/v1/chat/completions", request.RequestUri?.ToString());
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal("sk-test-secret", request.Headers.Authorization?.Parameter);

        var body = JsonDocument.Parse(handler.Bodies[0]).RootElement;
        Assert.Equal("test-model", body.GetProperty("model").GetString());
        Assert.Equal(0.3, body.GetProperty("temperature").GetDouble());
        var messages = body.GetProperty("messages");
        Assert.Equal("system", messages[0].GetProperty("role").GetString());
        Assert.Equal("system prompt", messages[0].GetProperty("content").GetString());
        Assert.Equal("user", messages[1].GetProperty("role").GetString());
        Assert.Equal("user prompt", messages[1].GetProperty("content").GetString());
    }

    [Fact]
    public async Task EnhanceAsync_ReadsProviderSpecificSecretAndReturnsProviderMetadata()
    {
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"choices":[{"message":{"content":"Groq text"}}]}"""));
        var secrets = new FakeSecretStore
        {
            Secrets =
            {
                ["VoiceInk.Windows.Enhancement.OpenAICompatible.Custom.ApiKey"] = "custom-secret",
                ["VoiceInk.Windows.Enhancement.OpenAICompatible.Groq.ApiKey"] = "gsk-test-secret"
            }
        };
        var service = new OpenAICompatibleTextEnhancementService(new HttpClient(handler), secrets);

        var result = await service.EnhanceAsync(Request(providerId: "groq"), CancellationToken.None);

        Assert.Equal("Groq text", result.Text);
        Assert.Equal("groq", result.ProviderName);
        Assert.Equal(["VoiceInk.Windows.Enhancement.OpenAICompatible.Groq.ApiKey"], secrets.ReadNames);
        Assert.Equal("gsk-test-secret", handler.Requests[0].Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task EnhanceAsync_CustomProviderFallsBackToLegacySecretName()
    {
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"choices":[{"message":{"content":"Custom text"}}]}"""));
        var secrets = new FakeSecretStore
        {
            Secrets =
            {
                ["VoiceInk.Windows.Enhancement.OpenAICompatible.ApiKey"] = "legacy-custom-secret"
            }
        };
        var service = new OpenAICompatibleTextEnhancementService(new HttpClient(handler), secrets);

        await service.EnhanceAsync(Request(providerId: "custom"), CancellationToken.None);

        Assert.Equal(
            [
                "VoiceInk.Windows.Enhancement.OpenAICompatible.Custom.ApiKey",
                "VoiceInk.Windows.Enhancement.OpenAICompatible.ApiKey"
            ],
            secrets.ReadNames);
        Assert.Equal("legacy-custom-secret", handler.Requests[0].Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task EnhanceAsync_OllamaProviderDoesNotRequireApiKeyOrBearerToken()
    {
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"choices":[{"message":{"content":"Local text"}}]}"""));
        var service = new OpenAICompatibleTextEnhancementService(new HttpClient(handler), new FakeSecretStore());

        var result = await service.EnhanceAsync(
            Request(
                endpoint: "http://localhost:11434/v1/chat/completions",
                model: "mistral",
                providerId: "ollama"),
            CancellationToken.None);

        Assert.Equal("Local text", result.Text);
        Assert.Equal("ollama", result.ProviderName);
        Assert.Null(handler.Requests[0].Headers.Authorization);
    }

    [Fact]
    public async Task EnhanceAsync_MissingApiKeyFailsBeforeHttp()
    {
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"choices":[{"message":{"content":"unused"}}]}"""));
        var service = new OpenAICompatibleTextEnhancementService(new HttpClient(handler), new FakeSecretStore());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.EnhanceAsync(Request(), CancellationToken.None));

        Assert.Contains("not configured", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task EnhanceAsync_HttpErrorDoesNotLeakApiKey()
    {
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.BadRequest, """{"error":{"message":"bad request sk-test-secret"}}"""));
        var secrets = new FakeSecretStore { Secret = "sk-test-secret" };
        var service = new OpenAICompatibleTextEnhancementService(new HttpClient(handler), secrets);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.EnhanceAsync(Request(maxRetries: 1), CancellationToken.None));

        Assert.Contains("HTTP 400", ex.Message);
        Assert.DoesNotContain("sk-test-secret", ex.Message);
    }

    [Theory]
    [InlineData(
        "http://api.example.test/v1/chat/completions",
        "AI enhancement endpoint must use HTTPS unless it targets localhost.")]
    [InlineData(
        "https://sk-test-secret@api.example.test/v1/chat/completions",
        "AI enhancement endpoint must not contain credentials.")]
    [InlineData(
        "https://api.example.test/v1/chat/completions?token=sk-test-secret",
        "AI enhancement endpoint must not contain API keys or tokens in the query string.")]
    public async Task EnhanceAsync_InsecureOrCredentialBearingEndpointFailsBeforeHttp(
        string endpoint,
        string expectedMessage)
    {
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"choices":[{"message":{"content":"unused"}}]}"""));
        var service = new OpenAICompatibleTextEnhancementService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "sk-test-secret" });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.EnhanceAsync(Request(endpoint: endpoint), CancellationToken.None));

        Assert.Equal(expectedMessage, ex.Message);
        Assert.DoesNotContain("sk-test-secret", ex.Message);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task EnhanceAsync_RetriesTransientHttpFailures()
    {
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.InternalServerError, """{"error":{"message":"try again"}}"""),
            _ => JsonResponse(HttpStatusCode.OK, """{"choices":[{"message":{"content":"Recovered"}}]}"""));
        var secrets = new FakeSecretStore { Secret = "sk-test-secret" };
        var service = new OpenAICompatibleTextEnhancementService(new HttpClient(handler), secrets);

        var result = await service.EnhanceAsync(Request(maxRetries: 3), CancellationToken.None);

        Assert.Equal("Recovered", result.Text);
        Assert.Equal(2, handler.Requests.Count);
    }

    private static TextEnhancementRequest Request(
        int maxRetries = 3,
        string endpoint = "https://api.example.test/v1/chat/completions",
        string model = "test-model",
        string providerId = "custom") =>
        new(
            endpoint,
            model,
            "system prompt",
            "user prompt",
            TimeSpan.FromSeconds(7),
            0.3,
            RetryOnTimeout: true,
            MaxRetries: maxRetries,
            ProviderId: providerId);

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json) =>
        new(statusCode)
        {
            Content = new StringContent(json)
        };

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
        public Dictionary<string, string> Secrets { get; } = [];
        public List<string> ReadNames { get; } = [];

        public Task<string?> ReadSecretAsync(string name, CancellationToken cancellationToken)
        {
            ReadNames.Add(name);
            return Task.FromResult(Secrets.TryGetValue(name, out var secret) ? secret : Secret);
        }

        public Task SaveSecretAsync(string name, string secret, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task DeleteSecretAsync(string name, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<bool> HasSecretAsync(string name, CancellationToken cancellationToken) =>
            Task.FromResult(Secrets.ContainsKey(name) || !string.IsNullOrWhiteSpace(Secret));
    }
}
