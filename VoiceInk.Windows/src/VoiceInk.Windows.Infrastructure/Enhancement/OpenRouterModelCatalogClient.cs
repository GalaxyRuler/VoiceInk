using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VoiceInk.Windows.Infrastructure.Enhancement;

public sealed class OpenRouterModelCatalogClient(HttpClient httpClient)
{
    public async Task<OpenRouterModelCatalogResult> ListModelsAsync(
        string endpoint,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
        {
            return OpenRouterModelCatalogResult.Failure("OpenRouter endpoint is not a valid URL");
        }

        var modelsUri = new UriBuilder(endpointUri.Scheme, endpointUri.Host, endpointUri.Port, "/api/v1/models").Uri;
        try
        {
            using var response = await httpClient.GetAsync(modelsUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OpenRouterModelCatalogResult.Failure(
                    $"OpenRouter model refresh failed: HTTP {(int)response.StatusCode}");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<OpenRouterModelsResponse>(
                stream,
                cancellationToken: cancellationToken);
            var models = payload?.Data?
                .Select(model => model.Id)
                .Where(model => !string.IsNullOrWhiteSpace(model))
                .Select(model => model!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? [];

            return models.Length == 0
                ? OpenRouterModelCatalogResult.Failure("No OpenRouter models found")
                : OpenRouterModelCatalogResult.FromModels(models);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (JsonException)
        {
            return OpenRouterModelCatalogResult.Failure("OpenRouter model refresh returned invalid JSON");
        }
        catch (HttpRequestException ex)
        {
            return OpenRouterModelCatalogResult.Failure(
                ex.StatusCode is HttpStatusCode statusCode
                    ? $"OpenRouter model refresh failed: HTTP {(int)statusCode}"
                    : "OpenRouter model refresh failed: service unavailable");
        }
        catch (Exception ex)
        {
            return OpenRouterModelCatalogResult.Failure($"OpenRouter model refresh failed: {ex.Message}");
        }
    }

    private sealed record OpenRouterModelsResponse(
        [property: JsonPropertyName("data")] IReadOnlyList<OpenRouterModel>? Data);

    private sealed record OpenRouterModel([property: JsonPropertyName("id")] string? Id);
}

public sealed record OpenRouterModelCatalogResult(
    bool Success,
    IReadOnlyList<string> Models,
    string Message)
{
    public static OpenRouterModelCatalogResult FromModels(IReadOnlyList<string> models) =>
        new(true, models, $"Loaded {models.Count} OpenRouter model{(models.Count == 1 ? string.Empty : "s")}");

    public static OpenRouterModelCatalogResult Failure(string message) =>
        new(false, [], message);
}
