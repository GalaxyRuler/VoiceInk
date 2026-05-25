using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed class GeminiCloudTranscriptionService(
    HttpClient httpClient,
    ISecretStore secretStore) : ITranscriptionService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<TranscriptionResult> TranscribeAsync(
        AudioCaptureResult audio,
        TranscriptionOptions options,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.CloudEndpoint)
            || string.IsNullOrWhiteSpace(options.CloudModel))
        {
            throw new InvalidOperationException("Cloud transcription endpoint and model are required.");
        }

        var apiKey = await ReadApiKeyAsync(options.CloudProviderId, cancellationToken)
            .ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Gemini transcription provider is not configured.");
        }

        var startedAt = Stopwatch.GetTimestamp();
        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            GenerateContentUri(options.CloudEndpoint, options.CloudModel));
        message.Headers.Add("x-goog-api-key", apiKey);
        message.Content = JsonContent.Create(await CreatePayloadAsync(audio, options, cancellationToken)
            .ConfigureAwait(false), options: JsonOptions);

        using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        var text = await ReadTranscriptTextAsync(response, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Gemini transcription provider returned no text.");
        }

        return new TranscriptionResult(
            text.Trim(),
            Stopwatch.GetElapsedTime(startedAt),
            TranscriptionConfiguration.OpenAICompatibleProviderNameFor(options.CloudProviderId));
    }

    private static async Task<object> CreatePayloadAsync(
        AudioCaptureResult audio,
        TranscriptionOptions options,
        CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(audio.FilePath, cancellationToken).ConfigureAwait(false);
        return new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { text = PromptText(options) },
                        new
                        {
                            inline_data = new
                            {
                                mime_type = "audio/wav",
                                data = Convert.ToBase64String(bytes)
                            }
                        }
                    }
                }
            }
        };
    }

    private static string PromptText(TranscriptionOptions options)
    {
        var prompt = "Transcribe this audio verbatim as plain text. Return only the transcript.";
        if (!string.IsNullOrWhiteSpace(options.Language)
            && !string.Equals(options.Language.Trim(), "auto", StringComparison.OrdinalIgnoreCase))
        {
            prompt += $" The spoken language uses language code {options.Language.Trim()}.";
        }

        if (!string.IsNullOrWhiteSpace(options.Prompt))
        {
            prompt += $" Additional user guidance: {options.Prompt.Trim()}";
        }

        return prompt;
    }

    private static Uri GenerateContentUri(string endpoint, string model)
    {
        if (!Uri.TryCreate(endpoint.Trim(), UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException("Cloud transcription endpoint is invalid.");
        }

        var baseText = baseUri.ToString().TrimEnd('/');
        return new Uri($"{baseText}/{Uri.EscapeDataString(model.Trim())}:generateContent");
    }

    private async Task<string?> ReadApiKeyAsync(
        string providerId,
        CancellationToken cancellationToken)
    {
        foreach (var secretName in TranscriptionConfiguration.SecretNamesForCloudProvider(providerId))
        {
            var apiKey = await secretStore.ReadSecretAsync(secretName, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                return apiKey;
            }
        }

        return null;
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new InvalidOperationException(SanitizedHttpError(response.StatusCode));
    }

    private static async Task<string> ReadTranscriptTextAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (!document.RootElement.TryGetProperty("candidates", out var candidates)
                || candidates.ValueKind != JsonValueKind.Array)
            {
                return string.Empty;
            }

            var parts = new List<string>();
            foreach (var candidate in candidates.EnumerateArray())
            {
                if (!candidate.TryGetProperty("content", out var content)
                    || !content.TryGetProperty("parts", out var contentParts)
                    || contentParts.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var part in contentParts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var text)
                        && text.ValueKind == JsonValueKind.String)
                    {
                        parts.Add(text.GetString() ?? string.Empty);
                    }
                }
            }

            return string.Concat(parts).Trim();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Gemini transcription provider returned invalid JSON.", ex);
        }
    }

    private static string SanitizedHttpError(HttpStatusCode statusCode) =>
        $"Gemini transcription provider returned HTTP {(int)statusCode}.";
}
