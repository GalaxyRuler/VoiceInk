using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Infrastructure.Enhancement;

public sealed class OpenAICompatibleTextEnhancementService(
    HttpClient httpClient,
    ISecretStore secretStore,
    ILocalCliProcessRunner? localCliProcessRunner = null) : ITextEnhancementService
{
    public const string SecretName = EnhancementConfiguration.LegacyCustomSecretName;
    private readonly ILocalCliProcessRunner localCliProcessRunner = localCliProcessRunner ?? new LocalCliProcessRunner();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<TextEnhancementResult> EnhanceAsync(
        TextEnhancementRequest request,
        CancellationToken cancellationToken)
    {
        var provider = EnhancementProviderPresetCatalog.Resolve(request.ProviderId);
        var providerName = EnhancementConfiguration.ProviderNameFor(provider.Id);
        if (provider.Id == EnhancementProviderPresetCatalog.LocalCli.Id)
        {
            return await RunLocalCliAsync(request, providerName, cancellationToken);
        }

        if (!EnhancementConfiguration.TryCreateEndpoint(
            request.Endpoint,
            out var endpoint,
            out var endpointError))
        {
            throw new InvalidOperationException(endpointError);
        }

        if (string.IsNullOrWhiteSpace(request.Model))
        {
            throw new InvalidOperationException("AI enhancement model is required.");
        }

        var apiKey = provider.RequiresApiKey
            ? await ReadApiKeyAsync(provider.Id, cancellationToken)
            : null;
        if (provider.RequiresApiKey && string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("AI enhancement provider is not configured.");
        }

        var attempts = Math.Max(1, request.MaxRetries);
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                return await SendOnceAsync(endpoint!, apiKey, providerName, request, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TimeoutException) when (request.RetryOnTimeout && attempt < attempts)
            {
                await DelayBeforeRetryAsync(attempt, cancellationToken);
            }
            catch (TimeoutException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
            catch (TransientEnhancementException) when (attempt < attempts)
            {
                await DelayBeforeRetryAsync(attempt, cancellationToken);
            }
            catch (TransientEnhancementException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        throw new InvalidOperationException("AI enhancement failed after retrying transient provider errors.");
    }

    private async Task<TextEnhancementResult> RunLocalCliAsync(
        TextEnhancementRequest request,
        string providerName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Endpoint))
        {
            throw new InvalidOperationException("Local CLI command is not configured.");
        }

        var fullPrompt = MakeLocalCliFullPrompt(request.SystemMessage, request.UserMessage);
        var startedAt = Stopwatch.GetTimestamp();
        var result = await localCliProcessRunner.RunAsync(
            new LocalCliProcessRequest(
                request.Endpoint.Trim(),
                request.SystemMessage,
                request.UserMessage,
                fullPrompt,
                request.Timeout),
            cancellationToken);
        if (result.TimedOut)
        {
            throw new InvalidOperationException(
                $"Local CLI command timed out after {Math.Ceiling(request.Timeout.TotalSeconds):0} seconds.");
        }

        var stdout = result.Stdout.Trim();
        var stderr = result.Stderr.Trim();
        if (result.ExitCode != 0)
        {
            if (result.ExitCode == 127 || stderr.Contains("not recognized", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Local CLI command was not found. Details: {(stderr.Length == 0 ? request.Endpoint.Trim() : stderr)}");
            }

            throw new InvalidOperationException(
                stderr.Length == 0
                    ? $"Local CLI command failed with exit code {result.ExitCode}."
                    : $"Local CLI command failed with exit code {result.ExitCode}: {stderr}");
        }

        if (stdout.Length == 0)
        {
            throw new InvalidOperationException("Local CLI command returned empty output.");
        }

        return new TextEnhancementResult(
            stdout,
            providerName,
            string.IsNullOrWhiteSpace(request.Model) ? "local-cli" : request.Model,
            Stopwatch.GetElapsedTime(startedAt));
    }

    private static string MakeLocalCliFullPrompt(string systemPrompt, string userPrompt) =>
        $"""
        <SYSTEM_PROMPT>
        {systemPrompt}
        </SYSTEM_PROMPT>

        <USER_PROMPT>
        {userPrompt}
        </USER_PROMPT>
        """;

    private async Task<TextEnhancementResult> SendOnceAsync(
        Uri endpoint,
        string? apiKey,
        string providerName,
        TextEnhancementRequest request,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(request.Timeout);

        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint);
        var isAnthropic = string.Equals(
            request.ProviderId,
            EnhancementProviderPresetCatalog.Anthropic.Id,
            StringComparison.OrdinalIgnoreCase);
        var isOllama = string.Equals(
            request.ProviderId,
            EnhancementProviderPresetCatalog.Ollama.Id,
            StringComparison.OrdinalIgnoreCase);
        if (isAnthropic && !string.IsNullOrWhiteSpace(apiKey))
        {
            message.Headers.Add("x-api-key", apiKey);
            message.Headers.Add("anthropic-version", "2023-06-01");
        }
        else if (!string.IsNullOrWhiteSpace(apiKey))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        message.Content = new StringContent(
            isAnthropic
                ? JsonSerializer.Serialize(
                    new AnthropicMessagesRequest(
                        request.Model,
                        request.SystemMessage,
                        [new AnthropicMessage("user", request.UserMessage)],
                        MaxTokens: 1024,
                        request.Temperature),
                    JsonOptions)
                : isOllama
                    ? JsonSerializer.Serialize(
                        new OllamaChatRequest(
                            request.Model,
                            [
                                new ChatMessage("system", request.SystemMessage),
                                new ChatMessage("user", request.UserMessage)
                            ],
                            Stream: false,
                            new OllamaOptions(request.Temperature)),
                        JsonOptions)
                : JsonSerializer.Serialize(
                    new ChatCompletionsRequest(
                        request.Model,
                        [
                            new ChatMessage("system", request.SystemMessage),
                            new ChatMessage("user", request.UserMessage)
                        ],
                        request.Temperature),
                    JsonOptions),
            Encoding.UTF8,
            "application/json");

        var startedAt = Stopwatch.GetTimestamp();
        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(message, timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("AI enhancement request timed out.");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var sanitized = SanitizedHttpError(response.StatusCode);
                if (IsTransientStatus(response.StatusCode))
                {
                    throw new TransientEnhancementException(sanitized);
                }

                throw new InvalidOperationException(sanitized);
            }

            string json;
            try
            {
                json = await response.Content.ReadAsStringAsync(timeout.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException("AI enhancement request timed out.");
            }

            var content = isAnthropic
                ? ExtractAnthropicMessageContent(json)
                : isOllama
                    ? ExtractOllamaMessageContent(json)
                : ExtractMessageContent(json);
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("AI enhancement provider returned no text.");
            }

            return new TextEnhancementResult(
                content,
                providerName,
                request.Model,
                Stopwatch.GetElapsedTime(startedAt));
        }
    }

    private static string ExtractOllamaMessageContent(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
                ?? string.Empty;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("AI enhancement provider returned invalid JSON.", ex);
        }
        catch (KeyNotFoundException ex)
        {
            throw new InvalidOperationException("AI enhancement provider returned an unexpected response.", ex);
        }
    }

    private async Task<string?> ReadApiKeyAsync(
        string providerId,
        CancellationToken cancellationToken)
    {
        foreach (var secretName in EnhancementConfiguration.SecretNamesForProvider(providerId))
        {
            var apiKey = await secretStore.ReadSecretAsync(secretName, cancellationToken);
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                return apiKey;
            }
        }

        return null;
    }

    private static string ExtractMessageContent(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var choices = document.RootElement.GetProperty("choices");
            if (choices.GetArrayLength() == 0)
            {
                return string.Empty;
            }

            return choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
                ?? string.Empty;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("AI enhancement provider returned invalid JSON.", ex);
        }
        catch (KeyNotFoundException ex)
        {
            throw new InvalidOperationException("AI enhancement provider returned an unexpected response.", ex);
        }
    }

    private static string ExtractAnthropicMessageContent(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var content = document.RootElement.GetProperty("content");
            var textBlocks = content.EnumerateArray()
                .Where(block => block.TryGetProperty("type", out var type)
                    && string.Equals(type.GetString(), "text", StringComparison.OrdinalIgnoreCase)
                    && block.TryGetProperty("text", out _))
                .Select(block => block.GetProperty("text").GetString())
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToArray();

            return string.Join(Environment.NewLine, textBlocks);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("AI enhancement provider returned invalid JSON.", ex);
        }
        catch (KeyNotFoundException ex)
        {
            throw new InvalidOperationException("AI enhancement provider returned an unexpected response.", ex);
        }
    }

    private static string SanitizedHttpError(HttpStatusCode statusCode) =>
        $"AI enhancement provider returned HTTP {(int)statusCode}.";

    private static bool IsTransientStatus(HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.TooManyRequests || (int)statusCode is >= 500 and <= 599;

    private static Task DelayBeforeRetryAsync(int attempt, CancellationToken cancellationToken) =>
        Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), cancellationToken);

    private sealed record ChatCompletionsRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyList<ChatMessage> Messages,
        [property: JsonPropertyName("temperature")] double Temperature);

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record OllamaChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyList<ChatMessage> Messages,
        [property: JsonPropertyName("stream")] bool Stream,
        [property: JsonPropertyName("options")] OllamaOptions Options);

    private sealed record OllamaOptions(
        [property: JsonPropertyName("temperature")] double Temperature);

    private sealed record AnthropicMessagesRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("system")] string System,
        [property: JsonPropertyName("messages")] IReadOnlyList<AnthropicMessage> Messages,
        [property: JsonPropertyName("max_tokens")] int MaxTokens,
        [property: JsonPropertyName("temperature")] double Temperature);

    private sealed record AnthropicMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed class TransientEnhancementException(string message) : Exception(message);
}

