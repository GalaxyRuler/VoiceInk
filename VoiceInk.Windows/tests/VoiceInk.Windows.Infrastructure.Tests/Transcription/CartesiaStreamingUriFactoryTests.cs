using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class CartesiaStreamingUriFactoryTests
{
    [Fact]
    public void Build_UsesRealtimeEndpointModelEncodingSampleRateAndVersion()
    {
        var uri = CartesiaStreamingUriFactory.Build(new AppSettings
        {
            CloudTranscriptionProviderId = "cartesia",
            CloudTranscriptionModel = "ink-whisper",
            TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible
        });

        Assert.Equal("wss://api.cartesia.ai/stt/websocket", uri.GetLeftPart(UriPartial.Path));
        Assert.Contains("model=ink-whisper", uri.Query);
        Assert.Contains("encoding=pcm_s16le", uri.Query);
        Assert.Contains("sample_rate=16000", uri.Query);
        Assert.Contains("cartesia_version=2026-03-01", uri.Query);
    }
}
