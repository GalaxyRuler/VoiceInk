using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public static class AssemblyAIStreamingUriFactory
{
    public static Uri Build(AppSettings settings)
    {
        var endpointText = string.IsNullOrWhiteSpace(settings.CloudTranscriptionEndpoint)
            ? TranscriptionProviderPresetCatalog.AssemblyAI.Endpoint
            : settings.CloudTranscriptionEndpoint;
        if (!TranscriptionConfiguration.TryCreateCloudEndpoint(
            endpointText,
            out var endpoint,
            out var endpointError))
        {
            throw new InvalidOperationException(endpointError);
        }

        var model = string.IsNullOrWhiteSpace(settings.CloudTranscriptionModel)
            ? TranscriptionProviderPresetCatalog.AssemblyAI.DefaultModel
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

        queryParts.Add(QueryParameter("speech_model", StreamingModelId(model)));
        queryParts.Add(QueryParameter("sample_rate", "16000"));
        queryParts.Add(QueryParameter("format_turns", "true"));

        builder.Query = string.Join("&", queryParts);
        return builder.Uri;
    }

    private static string StreamingModelId(string model) =>
        string.Equals(model, "universal-3-pro", StringComparison.OrdinalIgnoreCase)
            ? "u3-rt-pro"
            : model;

    private static string QueryParameter(string name, string value) =>
        $"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}";
}
