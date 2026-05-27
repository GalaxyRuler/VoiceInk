using System.Web;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public static class ElevenLabsStreamingUriFactory
{
    public static Uri Build(AppSettings settings)
    {
        var builder = new UriBuilder("wss://api.elevenlabs.io/v1/speech-to-text/realtime");
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["model_id"] = StreamingModel(settings.CloudTranscriptionModel);
        query["audio_format"] = "pcm_16000";
        query["commit_strategy"] = "manual";

        if (!string.IsNullOrWhiteSpace(settings.Language)
            && !string.Equals(settings.Language.Trim(), "auto", StringComparison.OrdinalIgnoreCase))
        {
            query["language_code"] = settings.Language.Trim();
        }

        builder.Query = query.ToString();
        return builder.Uri;
    }

    private static string StreamingModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model)
            || string.Equals(model.Trim(), TranscriptionProviderPresetCatalog.ElevenLabs.DefaultModel, StringComparison.OrdinalIgnoreCase)
            || string.Equals(model.Trim(), "scribe_v1", StringComparison.OrdinalIgnoreCase))
        {
            return "scribe_v2_realtime";
        }

        return model.Trim();
    }
}
