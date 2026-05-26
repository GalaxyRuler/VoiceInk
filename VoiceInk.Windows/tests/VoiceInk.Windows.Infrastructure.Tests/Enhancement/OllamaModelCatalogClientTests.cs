using System.Net;
using VoiceInk.Windows.Infrastructure.Enhancement;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Enhancement;

public sealed class OllamaModelCatalogClientTests
{
    [Fact]
    public async Task ListModelsAsync_ReadsModelNamesFromTagsEndpoint()
    {
        var handler = new FakeHandler("""{"models":[{"name":"llama3.2:latest"},{"model":"mistral:7b"}]}""");
        var client = new OllamaModelCatalogClient(new HttpClient(handler));

        var result = await client.ListModelsAsync(
            "http://localhost:11434/v1/chat/completions",
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(["llama3.2:latest", "mistral:7b"], result.Models);
        Assert.Equal(new Uri("http://localhost:11434/api/tags"), handler.RequestUri);
    }

    [Fact]
    public async Task ListModelsAsync_ReturnsFailureWhenNoModelsAreReturned()
    {
        var client = new OllamaModelCatalogClient(new HttpClient(new FakeHandler("""{"models":[]}""")));

        var result = await client.ListModelsAsync(
            "http://localhost:11434/v1/chat/completions",
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("No Ollama models found", result.Message);
    }

    [Fact]
    public async Task ListModelsAsync_ReturnsFailureForInvalidEndpoint()
    {
        var client = new OllamaModelCatalogClient(new HttpClient(new FakeHandler("""{"models":[]}""")));

        var result = await client.ListModelsAsync("not a url", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Ollama endpoint is not a valid URL", result.Message);
    }

    [Fact]
    public async Task ListModelsAsync_ReturnsSanitizedHttpFailure()
    {
        var client = new OllamaModelCatalogClient(new HttpClient(new FakeHandler("nope", HttpStatusCode.InternalServerError)));

        var result = await client.ListModelsAsync(
            "http://localhost:11434/v1/chat/completions",
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Ollama model refresh failed: HTTP 500", result.Message);
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
