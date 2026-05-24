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
    public const string SecretName = "VoiceInk.Windows.Enhancement.OpenAICompatible.ApiKey";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<TextEnhancementResult> EnhanceAsync(
        TextEnhancementRequest request,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(request.Endpoint, UriKind.Absolute, out var endpoint))
        {
            throw new InvalidOperationException("AI enhancement endpoint is invalid.");
        }

        if (string.IsNullOrWhiteSpace(request.Model))
        {
            throw new InvalidOperationException("AI enhancement model is required.");
        }

        var apiKey = await secretStore.ReadSecretAsync(SecretName, cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("AI enhancement provider is not configured.");
        }

        var attempts = Math.Max(1, request.MaxRetries);
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                return await SendOnceAsync(endpoint, apiKey, request, cancellationToken);
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
        string apiKey,
        TextEnhancementRequest request,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(request.Timeout);

        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        message.Content = new StringContent(
            JsonSerializer.Serialize(
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

            var content = ExtractMessageContent(json);
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("AI enhancement provider returned no text.");
            }

            return new TextEnhancementResult(
                content,
                "openai-compatible",
                request.Model,
                Stopwatch.GetElapsedTime(startedAt));
        }
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

    private sealed class TransientEnhancementException(string message) : Exception(message);
}
