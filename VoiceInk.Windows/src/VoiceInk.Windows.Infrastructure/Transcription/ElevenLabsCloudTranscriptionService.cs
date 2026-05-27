using System.Diagnostics;
using System.Net;
using System.Text.Json;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed class ElevenLabsCloudTranscriptionService(
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

        var apiKey = await ReadApiKeyAsync(options.CloudProviderId, cancellationToken)
            .ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("ElevenLabs transcription provider is not configured.");
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, EndpointWithoutQuery(endpoint!));
        message.Headers.Add("xi-api-key", apiKey);
        message.Content = CreateMultipartContent(audio, options, endpoint!);

        var startedAt = Stopwatch.GetTimestamp();
        using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException(SanitizedHttpError(response.StatusCode));
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var text = ExtractText(json);
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("ElevenLabs transcription provider returned no text.");
        }

        return new TranscriptionResult(
            text,
            Stopwatch.GetElapsedTime(startedAt),
            TranscriptionConfiguration.OpenAICompatibleProviderNameFor(options.CloudProviderId));
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

    private static MultipartFormDataContent CreateMultipartContent(
        AudioCaptureResult audio,
        TranscriptionOptions options,
        Uri endpoint)
    {
        var content = new MultipartFormDataContent();
        var fileStream = File.OpenRead(audio.FilePath);
        var audioContent = new StreamContent(fileStream);
        audioContent.Headers.ContentType = new("audio/wav");
        content.Add(audioContent, "file", Path.GetFileName(audio.FilePath));
        content.Add(new StringContent(options.CloudModel.Trim()), "model_id");

        if (!string.IsNullOrWhiteSpace(options.Language)
            && !string.Equals(options.Language.Trim(), "auto", StringComparison.OrdinalIgnoreCase))
        {
            content.Add(new StringContent(options.Language.Trim()), "language_code");
        }

        foreach (var option in EndpointQueryOptions(endpoint))
        {
            content.Add(new StringContent(option.Value), option.Name);
        }

        return content;
    }

    private static Uri EndpointWithoutQuery(Uri endpoint)
    {
        var builder = new UriBuilder(endpoint)
        {
            Query = string.Empty
        };

        return builder.Uri;
    }

    private static IEnumerable<(string Name, string Value)> EndpointQueryOptions(Uri endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint.Query))
        {
            yield break;
        }

        foreach (var part in endpoint.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pieces = part.Split('=', 2);
            var name = Uri.UnescapeDataString(pieces[0]).Trim();
            if (string.IsNullOrWhiteSpace(name)
                || ReservedMultipartFieldNames.Any(
                    reservedName => string.Equals(reservedName, name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var value = pieces.Length == 2
                ? Uri.UnescapeDataString(pieces[1]).Trim()
                : string.Empty;
            yield return (name, value);
        }
    }

    private static readonly string[] ReservedMultipartFieldNames =
    [
        "file",
        "model_id",
        "language_code"
    ];

    private static string ExtractText(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.TryGetProperty("text", out var text)
                ? text.GetString() ?? string.Empty
                : string.Empty;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("ElevenLabs transcription provider returned invalid JSON.", ex);
        }
    }

    private static string SanitizedHttpError(HttpStatusCode statusCode) =>
        $"ElevenLabs transcription provider returned HTTP {(int)statusCode}.";
}
