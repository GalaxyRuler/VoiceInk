using System.Web;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public static class CartesiaStreamingUriFactory
{
    private const string CartesiaVersion = "2026-03-01";

    public static Uri Build(AppSettings settings)
    {
        var model = string.IsNullOrWhiteSpace(settings.CloudTranscriptionModel)
            ? TranscriptionProviderPresetCatalog.Cartesia.DefaultModel
            : settings.CloudTranscriptionModel.Trim();
        var builder = new UriBuilder("wss://api.cartesia.ai/stt/websocket");
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["model"] = model;
        query["encoding"] = "pcm_s16le";
        query["sample_rate"] = "16000";
        query["cartesia_version"] = CartesiaVersion;
        builder.Query = query.ToString();
        return builder.Uri;
    }
}
