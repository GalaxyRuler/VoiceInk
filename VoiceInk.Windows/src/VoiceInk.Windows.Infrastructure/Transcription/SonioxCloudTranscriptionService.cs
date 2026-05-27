using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed class SonioxCloudTranscriptionService(
    HttpClient httpClient,
    ISecretStore secretStore,
    TimeSpan? pollDelay = null,
    int maxPollAttempts = 120) : ITranscriptionService
{
    private static readonly Uri BaseUri = new("https://api.soniox.com/v1/");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly TimeSpan pollDelay = pollDelay ?? TimeSpan.FromSeconds(1);

    public async Task<TranscriptionResult> TranscribeAsync(
        AudioCaptureResult audio,
        TranscriptionOptions options,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.CloudModel))
        {
            throw new InvalidOperationException("Cloud transcription model is required.");
        }

        if (!TranscriptionConfiguration.TryCreateCloudEndpoint(
            options.CloudEndpoint,
            out var endpoint,
            out var endpointError))
        {
            throw new InvalidOperationException(endpointError);
        }

        var apiKey = await ReadApiKeyAsync(options.CloudProviderId, cancellationToken)
            .ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Soniox transcription provider is not configured.");
        }

        string? fileId = null;
        string? transcriptionId = null;
        var startedAt = Stopwatch.GetTimestamp();
        try
        {
            fileId = await UploadFileAsync(audio, apiKey, cancellationToken).ConfigureAwait(false);
            transcriptionId = await CreateTranscriptionAsync(fileId, options, endpoint!, apiKey, cancellationToken)
                .ConfigureAwait(false);
            await WaitForCompletionAsync(transcriptionId, apiKey, cancellationToken).ConfigureAwait(false);
            var text = await GetTranscriptTextAsync(transcriptionId, apiKey, cancellationToken)
                .ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException("Soniox transcription provider returned no text.");
            }

            return new TranscriptionResult(
                text,
                Stopwatch.GetElapsedTime(startedAt),
                TranscriptionConfiguration.OpenAICompatibleProviderNameFor(options.CloudProviderId));
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(transcriptionId))
            {
                await DeleteNoThrowAsync($"transcriptions/{Uri.EscapeDataString(transcriptionId)}", apiKey, cancellationToken)
                    .ConfigureAwait(false);
            }

            if (!string.IsNullOrWhiteSpace(fileId))
            {
                await DeleteNoThrowAsync($"files/{Uri.EscapeDataString(fileId)}", apiKey, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }

    private async Task<string> UploadFileAsync(
        AudioCaptureResult audio,
        string apiKey,
        CancellationToken cancellationToken)
    {
        using var message = CreateRequest(HttpMethod.Post, "files", apiKey);
        var content = new MultipartFormDataContent();
        var fileStream = File.OpenRead(audio.FilePath);
        var audioContent = new StreamContent(fileStream);
        audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        content.Add(audioContent, "file", Path.GetFileName(audio.FilePath));
        message.Content = content;

        using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        using var document = await ReadJsonAsync(response, cancellationToken).ConfigureAwait(false);
        return RequiredString(document.RootElement, "id");
    }

    private async Task<string> CreateTranscriptionAsync(
        string fileId,
        TranscriptionOptions options,
        Uri endpoint,
        string apiKey,
        CancellationToken cancellationToken)
    {
        using var message = CreateRequest(HttpMethod.Post, "transcriptions", apiKey);
        var payload = new Dictionary<string, object?>
        {
            ["model"] = options.CloudModel.Trim(),
            ["file_id"] = fileId
        };
        if (!string.IsNullOrWhiteSpace(options.Language)
            && !string.Equals(options.Language.Trim(), "auto", StringComparison.OrdinalIgnoreCase))
        {
            payload["language_hints"] = new[] { options.Language.Trim() };
        }

        foreach (var option in EndpointQueryOptions(endpoint))
        {
            payload.TryAdd(option.Name, option.Value);
        }

        message.Content = new StringContent(
            JsonSerializer.Serialize(payload, JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        using var document = await ReadJsonAsync(response, cancellationToken).ConfigureAwait(false);
        return RequiredString(document.RootElement, "id");
    }

    private async Task WaitForCompletionAsync(
        string transcriptionId,
        string apiKey,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < maxPollAttempts; attempt++)
        {
            using var message = CreateRequest(
                HttpMethod.Get,
                $"transcriptions/{Uri.EscapeDataString(transcriptionId)}",
                apiKey);
            using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
            await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
            using var document = await ReadJsonAsync(response, cancellationToken).ConfigureAwait(false);
            var status = document.RootElement.TryGetProperty("status", out var statusElement)
                ? statusElement.GetString()
                : null;
            if (string.Equals(status, "completed", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.Equals(status, "error", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Soniox transcription provider failed.");
            }

            if (pollDelay > TimeSpan.Zero)
            {
                await Task.Delay(pollDelay, cancellationToken).ConfigureAwait(false);
            }
        }

        throw new TimeoutException("Soniox transcription provider timed out.");
    }

    private async Task<string> GetTranscriptTextAsync(
        string transcriptionId,
        string apiKey,
        CancellationToken cancellationToken)
    {
        using var message = CreateRequest(
            HttpMethod.Get,
            $"transcriptions/{Uri.EscapeDataString(transcriptionId)}/transcript",
            apiKey);
        using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        using var document = await ReadJsonAsync(response, cancellationToken).ConfigureAwait(false);
        if (!document.RootElement.TryGetProperty("tokens", out var tokens)
            || tokens.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        var parts = new List<string>();
        foreach (var token in tokens.EnumerateArray())
        {
            if (token.TryGetProperty("text", out var textElement)
                && textElement.ValueKind == JsonValueKind.String)
            {
                parts.Add(textElement.GetString() ?? string.Empty);
            }
        }

        return string.Concat(parts).Trim();
    }

    private async Task DeleteNoThrowAsync(
        string relativePath,
        string apiKey,
        CancellationToken cancellationToken)
    {
        try
        {
            using var message = CreateRequest(HttpMethod.Delete, relativePath, apiKey);
            using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
            await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
        }
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

    private static IEnumerable<(string Name, object Value)> EndpointQueryOptions(Uri endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint.Query))
        {
            yield break;
        }

        foreach (var part in endpoint.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pieces = part.Split('=', 2);
            var name = DecodeQueryComponent(pieces[0]).Trim();
            if (string.IsNullOrWhiteSpace(name)
                || ReservedTranscriptionPayloadNames.Any(
                    reservedName => string.Equals(reservedName, name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var value = pieces.Length == 2
                ? DecodeQueryComponent(pieces[1]).Trim()
                : string.Empty;
            yield return (name, TypedQueryValue(value));
        }
    }

    private static object TypedQueryValue(string value)
    {
        if (bool.TryParse(value, out var booleanValue))
        {
            return booleanValue;
        }

        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integerValue))
        {
            return integerValue;
        }

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
        {
            return doubleValue;
        }

        return value;
    }

    private static string DecodeQueryComponent(string value) =>
        Uri.UnescapeDataString(value.Replace("+", " ", StringComparison.Ordinal));

    private static readonly string[] ReservedTranscriptionPayloadNames =
    [
        "file_id",
        "language_hints",
        "model"
    ];

    private static HttpRequestMessage CreateRequest(HttpMethod method, string relativePath, string apiKey)
    {
        var request = new HttpRequestMessage(method, new Uri(BaseUri, relativePath));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return request;
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

    private static async Task<JsonDocument> ReadJsonAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);
            return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Soniox transcription provider returned invalid JSON.", ex);
        }
    }

    private static string RequiredString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(property.GetString()))
        {
            return property.GetString()!;
        }

        throw new InvalidOperationException("Soniox transcription provider returned an unexpected response.");
    }

    private static string SanitizedHttpError(HttpStatusCode statusCode) =>
        $"Soniox transcription provider returned HTTP {(int)statusCode}.";
}
