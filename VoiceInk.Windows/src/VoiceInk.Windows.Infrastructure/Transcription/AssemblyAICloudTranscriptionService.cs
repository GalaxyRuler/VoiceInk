using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed class AssemblyAICloudTranscriptionService(
    HttpClient httpClient,
    ISecretStore secretStore,
    TimeSpan? pollDelay = null,
    int maxPollAttempts = 120) : ITranscriptionService
{
    private static readonly Uri UploadUri = new("https://api.assemblyai.com/v2/upload");
    private static readonly Uri TranscriptUri = new("https://api.assemblyai.com/v2/transcript");
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

        var apiKey = await ReadApiKeyAsync(options.CloudProviderId, cancellationToken)
            .ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("AssemblyAI transcription provider is not configured.");
        }

        var startedAt = Stopwatch.GetTimestamp();
        var uploadUrl = await UploadAudioAsync(audio, apiKey, cancellationToken).ConfigureAwait(false);
        var transcriptId = await SubmitTranscriptAsync(uploadUrl, options, apiKey, cancellationToken)
            .ConfigureAwait(false);
        var text = await PollTranscriptAsync(transcriptId, apiKey, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("AssemblyAI transcription provider returned no text.");
        }

        return new TranscriptionResult(
            text,
            Stopwatch.GetElapsedTime(startedAt),
            TranscriptionConfiguration.OpenAICompatibleProviderNameFor(options.CloudProviderId));
    }

    private async Task<string> UploadAudioAsync(
        AudioCaptureResult audio,
        string apiKey,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, UploadUri);
        message.Headers.Authorization = new AuthenticationHeaderValue(apiKey);
        message.Content = new ByteArrayContent(await File.ReadAllBytesAsync(audio.FilePath, cancellationToken)
            .ConfigureAwait(false));
        message.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

        using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        using var document = await ReadJsonAsync(response, cancellationToken).ConfigureAwait(false);
        if (!document.RootElement.TryGetProperty("upload_url", out var uploadUrl)
            || string.IsNullOrWhiteSpace(uploadUrl.GetString()))
        {
            throw new InvalidOperationException("AssemblyAI transcription provider returned an unexpected response.");
        }

        return uploadUrl.GetString()!;
    }

    private async Task<string> SubmitTranscriptAsync(
        string uploadUrl,
        TranscriptionOptions options,
        string apiKey,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, TranscriptUri);
        message.Headers.Authorization = new AuthenticationHeaderValue(apiKey);

        var payload = new Dictionary<string, object?>
        {
            ["audio_url"] = uploadUrl,
            ["speech_model"] = options.CloudModel.Trim()
        };
        if (!string.IsNullOrWhiteSpace(options.Language)
            && !string.Equals(options.Language.Trim(), "auto", StringComparison.OrdinalIgnoreCase))
        {
            payload["language_code"] = options.Language.Trim();
        }

        if (!string.IsNullOrWhiteSpace(options.Prompt))
        {
            payload["prompt"] = options.Prompt;
        }

        message.Content = new StringContent(
            JsonSerializer.Serialize(payload, JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        using var document = await ReadJsonAsync(response, cancellationToken).ConfigureAwait(false);
        if (!document.RootElement.TryGetProperty("id", out var id)
            || string.IsNullOrWhiteSpace(id.GetString()))
        {
            throw new InvalidOperationException("AssemblyAI transcription provider returned an unexpected response.");
        }

        return id.GetString()!;
    }

    private async Task<string> PollTranscriptAsync(
        string transcriptId,
        string apiKey,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < maxPollAttempts; attempt++)
        {
            using var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri($"https://api.assemblyai.com/v2/transcript/{Uri.EscapeDataString(transcriptId)}"));
            message.Headers.Authorization = new AuthenticationHeaderValue(apiKey);
            using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
            await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
            using var document = await ReadJsonAsync(response, cancellationToken).ConfigureAwait(false);
            var status = document.RootElement.TryGetProperty("status", out var statusElement)
                ? statusElement.GetString()
                : null;

            if (string.Equals(status, "completed", StringComparison.OrdinalIgnoreCase))
            {
                return document.RootElement.TryGetProperty("text", out var text)
                    ? text.GetString() ?? string.Empty
                    : string.Empty;
            }

            if (string.Equals(status, "error", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("AssemblyAI transcription provider failed.");
            }

            if (pollDelay > TimeSpan.Zero)
            {
                await Task.Delay(pollDelay, cancellationToken).ConfigureAwait(false);
            }
        }

        throw new TimeoutException("AssemblyAI transcription provider timed out.");
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
            throw new InvalidOperationException(
                "AssemblyAI transcription provider returned invalid JSON.",
                ex);
        }
    }

    private static string SanitizedHttpError(HttpStatusCode statusCode) =>
        $"AssemblyAI transcription provider returned HTTP {(int)statusCode}.";
}
