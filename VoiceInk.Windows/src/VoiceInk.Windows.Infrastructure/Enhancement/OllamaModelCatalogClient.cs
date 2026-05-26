using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VoiceInk.Windows.Infrastructure.Enhancement;

public sealed class OllamaModelCatalogClient(HttpClient httpClient)
{
    public async Task<OllamaModelCatalogResult> ListModelsAsync(
        string endpoint,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
        {
            return OllamaModelCatalogResult.Failure("Ollama endpoint is not a valid URL");
        }

        var tagsUri = new UriBuilder(endpointUri.Scheme, endpointUri.Host, endpointUri.Port, "/api/tags").Uri;
        try
        {
            using var response = await httpClient.GetAsync(tagsUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OllamaModelCatalogResult.Failure(
                    $"Ollama model refresh failed: HTTP {(int)response.StatusCode}");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<OllamaTagsResponse>(
                stream,
                cancellationToken: cancellationToken);
            var models = payload?.Models?
                .Select(model => string.IsNullOrWhiteSpace(model.Name) ? model.Model : model.Name)
                .Where(model => !string.IsNullOrWhiteSpace(model))
                .Select(model => model!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? [];

            return models.Length == 0
                ? OllamaModelCatalogResult.Failure("No Ollama models found")
                : OllamaModelCatalogResult.FromModels(models);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (JsonException)
        {
            return OllamaModelCatalogResult.Failure("Ollama model refresh returned invalid JSON");
        }
        catch (HttpRequestException ex)
        {
            return OllamaModelCatalogResult.Failure(
                ex.StatusCode is HttpStatusCode statusCode
                    ? $"Ollama model refresh failed: HTTP {(int)statusCode}"
                    : "Ollama model refresh failed: local server unavailable");
        }
        catch (Exception ex)
        {
            return OllamaModelCatalogResult.Failure($"Ollama model refresh failed: {ex.Message}");
        }
    }

    private sealed record OllamaTagsResponse([property: JsonPropertyName("models")] IReadOnlyList<OllamaModel>? Models);

    private sealed record OllamaModel(
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("model")] string? Model);
}

public sealed record OllamaModelCatalogResult(
    bool Success,
    IReadOnlyList<string> Models,
    string Message)
{
    public static OllamaModelCatalogResult FromModels(IReadOnlyList<string> models) =>
        new(true, models, $"Loaded {models.Count} Ollama model{(models.Count == 1 ? string.Empty : "s")}");

    public static OllamaModelCatalogResult Failure(string message) =>
        new(false, [], message);
}
