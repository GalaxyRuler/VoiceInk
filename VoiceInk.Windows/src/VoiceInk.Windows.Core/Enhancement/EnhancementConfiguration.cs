namespace VoiceInk.Windows.Core.Enhancement;

public static class EnhancementConfiguration
{
    public const string OpenAICompatibleProviderName = "openai-compatible";
    public const string ProviderRequiredMessage = "AI enhancement endpoint and model are required.";
    public const string EndpointInvalidMessage = "AI enhancement endpoint is invalid.";
    public const string EndpointHttpsRequiredMessage =
        "AI enhancement endpoint must use HTTPS unless it targets localhost.";
    public const string EndpointCredentialsRejectedMessage =
        "AI enhancement endpoint must not contain credentials.";
    public const string EndpointQuerySecretRejectedMessage =
        "AI enhancement endpoint must not contain API keys or tokens in the query string.";
    public const string LegacyCustomSecretName = "VoiceInk.Windows.Enhancement.OpenAICompatible.ApiKey";

    private const string SecretNamePrefix = "VoiceInk.Windows.Enhancement.OpenAICompatible";

    public static string? ValidateRequiredSettings(
        bool isEnabled,
        string endpoint,
        string model)
    {
        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            var endpointError = ValidateEndpoint(endpoint);
            if (endpointError is not null)
            {
                return endpointError;
            }
        }

        if (!isEnabled)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(model))
        {
            return ProviderRequiredMessage;
        }

        return null;
    }

    public static string? ValidateEndpoint(string endpoint)
    {
        if (!Uri.TryCreate(endpoint.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            return EndpointInvalidMessage;
        }

        if (!string.IsNullOrWhiteSpace(uri.UserInfo))
        {
            return EndpointCredentialsRejectedMessage;
        }

        if (ContainsSecretQueryParameter(uri))
        {
            return EndpointQuerySecretRejectedMessage;
        }

        if (uri.Scheme != Uri.UriSchemeHttps && !uri.IsLoopback)
        {
            return EndpointHttpsRequiredMessage;
        }

        return null;
    }

    public static bool TryCreateEndpoint(
        string endpoint,
        out Uri? uri,
        out string? errorMessage)
    {
        uri = null;
        errorMessage = ValidateEndpoint(endpoint);
        if (errorMessage is not null)
        {
            return false;
        }

        uri = new Uri(endpoint.Trim(), UriKind.Absolute);
        return true;
    }

    public static string SecretNameForProvider(string? providerId)
    {
        var preset = EnhancementProviderPresetCatalog.Resolve(providerId);
        var segment = preset.Id switch
        {
            "cerebras" => "Cerebras",
            "groq" => "Groq",
            "gemini" => "Gemini",
            "openai" => "OpenAI",
            "openrouter" => "OpenRouter",
            "mistral" => "Mistral",
            "ollama" => "Ollama",
            _ => "Custom"
        };

        return $"{SecretNamePrefix}.{segment}.ApiKey";
    }

    public static IReadOnlyList<string> SecretNamesForProvider(string? providerId)
    {
        var preset = EnhancementProviderPresetCatalog.Resolve(providerId);
        if (!preset.RequiresApiKey)
        {
            return [];
        }

        var primary = SecretNameForProvider(providerId);
        return preset.Id == EnhancementProviderPresetCatalog.Custom.Id
            ? [primary, LegacyCustomSecretName]
            : [primary];
    }

    public static string ProviderNameFor(string? providerId)
    {
        var preset = EnhancementProviderPresetCatalog.Resolve(providerId);
        return preset.Id == EnhancementProviderPresetCatalog.Custom.Id
            ? OpenAICompatibleProviderName
            : preset.Id;
    }

    private static bool ContainsSecretQueryParameter(Uri uri)
    {
        if (string.IsNullOrWhiteSpace(uri.Query))
        {
            return false;
        }

        foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var rawName = pair.Split('=', 2)[0];
            var normalized = Uri.UnescapeDataString(rawName)
                .Trim()
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .Replace(".", string.Empty)
                .ToLowerInvariant();
            if (normalized is "apikey"
                or "apitoken"
                or "accesskey"
                or "accesstoken"
                or "authkey"
                or "authtoken"
                or "authorization"
                or "bearer"
                or "bearertoken"
                or "clientsecret"
                or "credential"
                or "key"
                or "secret"
                or "token")
            {
                return true;
            }
        }

        return false;
    }
}
