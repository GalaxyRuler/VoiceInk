using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Infrastructure.Enhancement;

public sealed class OpenAICompatibleTextEnhancementService(
    HttpClient httpClient,
    ISecretStore secretStore) : ITextEnhancementService
{
    public const string SecretName = EnhancementConfiguration.LegacyCustomSecretName;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<TextEnhancementResult> EnhanceAsync(
        TextEnhancementRequest request,
        CancellationToken cancellationToken)
    {
        if (!EnhancementConfiguration.TryCreateEndpoint(
            request.Endpoint,
            out var endpoint,
            out var endpointError))
        {
            throw new InvalidOperationException(endpointError);
        }

        if (string.IsNullOrWhiteSpace(request.Model))
        {
            throw new InvalidOperationException("AI enhancement model is required.");
        }

        var provider = EnhancementProviderPresetCatalog.Resolve(request.ProviderId);
        var providerName = EnhancementConfiguration.ProviderNameFor(provider.Id);
        var apiKey = provider.RequiresApiKey
            ? await ReadApiKeyAsync(provider.Id, cancellationToken)
            : null;
        if (provider.RequiresApiKey && string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("AI enhancement provider is not configured.");
        }

        var attempts = Math.Max(1, request.MaxRetries);
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                return await SendOnceAsync(endpoint!, apiKey, providerName, request, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TimeoutException) when (request.RetryOnTimeout && attempt < attempts)
            {
                await DelayBeforeRetryAsync(attempt, cancellationToken);
            }
            catch (TimeoutException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
            catch (TransientEnhancementException) when (attempt < attempts)
            {
                await DelayBeforeRetryAsync(attempt, cancellationToken);
            }
            catch (TransientEnhancementException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        throw new InvalidOperationException("AI enhancement failed after retrying transient provider errors.");
    }

    private async Task<TextEnhancementResult> SendOnceAsync(
        Uri endpoint,
        string? apiKey,
        string providerName,
        TextEnhancementRequest request,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(request.Timeout);

        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint);
        var isAnthropic = string.Equals(
            request.ProviderId,
            EnhancementProviderPresetCatalog.Anthropic.Id,
            StringComparison.OrdinalIgnoreCase);
        if (isAnthropic && !string.IsNullOrWhiteSpace(apiKey))
        {
            message.Headers.Add("x-api-key", apiKey);
            message.Headers.Add("anthropic-version", "2023-06-01");
        }
        else if (!string.IsNullOrWhiteSpace(apiKey))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        message.Content = new StringContent(
            isAnthropic
                ? JsonSerializer.Serialize(
                    new AnthropicMessagesRequest(
                        request.Model,
                        request.SystemMessage,
                        [new AnthropicMessage("user", request.UserMessage)],
                        MaxTokens: 1024,
                        request.Temperature),
                    JsonOptions)
                : JsonSerializer.Serialize(
                    new ChatCompletionsRequest(
                        request.Model,
                        [
                            new ChatMessage("system", request.SystemMessage),
                            new ChatMessage("user", request.UserMessage)
                        ],
                        request.Temperature),
                    JsonOptions),
            Encoding.UTF8,
            "application/json");

        var startedAt = Stopwatch.GetTimestamp();
        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(message, timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("AI enhancement request timed out.");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var sanitized = SanitizedHttpError(response.StatusCode);
                if (IsTransientStatus(response.StatusCode))
                {
                    throw new TransientEnhancementException(sanitized);
                }

                throw new InvalidOperationException(sanitized);
            }

            string json;
            try
            {
                json = await response.Content.ReadAsStringAsync(timeout.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException("AI enhancement request timed out.");
            }

            var content = isAnthropic
                ? ExtractAnthropicMessageContent(json)
                : ExtractMessageContent(json);
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("AI enhancement provider returned no text.");
            }

            return new TextEnhancementResult(
                content,
                providerName,
                request.Model,
                Stopwatch.GetElapsedTime(startedAt));
        }
    }

    private async Task<string?> ReadApiKeyAsync(
        string providerId,
        CancellationToken cancellationToken)
    {
        foreach (var secretName in EnhancementConfiguration.SecretNamesForProvider(providerId))
        {
            var apiKey = await secretStore.ReadSecretAsync(secretName, cancellationToken);
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                return apiKey;
            }
        }

        return null;
    }

    private static string ExtractMessageContent(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var choices = document.RootElement.GetProperty("choices");
            if (choices.GetArrayLength() == 0)
            {
                return string.Empty;
            }

            return choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
                ?? string.Empty;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("AI enhancement provider returned invalid JSON.", ex);
        }
        catch (KeyNotFoundException ex)
        {
            throw new InvalidOperationException("AI enhancement provider returned an unexpected response.", ex);
        }
    }

    private static string ExtractAnthropicMessageContent(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var content = document.RootElement.GetProperty("content");
            var textBlocks = content.EnumerateArray()
                .Where(block => block.TryGetProperty("type", out var type)
                    && string.Equals(type.GetString(), "text", StringComparison.OrdinalIgnoreCase)
                    && block.TryGetProperty("text", out _))
                .Select(block => block.GetProperty("text").GetString())
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToArray();

            return string.Join(Environment.NewLine, textBlocks);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("AI enhancement provider returned invalid JSON.", ex);
        }
        catch (KeyNotFoundException ex)
        {
            throw new InvalidOperationException("AI enhancement provider returned an unexpected response.", ex);
        }
    }

    private static string SanitizedHttpError(HttpStatusCode statusCode) =>
        $"AI enhancement provider returned HTTP {(int)statusCode}.";

    private static bool IsTransientStatus(HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.TooManyRequests || (int)statusCode is >= 500 and <= 599;

    private static Task DelayBeforeRetryAsync(int attempt, CancellationToken cancellationToken) =>
        Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), cancellationToken);

    private sealed record ChatCompletionsRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyList<ChatMessage> Messages,
        [property: JsonPropertyName("temperature")] double Temperature);

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record AnthropicMessagesRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("system")] string System,
        [property: JsonPropertyName("messages")] IReadOnlyList<AnthropicMessage> Messages,
        [property: JsonPropertyName("max_tokens")] int MaxTokens,
        [property: JsonPropertyName("temperature")] double Temperature);

    private sealed record AnthropicMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed class TransientEnhancementException(string message) : Exception(message);
}
