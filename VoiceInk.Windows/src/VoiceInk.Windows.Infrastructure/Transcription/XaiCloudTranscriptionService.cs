using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed class XaiCloudTranscriptionService(
    HttpClient httpClient,
    ISecretStore secretStore) : ITranscriptionService
{
    public async Task<TranscriptionResult> TranscribeAsync(
        AudioCaptureResult audio,
        TranscriptionOptions options,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.CloudEndpoint)
            || string.IsNullOrWhiteSpace(options.CloudModel))
        {
            throw new InvalidOperationException(TranscriptionConfiguration.CloudProviderRequiredMessage);
        }

        if (!TranscriptionConfiguration.TryCreateCloudEndpoint(
            options.CloudEndpoint,
            out var endpoint,
            out var endpointError))
        {
            throw new InvalidOperationException(endpointError);
        }

        var apiKey = await ReadApiKeyAsync(options.CloudProviderId, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("xAI transcription provider is not configured.");
        }

        var startedAt = Stopwatch.GetTimestamp();
        using var message = new HttpRequestMessage(HttpMethod.Post, EndpointWithoutQuery(endpoint!));
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(options.CloudModel.Trim()), "model");
        content.Add(new StringContent("true"), "format");
        if (!string.IsNullOrWhiteSpace(options.Language)
            && !string.Equals(options.Language.Trim(), "auto", StringComparison.OrdinalIgnoreCase))
        {
            content.Add(new StringContent(options.Language.Trim()), "language");
        }

        foreach (var option in EndpointQueryOptions(endpoint!))
        {
            content.Add(new StringContent(option.Value), option.Name);
        }

        var fileStream = File.OpenRead(audio.FilePath);
        var audioContent = new StreamContent(fileStream);
        audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        content.Add(audioContent, "file", Path.GetFileName(audio.FilePath));
        message.Content = content;

        using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        var text = await ReadTextAsync(response, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("xAI transcription provider returned no text.");
        }

        return new TranscriptionResult(
            text.Trim(),
            Stopwatch.GetElapsedTime(startedAt),
            TranscriptionConfiguration.OpenAICompatibleProviderNameFor(options.CloudProviderId));
    }

    private async Task<string?> ReadApiKeyAsync(string providerId, CancellationToken cancellationToken)
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
            var name = DecodeQueryComponent(pieces[0]).Trim();
            if (string.IsNullOrWhiteSpace(name)
                || ReservedMultipartFieldNames.Any(
                    reserved => string.Equals(reserved, name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var value = pieces.Length == 2
                ? DecodeQueryComponent(pieces[1]).Trim()
                : string.Empty;
            yield return (name, value);
        }
    }

    private static string DecodeQueryComponent(string value) =>
        Uri.UnescapeDataString(value.Replace("+", " "));

    private static readonly string[] ReservedMultipartFieldNames =
    [
        "file",
        "format",
        "language",
        "model"
    ];

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new InvalidOperationException($"xAI transcription provider returned HTTP {(int)response.StatusCode}.");
    }

    private static async Task<string> ReadTextAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (document.RootElement.TryGetProperty("text", out var text)
                && text.ValueKind == JsonValueKind.String)
            {
                return text.GetString() ?? string.Empty;
            }

            return string.Empty;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("xAI transcription provider returned invalid JSON.", ex);
        }
    }
}
