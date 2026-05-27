using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class MistralStreamingUriFactoryTests
{
    [Fact]
    public void Build_MapsPresetBatchEndpointToRealtimeWebSocket()
    {
        var uri = MistralStreamingUriFactory.Build(new AppSettings
        {
            CloudTranscriptionEndpoint = "https://api.mistral.ai/v1/audio/transcriptions",
            CloudTranscriptionModel = "voxtral-mini-latest"
        });

        Assert.Equal("wss", uri.Scheme);
        Assert.Equal("api.mistral.ai", uri.Host);
        Assert.Equal("/v1/audio/transcriptions/realtime", uri.AbsolutePath);
        Assert.Contains("model=voxtral-mini-transcribe-realtime-2602", uri.Query);
    }

    [Fact]
    public void Build_PreservesExistingSafeQueryAndExplicitRealtimeModel()
    {
        var uri = MistralStreamingUriFactory.Build(new AppSettings
        {
            CloudTranscriptionEndpoint = "https://example.test/api/v1/audio/transcriptions?region=eu",
            CloudTranscriptionModel = "voxtral-mini-transcribe-realtime-2602"
        });

        Assert.Equal("wss://example.test/api/v1/audio/transcriptions/realtime", uri.GetLeftPart(UriPartial.Path));
        Assert.Contains("region=eu", uri.Query);
        Assert.Contains("model=voxtral-mini-transcribe-realtime-2602", uri.Query);
    }

    [Fact]
    public void Build_PreservesAlreadyRealtimeEndpointPath()
    {
        var uri = MistralStreamingUriFactory.Build(new AppSettings
        {
            CloudTranscriptionEndpoint = "https://api.mistral.ai/v1/audio/transcriptions/realtime",
            CloudTranscriptionModel = "voxtral-mini-latest"
        });

        Assert.Equal("/v1/audio/transcriptions/realtime", uri.AbsolutePath);
    }
}
