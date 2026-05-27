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

    [Theory]
    [InlineData("openai", "gpt-5.4", "none", null, null)]
    [InlineData("gemini", "gemini-3.1-pro-preview", "low", null, null)]
    [InlineData("cerebras", "gpt-oss-120b", "low", "hidden", null)]
    [InlineData("groq", "openai/gpt-oss-20b", "low", null, false)]
    [InlineData("groq", "qwen/qwen3-32b", "none", null, null)]
    public async Task EnhanceAsync_SendsProviderReasoningParameters(
        string providerId,
        string model,
        string expectedReasoningEffort,
        string? expectedReasoningFormat,
        bool? expectedIncludeReasoning)
    {
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"choices":[{"message":{"content":"Enhanced text"}}]}"""));
        var service = new OpenAICompatibleTextEnhancementService(
            new HttpClient(handler),
            new FakeSecretStore { Secret = "provider-secret" });

        await service.EnhanceAsync(
            Request(
                endpoint: EnhancementProviderPresetCatalog.Resolve(providerId).Endpoint,
                model: model,
                providerId: providerId),
            CancellationToken.None);

        var body = JsonDocument.Parse(handler.Bodies[0]).RootElement;
        Assert.Equal(expectedReasoningEffort, body.GetProperty("reasoning_effort").GetString());
        if (expectedReasoningFormat is not null)
        {
            Assert.Equal(expectedReasoningFormat, body.GetProperty("reasoning_format").GetString());
        }
        else
        {
            Assert.False(body.TryGetProperty("reasoning_format", out _));
        }

        if (expectedIncludeReasoning is not null)
        {
            Assert.Equal(expectedIncludeReasoning, body.GetProperty("include_reasoning").GetBoolean());
        }
        else
        {
            Assert.False(body.TryGetProperty("include_reasoning", out _));
        }
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
    public async Task EnhanceAsync_OllamaProviderUsesNativeChatApiWithoutApiKey()
    {
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"message":{"role":"assistant","content":"Local text"},"done":true}"""));
        var service = new OpenAICompatibleTextEnhancementService(new HttpClient(handler), new FakeSecretStore());

        var result = await service.EnhanceAsync(
            Request(
                endpoint: "http://localhost:11434/api/chat",
                model: "mistral",
                providerId: "ollama"),
            CancellationToken.None);

        Assert.Equal("Local text", result.Text);
        Assert.Equal("ollama", result.ProviderName);
        Assert.Null(handler.Requests[0].Headers.Authorization);
        Assert.Equal("http://localhost:11434/api/chat", handler.Requests[0].RequestUri?.ToString());

        var body = JsonDocument.Parse(handler.Bodies[0]).RootElement;
        Assert.Equal("mistral", body.GetProperty("model").GetString());
        Assert.False(body.GetProperty("stream").GetBoolean());
        var messages = body.GetProperty("messages");
        Assert.Equal("system", messages[0].GetProperty("role").GetString());
        Assert.Equal("system prompt", messages[0].GetProperty("content").GetString());
        Assert.Equal("user", messages[1].GetProperty("role").GetString());
        Assert.Equal("user prompt", messages[1].GetProperty("content").GetString());
    }

    [Fact]
    public async Task EnhanceAsync_AnthropicProviderSendsMessagesRequest()
    {
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.OK, """{"content":[{"type":"text","text":"Claude text"}]}"""));
        var secrets = new FakeSecretStore
        {
            Secrets =
            {
                ["VoiceInk.Windows.Enhancement.OpenAICompatible.Anthropic.ApiKey"] = "sk-ant-test"
            }
        };
        var service = new OpenAICompatibleTextEnhancementService(new HttpClient(handler), secrets);

        var result = await service.EnhanceAsync(
            Request(
                endpoint: "https://api.anthropic.com/v1/messages",
                model: "claude-sonnet-4-6",
                providerId: "anthropic"),
            CancellationToken.None);

        Assert.Equal("Claude text", result.Text);
        Assert.Equal("anthropic", result.ProviderName);
        Assert.Equal("claude-sonnet-4-6", result.ModelName);
        Assert.Equal(["VoiceInk.Windows.Enhancement.OpenAICompatible.Anthropic.ApiKey"], secrets.ReadNames);
        var request = handler.Requests[0];
        Assert.Equal("https://api.anthropic.com/v1/messages", request.RequestUri?.ToString());
        Assert.Null(request.Headers.Authorization);
        Assert.True(request.Headers.TryGetValues("x-api-key", out var apiKeyValues));
        Assert.Equal("sk-ant-test", Assert.Single(apiKeyValues));
        Assert.True(request.Headers.TryGetValues("anthropic-version", out var versionValues));
        Assert.Equal("2023-06-01", Assert.Single(versionValues));

        var body = JsonDocument.Parse(handler.Bodies[0]).RootElement;
        Assert.Equal("claude-sonnet-4-6", body.GetProperty("model").GetString());
        Assert.Equal("system prompt", body.GetProperty("system").GetString());
        Assert.Equal(0.3, body.GetProperty("temperature").GetDouble());
        Assert.Equal(1024, body.GetProperty("max_tokens").GetInt32());
        var messages = body.GetProperty("messages");
        Assert.Equal("user", messages[0].GetProperty("role").GetString());
        Assert.Equal("user prompt", messages[0].GetProperty("content").GetString());
    }

    [Fact]
    public async Task EnhanceAsync_AnthropicHttpErrorDoesNotLeakApiKey()
    {
        var handler = new QueueHttpMessageHandler(
            _ => JsonResponse(HttpStatusCode.BadRequest, """{"error":{"message":"bad sk-ant-test"}}"""));
        var secrets = new FakeSecretStore
        {
            Secrets =
            {
                ["VoiceInk.Windows.Enhancement.OpenAICompatible.Anthropic.ApiKey"] = "sk-ant-test"
            }
        };
        var service = new OpenAICompatibleTextEnhancementService(new HttpClient(handler), secrets);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.EnhanceAsync(
                Request(
                    endpoint: "https://api.anthropic.com/v1/messages",
                    providerId: "anthropic",
                    maxRetries: 1),
                CancellationToken.None));

        Assert.Contains("HTTP 400", ex.Message);
        Assert.DoesNotContain("sk-ant-test", ex.Message);
    }

    [Fact]
    public async Task EnhanceAsync_LocalCliProviderRunsCommandWithPromptEnvironment()
    {
        var runner = new FakeLocalCliRunner(new LocalCliProcessResult(0, "Local CLI text", string.Empty, TimedOut: false));
        var secrets = new FakeSecretStore { Secret = "unused-secret" };
        var service = new OpenAICompatibleTextEnhancementService(
            new HttpClient(new QueueHttpMessageHandler()),
            secrets,
            runner);

        var result = await service.EnhanceAsync(
            Request(
                endpoint: "claude -p \"%VOICEINK_FULL_PROMPT%\"",
                model: "local-cli",
                providerId: "local-cli"),
            CancellationToken.None);

        Assert.Equal("Local CLI text", result.Text);
        Assert.Equal("local-cli", result.ProviderName);
        Assert.Equal("local-cli", result.ModelName);
        Assert.Empty(secrets.ReadNames);
        Assert.NotNull(runner.Request);
        Assert.Equal("claude -p \"%VOICEINK_FULL_PROMPT%\"", runner.Request.CommandTemplate);
        Assert.Equal("system prompt", runner.Request.SystemPrompt);
        Assert.Equal("user prompt", runner.Request.UserPrompt);
        Assert.Contains("<SYSTEM_PROMPT>", runner.Request.FullPrompt);
        Assert.Contains("system prompt", runner.Request.FullPrompt);
        Assert.Contains("<USER_PROMPT>", runner.Request.FullPrompt);
        Assert.Contains("user prompt", runner.Request.FullPrompt);
        Assert.Equal(TimeSpan.FromSeconds(7), runner.Request.Timeout);
    }

    [Fact]
    public async Task EnhanceAsync_LocalCliProviderReportsEmptyOutput()
    {
        var runner = new FakeLocalCliRunner(new LocalCliProcessResult(0, "", "", TimedOut: false));
        var service = new OpenAICompatibleTextEnhancementService(
            new HttpClient(new QueueHttpMessageHandler()),
            new FakeSecretStore(),
            runner);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.EnhanceAsync(
                Request(
                    endpoint: "voiceink-local-test",
                    model: "local-cli",
                    providerId: "local-cli",
                    maxRetries: 1),
                CancellationToken.None));

        Assert.Equal("Local CLI command returned empty output.", ex.Message);
    }

    [Fact]
    public async Task EnhanceAsync_LocalCliProviderReportsNonzeroExit()
    {
        var runner = new FakeLocalCliRunner(new LocalCliProcessResult(2, "ignored", "bad stderr", TimedOut: false));
        var service = new OpenAICompatibleTextEnhancementService(
            new HttpClient(new QueueHttpMessageHandler()),
            new FakeSecretStore(),
            runner);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.EnhanceAsync(
                Request(
                    endpoint: "voiceink-local-test",
                    model: "local-cli",
                    providerId: "local-cli",
                    maxRetries: 1),
                CancellationToken.None));

        Assert.Equal("Local CLI command failed with exit code 2: bad stderr", ex.Message);
    }

    [Fact]
    public async Task EnhanceAsync_LocalCliProviderReportsTimeout()
    {
        var runner = new FakeLocalCliRunner(new LocalCliProcessResult(0, string.Empty, string.Empty, TimedOut: true));
        var service = new OpenAICompatibleTextEnhancementService(
            new HttpClient(new QueueHttpMessageHandler()),
            new FakeSecretStore(),
            runner);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.EnhanceAsync(
                Request(
                    endpoint: "voiceink-local-test",
                    model: "local-cli",
                    providerId: "local-cli",
                    maxRetries: 1),
                CancellationToken.None));

        Assert.Equal("Local CLI command timed out after 7 seconds.", ex.Message);
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

    private sealed class FakeLocalCliRunner(LocalCliProcessResult result) : ILocalCliProcessRunner
    {
        public LocalCliProcessRequest? Request { get; private set; }

        public Task<LocalCliProcessResult> RunAsync(
            LocalCliProcessRequest request,
            CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(result);
        }
    }
}
