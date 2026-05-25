using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class AssemblyAIStreamingUriFactoryTests
{
    [Fact]
    public void Build_IncludesAssemblyAiEndpointMappedModelAndSampleRate()
    {
        var uri = AssemblyAIStreamingUriFactory.Build(
            new AppSettings
            {
                TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
                CloudTranscriptionProviderId = "assemblyai",
                CloudTranscriptionEndpoint = "https://streaming.assemblyai.com/v3/ws",
                CloudTranscriptionModel = "universal-3-pro",
                Language = "en"
            });

        var query = Query(uri);
        Assert.Equal("wss", uri.Scheme);
        Assert.Equal("streaming.assemblyai.com", uri.Host);
        Assert.Equal("/v3/ws", uri.AbsolutePath);
        Assert.Equal("u3-rt-pro", query["speech_model"]);
        Assert.Equal("16000", query["sample_rate"]);
        Assert.Equal("true", query["format_turns"]);
    }

    [Fact]
    public void Build_OmitsAutomaticLanguage()
    {
        var uri = AssemblyAIStreamingUriFactory.Build(
            new AppSettings
            {
                TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
                CloudTranscriptionProviderId = "assemblyai",
                CloudTranscriptionEndpoint = "https://streaming.assemblyai.com/v3/ws",
                CloudTranscriptionModel = "universal-streaming",
                Language = "auto"
            });

        Assert.DoesNotContain("language_code", Query(uri).Keys);
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
