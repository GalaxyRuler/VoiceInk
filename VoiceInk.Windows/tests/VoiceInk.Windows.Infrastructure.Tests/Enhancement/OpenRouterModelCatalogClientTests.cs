using System.Net;
using VoiceInk.Windows.Infrastructure.Enhancement;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Enhancement;

public sealed class OpenRouterModelCatalogClientTests
{
    [Fact]
    public async Task ListModelsAsync_ReadsModelIdsFromModelsEndpoint()
    {
        var handler = new FakeHandler("""{"data":[{"id":"openai/gpt-oss-120b"},{"id":"anthropic/claude-sonnet-4.5"}]}""");
        var client = new OpenRouterModelCatalogClient(new HttpClient(handler));

        var result = await client.ListModelsAsync(
            "https://openrouter.ai/api/v1/chat/completions",
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(["anthropic/claude-sonnet-4.5", "openai/gpt-oss-120b"], result.Models);
        Assert.Equal(new Uri("https://openrouter.ai/api/v1/models"), handler.RequestUri);
    }

    [Fact]
    public async Task ListModelsAsync_DeduplicatesAndTrimsModelIds()
    {
        var handler = new FakeHandler("""{"data":[{"id":" openai/gpt-oss-120b "},{"id":"OPENAI/GPT-OSS-120B"},{"id":""},{"name":"missing id"}]}""");
        var client = new OpenRouterModelCatalogClient(new HttpClient(handler));

        var result = await client.ListModelsAsync(
            "https://openrouter.ai/api/v1/chat/completions",
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(["openai/gpt-oss-120b"], result.Models);
    }

    [Fact]
    public async Task ListModelsAsync_ReturnsFailureWhenNoModelsAreReturned()
    {
        var client = new OpenRouterModelCatalogClient(new HttpClient(new FakeHandler("""{"data":[]}""")));

        var result = await client.ListModelsAsync(
            "https://openrouter.ai/api/v1/chat/completions",
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("No OpenRouter models found", result.Message);
    }

    [Fact]
    public async Task ListModelsAsync_ReturnsFailureForInvalidEndpoint()
    {
        var client = new OpenRouterModelCatalogClient(new HttpClient(new FakeHandler("""{"data":[]}""")));

        var result = await client.ListModelsAsync("not a url", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("OpenRouter endpoint is not a valid URL", result.Message);
    }

    [Fact]
    public async Task ListModelsAsync_ReturnsSanitizedHttpFailure()
    {
        var client = new OpenRouterModelCatalogClient(new HttpClient(new FakeHandler("nope", HttpStatusCode.TooManyRequests)));

        var result = await client.ListModelsAsync(
            "https://openrouter.ai/api/v1/chat/completions",
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("OpenRouter model refresh failed: HTTP 429", result.Message);
    }

    private sealed class FakeHandler(string body, HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body)
            });
        }
    }
}
