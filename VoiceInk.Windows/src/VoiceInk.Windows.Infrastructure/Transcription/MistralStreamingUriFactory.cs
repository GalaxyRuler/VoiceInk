using System.Web;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public static class MistralStreamingUriFactory
{
    private const string RealtimePathSuffix = "/v1/audio/transcriptions/realtime";

    public static Uri Build(AppSettings settings)
    {
        var source = Uri.TryCreate(settings.CloudTranscriptionEndpoint?.Trim(), UriKind.Absolute, out var endpoint)
            ? endpoint
            : new Uri(TranscriptionProviderPresetCatalog.Mistral.Endpoint);

        var builder = new UriBuilder(source)
        {
            Scheme = source.Scheme == Uri.UriSchemeHttp ? "ws" : "wss",
            Port = source.IsDefaultPort ? -1 : source.Port,
            Path = RealtimePath(source.AbsolutePath)
        };

        var query = HttpUtility.ParseQueryString(source.Query);
        query["model"] = StreamingModel(settings.CloudTranscriptionModel);
        builder.Query = query.ToString();
        return builder.Uri;
    }

    private static string RealtimePath(string path)
    {
        var trimmed = "/" + path.Trim('/');
        if (trimmed.Equals("/", StringComparison.Ordinal))
        {
            return RealtimePathSuffix;
        }

        if (trimmed.EndsWith("/v1/audio/transcriptions", StringComparison.OrdinalIgnoreCase))
        {
            return $"{trimmed}/realtime";
        }

        if (trimmed.EndsWith("/v1/audio/transcriptions/realtime", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        if (trimmed.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
        {
            return $"{trimmed}/audio/transcriptions/realtime";
        }

        return $"{trimmed.TrimEnd('/')}/audio/transcriptions/realtime";
    }

    private static string StreamingModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model)
            || string.Equals(model.Trim(), TranscriptionProviderPresetCatalog.Mistral.DefaultModel, StringComparison.OrdinalIgnoreCase))
        {
            return "voxtral-mini-transcribe-realtime-2602";
        }

        return model.Trim();
    }
}
