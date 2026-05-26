using System.Net;
using System.Net.Http.Headers;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed class CloudTranscriptionProviderProbeService(HttpClient httpClient, ISecretStore secretStore)
{
    private static readonly HashSet<string> OpenAICompatibleProbeProviders =
    [
        TranscriptionProviderPresetCatalog.Custom.Id,
        TranscriptionProviderPresetCatalog.Groq.Id,
        TranscriptionProviderPresetCatalog.Mistral.Id,
        TranscriptionProviderPresetCatalog.Xai.Id
    ];

    public async Task<CloudTranscriptionProviderProbeResult> ProbeAsync(
        AppSettings settings,
        CancellationToken cancellationToken)
    {
        var providerId = TranscriptionProviderPresetCatalog.Resolve(settings.CloudTranscriptionProviderId).Id;
        var preset = TranscriptionProviderPresetCatalog.Resolve(providerId);
        var apiKey = await ReadApiKeyAsync(providerId, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new CloudTranscriptionProviderProbeResult(false, $"No {preset.DisplayName} API key stored");
        }

        var request = BuildProbeRequest(settings, preset, apiKey);
        if (request is null)
        {
            return new CloudTranscriptionProviderProbeResult(
                false,
                $"{preset.DisplayName} provider test request is not available yet");
        }

        using (request)
        using (var response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false))
        {
            if (response.IsSuccessStatusCode)
            {
                return new CloudTranscriptionProviderProbeResult(true, $"{preset.DisplayName} test request succeeded");
            }

            return new CloudTranscriptionProviderProbeResult(
                false,
                $"{preset.DisplayName} test request was rejected ({(int)response.StatusCode} {response.StatusCode})");
        }
    }

    private static HttpRequestMessage? BuildProbeRequest(
        AppSettings settings,
        TranscriptionProviderPreset preset,
        string apiKey)
    {
        if (preset.Id == TranscriptionProviderPresetCatalog.Deepgram.Id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.deepgram.com/v1/auth/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Token", apiKey);
            return request;
        }

        if (preset.Id == TranscriptionProviderPresetCatalog.AssemblyAI.Id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.assemblyai.com/v2/transcript?limit=1");
            request.Headers.TryAddWithoutValidation("Authorization", apiKey);
            return request;
        }

        if (preset.Id == TranscriptionProviderPresetCatalog.ElevenLabs.Id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.elevenlabs.io/v1/models");
            request.Headers.TryAddWithoutValidation("xi-api-key", apiKey);
            return request;
        }

        if (preset.Id == TranscriptionProviderPresetCatalog.Soniox.Id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.soniox.com/v1/models");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            return request;
        }

        if (preset.Id == TranscriptionProviderPresetCatalog.Gemini.Id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://generativelanguage.googleapis.com/v1beta/models");
            request.Headers.TryAddWithoutValidation("x-goog-api-key", apiKey);
            return request;
        }

        if (OpenAICompatibleProbeProviders.Contains(preset.Id))
        {
            var endpoint = preset.Id == TranscriptionProviderPresetCatalog.Custom.Id
                ? settings.CloudTranscriptionEndpoint
                : preset.Endpoint;
            var modelsUri = TryBuildOpenAICompatibleModelsUri(endpoint);
            if (modelsUri is null)
            {
                return null;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, modelsUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            return request;
        }

        return null;
    }

    private static Uri? TryBuildOpenAICompatibleModelsUri(string endpoint)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var transcriptionUri)
            || transcriptionUri.Scheme != Uri.UriSchemeHttps)
        {
            return null;
        }

        var path = transcriptionUri.AbsolutePath;
        var marker = "/audio/transcriptions";
        var markerIndex = path.LastIndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return null;
        }

        var modelsPath = path[..markerIndex].TrimEnd('/') + "/models";
        var builder = new UriBuilder(transcriptionUri)
        {
            Path = modelsPath,
            Query = string.Empty
        };
        return builder.Uri;
    }

    private async Task<string?> ReadApiKeyAsync(string providerId, CancellationToken cancellationToken)
    {
        foreach (var secretName in TranscriptionConfiguration.SecretNamesForCloudProvider(providerId))
        {
            var secret = await secretStore.ReadSecretAsync(secretName, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(secret))
            {
                return secret;
            }
        }

        return null;
    }
}
