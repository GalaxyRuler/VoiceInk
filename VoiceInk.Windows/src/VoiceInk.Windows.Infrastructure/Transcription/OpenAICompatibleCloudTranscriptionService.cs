using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed class OpenAICompatibleCloudTranscriptionService(
    HttpClient httpClient,
    ISecretStore secretStore) : ITranscriptionService
{
    public const string SecretName = "VoiceInk.Windows.Transcription.OpenAICompatible.ApiKey";

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

        var apiKey = await secretStore.ReadSecretAsync(SecretName, cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Cloud transcription provider is not configured.");
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        message.Content = CreateMultipartContent(audio, options);

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
            throw new InvalidOperationException("Cloud transcription provider returned no text.");
        }

        return new TranscriptionResult(
            text,
            Stopwatch.GetElapsedTime(startedAt),
            TranscriptionConfiguration.OpenAICompatibleProviderName);
    }

    private static MultipartFormDataContent CreateMultipartContent(
        AudioCaptureResult audio,
        TranscriptionOptions options)
    {
        var content = new MultipartFormDataContent();
        var fileStream = File.OpenRead(audio.FilePath);
        var audioContent = new StreamContent(fileStream);
        audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        content.Add(audioContent, "file", Path.GetFileName(audio.FilePath));
        content.Add(new StringContent(options.CloudModel.Trim()), "model");
        content.Add(new StringContent("json"), "response_format");

        if (!string.IsNullOrWhiteSpace(options.Language)
            && !string.Equals(options.Language.Trim(), "auto", StringComparison.OrdinalIgnoreCase))
        {
            content.Add(new StringContent(options.Language.Trim()), "language");
        }

        if (!string.IsNullOrWhiteSpace(options.Prompt))
        {
            content.Add(new StringContent(options.Prompt), "prompt");
        }

        return content;
    }

    private static string ExtractText(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.GetProperty("text").GetString() ?? string.Empty;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Cloud transcription provider returned invalid JSON.", ex);
        }
        catch (KeyNotFoundException ex)
        {
            throw new InvalidOperationException("Cloud transcription provider returned an unexpected response.", ex);
        }
    }

    private static string SanitizedHttpError(HttpStatusCode statusCode) =>
        $"Cloud transcription provider returned HTTP {(int)statusCode}.";
}
