using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class DeepgramStreamingUriFactoryTests
{
    [Fact]
    public void Build_IncludesDeepgramModelRawAudioAndInterimQuery()
    {
        var uri = DeepgramStreamingUriFactory.Build(
            new AppSettings
            {
                TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
                CloudTranscriptionProviderId = "deepgram",
                CloudTranscriptionEndpoint = "https://api.deepgram.com/v1/listen",
                CloudTranscriptionModel = "nova-3",
                Language = "en"
            });

        var query = Query(uri);
        Assert.Equal("wss", uri.Scheme);
        Assert.Equal("api.deepgram.com", uri.Host);
        Assert.Equal("/v1/listen", uri.AbsolutePath);
        Assert.Equal("nova-3", query["model"]);
        Assert.Equal("linear16", query["encoding"]);
        Assert.Equal("16000", query["sample_rate"]);
        Assert.Equal("1", query["channels"]);
        Assert.Equal("true", query["interim_results"]);
        Assert.Equal("true", query["smart_format"]);
        Assert.Equal("en", query["language"]);
    }

    [Fact]
    public void Build_OmitsAutomaticLanguage()
    {
        var uri = DeepgramStreamingUriFactory.Build(
            new AppSettings
            {
                TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
                CloudTranscriptionProviderId = "deepgram",
                CloudTranscriptionEndpoint = "https://api.deepgram.com/v1/listen",
                CloudTranscriptionModel = "nova-3",
                Language = "auto"
            });

        Assert.DoesNotContain("language", Query(uri).Keys);
    }

    private static Dictionary<string, string> Query(Uri uri)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            values[Uri.UnescapeDataString(parts[0])] = parts.Length == 2
                ? Uri.UnescapeDataString(parts[1])
                : string.Empty;
        }

        return values;
    }
}
