using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
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
    private const long InlineAudioLimitBytes = 20 * 1024 * 1024;

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
        message.Content = JsonContent.Create(await CreatePayloadAsync(audio, options, apiKey, cancellationToken)
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

    private async Task<object> CreatePayloadAsync(
        AudioCaptureResult audio,
        TranscriptionOptions options,
        string apiKey,
        CancellationToken cancellationToken)
    {
        var audioFile = new FileInfo(audio.FilePath);
        if (audioFile.Length > InlineAudioLimitBytes)
        {
            var file = await UploadFileAsync(audioFile, apiKey, cancellationToken).ConfigureAwait(false);
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
                                file_data = new
                                {
                                    mime_type = file.MimeType,
                                    file_uri = file.Uri
                                }
                            }
                        }
                    }
                }
            };
        }

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

    private async Task<UploadedGeminiFile> UploadFileAsync(
        FileInfo audioFile,
        string apiKey,
        CancellationToken cancellationToken)
    {
        var uploadUrl = await StartUploadAsync(audioFile, apiKey, cancellationToken).ConfigureAwait(false);
        await using var stream = audioFile.OpenRead();
        using var uploadRequest = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
        uploadRequest.Headers.Add("x-goog-api-key", apiKey);
        uploadRequest.Headers.Add("X-Goog-Upload-Offset", "0");
        uploadRequest.Headers.Add("X-Goog-Upload-Command", "upload, finalize");
        uploadRequest.Content = new StreamContent(stream);
        uploadRequest.Content.Headers.ContentLength = audioFile.Length;
        uploadRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        using var response = await httpClient.SendAsync(uploadRequest, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadUploadedFileAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Uri> StartUploadAsync(
        FileInfo audioFile,
        string apiKey,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://generativelanguage.googleapis.com/upload/v1beta/files");
        request.Headers.Add("x-goog-api-key", apiKey);
        request.Headers.Add("X-Goog-Upload-Protocol", "resumable");
        request.Headers.Add("X-Goog-Upload-Command", "start");
        request.Headers.Add("X-Goog-Upload-Header-Content-Length", audioFile.Length.ToString());
        request.Headers.Add("X-Goog-Upload-Header-Content-Type", "audio/wav");
        request.Content = JsonContent.Create(new
        {
            file = new
            {
                display_name = audioFile.Name
            }
        }, options: JsonOptions);

        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        if (!response.Headers.TryGetValues("X-Goog-Upload-URL", out var uploadUrls)
            || !Uri.TryCreate(uploadUrls.FirstOrDefault(), UriKind.Absolute, out var uploadUrl))
        {
            throw new InvalidOperationException("Gemini transcription provider did not return an upload URL.");
        }

        return uploadUrl;
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

    private static async Task<UploadedGeminiFile> ReadUploadedFileAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (!document.RootElement.TryGetProperty("file", out var file)
                || !file.TryGetProperty("uri", out var uri)
                || uri.ValueKind != JsonValueKind.String
                || string.IsNullOrWhiteSpace(uri.GetString()))
            {
                throw new InvalidOperationException("Gemini transcription provider returned no file URI.");
            }

            var mimeType = "audio/wav";
            if (file.TryGetProperty("mimeType", out var mimeTypeElement)
                && mimeTypeElement.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(mimeTypeElement.GetString()))
            {
                mimeType = mimeTypeElement.GetString()!;
            }

            return new UploadedGeminiFile(uri.GetString()!, mimeType);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Gemini transcription provider returned invalid JSON.", ex);
        }
    }

    private static string SanitizedHttpError(HttpStatusCode statusCode) =>
        $"Gemini transcription provider returned HTTP {(int)statusCode}.";

    private sealed record UploadedGeminiFile(string Uri, string MimeType);
}
