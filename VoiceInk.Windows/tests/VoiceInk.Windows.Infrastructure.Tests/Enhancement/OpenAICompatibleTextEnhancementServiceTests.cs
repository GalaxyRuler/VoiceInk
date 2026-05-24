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

    private static TextEnhancementRequest Request(int maxRetries = 3) =>
        new(
            "https://api.example.test/v1/chat/completions",
            "test-model",
            "system prompt",
            "user prompt",
            TimeSpan.FromSeconds(7),
            0.3,
            RetryOnTimeout: true,
            MaxRetries: maxRetries);

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

        public Task<string?> ReadSecretAsync(string name, CancellationToken cancellationToken) =>
            Task.FromResult(Secret);

        public Task SaveSecretAsync(string name, string secret, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task DeleteSecretAsync(string name, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<bool> HasSecretAsync(string name, CancellationToken cancellationToken) =>
            Task.FromResult(!string.IsNullOrWhiteSpace(Secret));
    }
}
