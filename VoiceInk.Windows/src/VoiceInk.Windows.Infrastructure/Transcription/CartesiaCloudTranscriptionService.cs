using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed class CartesiaCloudTranscriptionService(
    HttpClient httpClient,
    ISecretStore secretStore) : ITranscriptionService
{
    private const string CartesiaVersion = "2026-03-01";

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

        var apiKey = await ReadApiKeyAsync(options.CloudProviderId, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Cartesia transcription provider is not configured.");
        }

        var startedAt = Stopwatch.GetTimestamp();
        using var message = new HttpRequestMessage(HttpMethod.Post, CreateEndpoint(options.CloudEndpoint));
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        message.Headers.Add("Cartesia-Version", CartesiaVersion);
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(options.CloudModel.Trim()), "model");
        if (!string.IsNullOrWhiteSpace(options.Language)
            && !string.Equals(options.Language.Trim(), "auto", StringComparison.OrdinalIgnoreCase))
        {
            content.Add(new StringContent(options.Language.Trim()), "language");
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
            throw new InvalidOperationException("Cartesia transcription provider returned no text.");
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

    private static Uri CreateEndpoint(string endpoint)
    {
        if (!Uri.TryCreate(endpoint.Trim(), UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException("Cloud transcription endpoint is invalid.");
        }

        return uri;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new InvalidOperationException($"Cartesia transcription provider returned HTTP {(int)response.StatusCode}.");
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
            throw new InvalidOperationException("Cartesia transcription provider returned invalid JSON.", ex);
        }
    }
}
