using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public static class DeepgramStreamingUriFactory
{
    public static Uri Build(AppSettings settings)
    {
        var endpointText = string.IsNullOrWhiteSpace(settings.CloudTranscriptionEndpoint)
            ? TranscriptionProviderPresetCatalog.Deepgram.Endpoint
            : settings.CloudTranscriptionEndpoint;
        if (!TranscriptionConfiguration.TryCreateCloudEndpoint(
            endpointText,
            out var endpoint,
            out var endpointError))
        {
            throw new InvalidOperationException(endpointError);
        }

        var model = string.IsNullOrWhiteSpace(settings.CloudTranscriptionModel)
            ? TranscriptionProviderPresetCatalog.Deepgram.DefaultModel
            : settings.CloudTranscriptionModel.Trim();

        var builder = new UriBuilder(endpoint!)
        {
            Scheme = endpoint!.Scheme == Uri.UriSchemeHttp ? "ws" : "wss"
        };
        if (endpoint.IsDefaultPort)
        {
            builder.Port = -1;
        }

        var queryParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(endpoint.Query))
        {
            queryParts.Add(endpoint.Query.TrimStart('?'));
        }

        queryParts.Add(QueryParameter("model", model));
        queryParts.Add(QueryParameter("encoding", "linear16"));
        queryParts.Add(QueryParameter("sample_rate", "16000"));
        queryParts.Add(QueryParameter("channels", "1"));
        queryParts.Add(QueryParameter("interim_results", "true"));
        queryParts.Add(QueryParameter("smart_format", "true"));
        if (!string.IsNullOrWhiteSpace(settings.Language)
            && !string.Equals(settings.Language.Trim(), "auto", StringComparison.OrdinalIgnoreCase))
        {
            queryParts.Add(QueryParameter("language", settings.Language.Trim()));
        }

        builder.Query = string.Join("&", queryParts);
        return builder.Uri;
    }

    private static string QueryParameter(string name, string value) =>
        $"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}";
}
