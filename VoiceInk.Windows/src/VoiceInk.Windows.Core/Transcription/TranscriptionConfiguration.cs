using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Models;

namespace VoiceInk.Windows.Core.Transcription;

public static class TranscriptionConfiguration
{
    public const string LocalWhisperProviderName = "local-whisper";
    public const string OpenAICompatibleProviderName = "openai-compatible";
    public const string LocalModelPathRequiredMessage = "Local whisper model path is required.";
    public const string CloudProviderRequiredMessage = "Cloud transcription endpoint and model are required.";
    public const string CloudEndpointInvalidMessage = "Cloud transcription endpoint is invalid.";
    public const string CloudEndpointHttpsRequiredMessage =
        "Cloud transcription endpoint must use HTTPS unless it targets localhost.";
    public const string CloudEndpointCredentialsRejectedMessage =
        "Cloud transcription endpoint must not contain credentials.";
    public const string CloudEndpointQuerySecretRejectedMessage =
        "Cloud transcription endpoint must not contain API keys or tokens in the query string.";
    private const string CloudSecretNamePrefix = "VoiceInk.Windows.Transcription.OpenAICompatible";
    private const string LegacyCustomSecretName = "VoiceInk.Windows.Transcription.OpenAICompatible.ApiKey";

    public static string? ValidateRequiredSettings(AppSettings settings) =>
        settings.TranscriptionProvider switch
        {
            TranscriptionProviderKind.LocalWhisper when string.IsNullOrWhiteSpace(settings.ModelPath) =>
                LocalModelPathRequiredMessage,
            TranscriptionProviderKind.OpenAICompatible
                when string.IsNullOrWhiteSpace(settings.CloudTranscriptionEndpoint)
                    || string.IsNullOrWhiteSpace(settings.CloudTranscriptionModel) =>
                CloudProviderRequiredMessage,
            TranscriptionProviderKind.OpenAICompatible =>
                ValidateCloudEndpoint(settings.CloudTranscriptionEndpoint),
            _ => null
        };

    public static bool TryCreateCloudEndpoint(
        string endpoint,
        out Uri? uri,
        out string? errorMessage)
    {
        uri = null;
        errorMessage = ValidateCloudEndpoint(endpoint);
        if (errorMessage is not null)
        {
            return false;
        }

        uri = new Uri(endpoint.Trim(), UriKind.Absolute);
        return true;
    }

    public static TranscriptionOptions BuildOptions(AppSettings settings, string prompt) =>
        new(
            settings.ModelPath,
            LanguageForProvider(settings),
            prompt,
            settings.TranscriptionProvider,
            settings.CloudTranscriptionEndpoint,
            settings.CloudTranscriptionModel,
            TranscriptionProviderPresetCatalog.Resolve(settings.CloudTranscriptionProviderId).Id);

    public static string SecretNameForCloudProvider(string? providerId)
    {
        var preset = TranscriptionProviderPresetCatalog.Resolve(providerId);
        var segment = preset.Id switch
        {
            "groq" => "Groq",
            "deepgram" => "Deepgram",
            _ => "Custom"
        };

        return $"{CloudSecretNamePrefix}.{segment}.ApiKey";
    }

    public static IReadOnlyList<string> SecretNamesForCloudProvider(string? providerId)
    {
        var primary = SecretNameForCloudProvider(providerId);
        return TranscriptionProviderPresetCatalog.Resolve(providerId).Id == TranscriptionProviderPresetCatalog.Custom.Id
            ? [primary, LegacyCustomSecretName]
            : [primary];
    }

    public static string ProviderName(AppSettings settings) =>
        settings.TranscriptionProvider switch
        {
            TranscriptionProviderKind.LocalWhisper => LocalWhisperProviderName,
            TranscriptionProviderKind.OpenAICompatible => OpenAICompatibleProviderNameFor(
                settings.CloudTranscriptionProviderId),
            _ => settings.TranscriptionProvider.ToString()
        };

    public static string ModelMetadata(AppSettings settings) =>
        settings.TranscriptionProvider switch
        {
            TranscriptionProviderKind.OpenAICompatible => settings.CloudTranscriptionModel,
            _ => settings.ModelPath
        };

    public static string OpenAICompatibleProviderNameFor(string? providerId)
    {
        var preset = TranscriptionProviderPresetCatalog.Resolve(providerId);
        return preset.Id == TranscriptionProviderPresetCatalog.Custom.Id
            ? OpenAICompatibleProviderName
            : preset.Id;
    }

    private static string? ValidateCloudEndpoint(string endpoint)
    {
        if (!Uri.TryCreate(endpoint.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            return CloudEndpointInvalidMessage;
        }

        if (!string.IsNullOrWhiteSpace(uri.UserInfo))
        {
            return CloudEndpointCredentialsRejectedMessage;
        }

        if (ContainsSecretQueryParameter(uri))
        {
            return CloudEndpointQuerySecretRejectedMessage;
        }

        if (uri.Scheme != Uri.UriSchemeHttps && !uri.IsLoopback)
        {
            return CloudEndpointHttpsRequiredMessage;
        }

        return null;
    }

    private static string LanguageForProvider(AppSettings settings) =>
        settings.TranscriptionProvider == TranscriptionProviderKind.LocalWhisper
            ? WhisperLanguageCatalog.CompatibleLanguageOrFallback(
                settings.ModelPath,
                settings.ImportedWhisperModels,
                settings.Language)
            : settings.Language;

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
