using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed class DeepgramCloudTranscriptionService(
    HttpClient httpClient,
    ISecretStore secretStore) : ITranscriptionService
{
    public async Task<TranscriptionResult> TranscribeAsync(
        AudioCaptureResult audio,
        TranscriptionOptions options,
        CancellationToken cancellationToken)
    {
        if (!TranscriptionConfiguration.TryCreateCloudEndpoint(
            options.CloudEndpoint,
            out var endpoint,
            out var endpointError))
        {
            throw new InvalidOperationException(endpointError);
        }

        if (string.IsNullOrWhiteSpace(options.CloudModel))
        {
            throw new InvalidOperationException("Cloud transcription model is required.");
        }

        var apiKey = await ReadApiKeyAsync(options.CloudProviderId, cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Cloud transcription provider is not configured.");
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, BuildRequestUri(endpoint!, options));
        message.Headers.Authorization = new AuthenticationHeaderValue("Token", apiKey);
        message.Content = CreateAudioContent(audio);

        var startedAt = Stopwatch.GetTimestamp();
        using var response = await httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(SanitizedHttpError(response.StatusCode));
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var text = ExtractText(json);
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Deepgram transcription provider returned no text.");
        }

        return new TranscriptionResult(
            text,
            Stopwatch.GetElapsedTime(startedAt),
            "deepgram");
    }

    private async Task<string?> ReadApiKeyAsync(
        string providerId,
        CancellationToken cancellationToken)
    {
        foreach (var secretName in TranscriptionConfiguration.SecretNamesForCloudProvider(providerId))
        {
            var apiKey = await secretStore.ReadSecretAsync(secretName, cancellationToken);
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                return apiKey;
            }
        }

        return null;
    }

    private static Uri BuildRequestUri(Uri endpoint, TranscriptionOptions options)
    {
        var builder = new UriBuilder(endpoint);
        var queryParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(endpoint.Query))
        {
            queryParts.Add(endpoint.Query.TrimStart('?'));
        }

        queryParts.Add(QueryParameter("model", options.CloudModel.Trim()));
        if (!HasQueryParameter(endpoint.Query, "smart_format"))
        {
            queryParts.Add(QueryParameter("smart_format", "true"));
        }

        if (!string.IsNullOrWhiteSpace(options.Language)
            && !string.Equals(options.Language.Trim(), "auto", StringComparison.OrdinalIgnoreCase)
            && !HasQueryParameter(endpoint.Query, "language"))
        {
            queryParts.Add(QueryParameter("language", options.Language.Trim()));
        }

        builder.Query = string.Join("&", queryParts);
        return builder.Uri;
    }

    private static StreamContent CreateAudioContent(AudioCaptureResult audio)
    {
        var content = new StreamContent(File.OpenRead(audio.FilePath));
        content.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        return content;
    }

    private static string ExtractText(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("results", out var results)
                || !results.TryGetProperty("channels", out var channels)
                || channels.ValueKind != JsonValueKind.Array
                || channels.GetArrayLength() == 0)
            {
                throw UnexpectedResponse();
            }

            if (!channels[0].TryGetProperty("alternatives", out var alternatives)
                || alternatives.ValueKind != JsonValueKind.Array
                || alternatives.GetArrayLength() == 0
                || !alternatives[0].TryGetProperty("transcript", out var transcript))
            {
                throw UnexpectedResponse();
            }

            return transcript.GetString() ?? string.Empty;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Deepgram transcription provider returned invalid JSON.", ex);
        }
        catch (KeyNotFoundException ex)
        {
            throw new InvalidOperationException("Deepgram transcription provider returned an unexpected response.", ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException("Deepgram transcription provider returned an unexpected response.", ex);
        }
    }

    private static InvalidOperationException UnexpectedResponse() =>
        new("Deepgram transcription provider returned an unexpected response.");

    private static string QueryParameter(string name, string value) =>
        $"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}";

    private static bool HasQueryParameter(string query, string name)
    {
        var trimmed = query.TrimStart('?');
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return false;
        }

        return trimmed
            .Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => part.Split('=', 2)[0])
            .Any(part => string.Equals(Uri.UnescapeDataString(part), name, StringComparison.OrdinalIgnoreCase));
    }

    private static string SanitizedHttpError(HttpStatusCode statusCode) =>
        $"Deepgram transcription provider returned HTTP {(int)statusCode}.";
}
