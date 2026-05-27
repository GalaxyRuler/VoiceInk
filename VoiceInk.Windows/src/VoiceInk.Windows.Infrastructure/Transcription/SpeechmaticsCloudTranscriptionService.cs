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

public sealed class SpeechmaticsCloudTranscriptionService(
    HttpClient httpClient,
    ISecretStore secretStore,
    TimeSpan? pollDelay = null,
    int maxPollAttempts = 120) : ITranscriptionService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly TimeSpan pollDelay = pollDelay ?? TimeSpan.FromSeconds(1);

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
            throw new InvalidOperationException("Speechmatics transcription provider is not configured.");
        }

        var jobsUri = EndpointWithoutQuery(endpoint!);
        string? jobId = null;
        var startedAt = Stopwatch.GetTimestamp();
        try
        {
            jobId = await CreateJobAsync(jobsUri, audio, options, endpoint!, apiKey, cancellationToken)
                .ConfigureAwait(false);
            await WaitForCompletionAsync(jobsUri, jobId, apiKey, cancellationToken)
                .ConfigureAwait(false);
            var text = await GetTranscriptTextAsync(jobsUri, jobId, apiKey, cancellationToken)
                .ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException("Speechmatics transcription provider returned no text.");
            }

            return new TranscriptionResult(
                text.Trim(),
                Stopwatch.GetElapsedTime(startedAt),
                TranscriptionConfiguration.OpenAICompatibleProviderNameFor(options.CloudProviderId));
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(jobId))
            {
                await DeleteNoThrowAsync(jobsUri, jobId, apiKey, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }

    private async Task<string> CreateJobAsync(
        Uri jobsUri,
        AudioCaptureResult audio,
        TranscriptionOptions options,
        Uri endpoint,
        string apiKey,
        CancellationToken cancellationToken)
    {
        using var message = CreateRequest(HttpMethod.Post, jobsUri, apiKey);
        var content = new MultipartFormDataContent();
        var configJson = JsonSerializer.Serialize(CreateJobConfig(options, endpoint), JsonOptions);
        var configContent = new StringContent(configJson, Encoding.UTF8, "application/json");
        content.Add(configContent, "config");
        var fileStream = File.OpenRead(audio.FilePath);
        var audioContent = new StreamContent(fileStream);
        audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        content.Add(audioContent, "data_file", Path.GetFileName(audio.FilePath));
        message.Content = content;

        using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        using var document = await ReadJsonAsync(response, cancellationToken).ConfigureAwait(false);
        return RequiredJobId(document.RootElement);
    }

    private static object CreateJobConfig(TranscriptionOptions options, Uri endpoint)
    {
        var language = string.IsNullOrWhiteSpace(options.Language)
            ? "auto"
            : options.Language.Trim();
        var transcriptionConfig = new Dictionary<string, object?>
        {
            ["language"] = language
        };
        if (string.Equals(options.CloudModel, "speechmatics-enhanced", StringComparison.OrdinalIgnoreCase))
        {
            transcriptionConfig["operating_point"] = "enhanced";
        }

        foreach (var option in EndpointQueryOptions(endpoint))
        {
            transcriptionConfig.TryAdd(option.Name, option.Value);
        }

        return new Dictionary<string, object?>
        {
            ["type"] = "transcription",
            ["transcription_config"] = transcriptionConfig
        };
    }

    private async Task WaitForCompletionAsync(
        Uri jobsUri,
        string jobId,
        string apiKey,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < maxPollAttempts; attempt++)
        {
            using var message = CreateRequest(HttpMethod.Get, JobUri(jobsUri, jobId), apiKey);
            using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
            await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
            using var document = await ReadJsonAsync(response, cancellationToken).ConfigureAwait(false);
            var status = JobStatus(document.RootElement);
            if (string.Equals(status, "done", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "completed", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.Equals(status, "rejected", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Speechmatics transcription provider failed.");
            }

            if (pollDelay > TimeSpan.Zero)
            {
                await Task.Delay(pollDelay, cancellationToken).ConfigureAwait(false);
            }
        }

        throw new TimeoutException("Speechmatics transcription provider timed out.");
    }

    private async Task<string> GetTranscriptTextAsync(
        Uri jobsUri,
        string jobId,
        string apiKey,
        CancellationToken cancellationToken)
    {
        using var message = CreateRequest(HttpMethod.Get, new Uri($"{JobUri(jobsUri, jobId)}/transcript"), apiKey);
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
        using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task DeleteNoThrowAsync(
        Uri jobsUri,
        string jobId,
        string apiKey,
        CancellationToken cancellationToken)
    {
        try
        {
            using var message = CreateRequest(HttpMethod.Delete, JobUri(jobsUri, jobId), apiKey);
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

    private static Uri EndpointWithoutQuery(Uri endpoint)
    {
        var builder = new UriBuilder(endpoint)
        {
            Query = string.Empty
        };

        return builder.Uri;
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
                || ReservedTranscriptionConfigNames.Any(
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

    private static readonly string[] ReservedTranscriptionConfigNames =
    [
        "language",
        "operating_point"
    ];

    private static Uri JobUri(Uri jobsUri, string jobId)
    {
        var escapedJobId = Uri.EscapeDataString(jobId);
        var baseText = jobsUri.ToString().TrimEnd('/');
        return new Uri($"{baseText}/{escapedJobId}");
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, Uri uri, string apiKey)
    {
        var request = new HttpRequestMessage(method, uri);
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
            throw new InvalidOperationException("Speechmatics transcription provider returned invalid JSON.", ex);
        }
    }

    private static string RequiredJobId(JsonElement element)
    {
        if (TryReadString(element, "id", out var rootId))
        {
            return rootId;
        }

        if (element.TryGetProperty("job", out var job)
            && TryReadString(job, "id", out var jobId))
        {
            return jobId;
        }

        throw new InvalidOperationException("Speechmatics transcription provider returned an unexpected response.");
    }

    private static string? JobStatus(JsonElement element)
    {
        if (TryReadString(element, "status", out var rootStatus))
        {
            return rootStatus;
        }

        return element.TryGetProperty("job", out var job)
            && TryReadString(job, "status", out var jobStatus)
            ? jobStatus
            : null;
    }

    private static bool TryReadString(JsonElement element, string propertyName, out string value)
    {
        if (element.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(property.GetString()))
        {
            value = property.GetString()!;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static string SanitizedHttpError(HttpStatusCode statusCode) =>
        $"Speechmatics transcription provider returned HTTP {(int)statusCode}.";
}