public interface ILocalCliProcessRunner
{
    Task<LocalCliProcessResult> RunAsync(
        LocalCliProcessRequest request,
        CancellationToken cancellationToken);
}

public sealed record LocalCliProcessRequest(
    string CommandTemplate,
    string SystemPrompt,
    string UserPrompt,
    string FullPrompt,
    TimeSpan Timeout);

public sealed record LocalCliProcessResult(
    int ExitCode,
    string Stdout,
    string Stderr,
    bool TimedOut);

public sealed class LocalCliProcessRunner : ILocalCliProcessRunner
{
    public async Task<LocalCliProcessResult> RunAsync(
        LocalCliProcessRequest request,
        CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo("cmd.exe")
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        process.StartInfo.ArgumentList.Add("/S");
        process.StartInfo.ArgumentList.Add("/C");
        process.StartInfo.ArgumentList.Add(request.CommandTemplate);
        process.StartInfo.Environment["VOICEINK_SYSTEM_PROMPT"] = request.SystemPrompt;
        process.StartInfo.Environment["VOICEINK_USER_PROMPT"] = request.UserPrompt;
        process.StartInfo.Environment["VOICEINK_FULL_PROMPT"] = request.FullPrompt;

        try
        {
            process.Start();
        }
        catch (Win32Exception ex)
        {
            return new LocalCliProcessResult(127, string.Empty, ex.Message, TimedOut: false);
        }

        await process.StandardInput.WriteAsync(request.FullPrompt.AsMemory(), cancellationToken);
        process.StandardInput.Close();

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        var waitTask = process.WaitForExitAsync(cancellationToken);
        var timeoutTask = Task.Delay(request.Timeout, cancellationToken);
        if (await Task.WhenAny(waitTask, timeoutTask) == timeoutTask)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TryKill(process);
            return new LocalCliProcessResult(0, string.Empty, string.Empty, TimedOut: true);
        }

        await waitTask;
        return new LocalCliProcessResult(
            process.ExitCode,
            await stdoutTask,
            await stderrTask,
            TimedOut: false);
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
        }
    }
}
