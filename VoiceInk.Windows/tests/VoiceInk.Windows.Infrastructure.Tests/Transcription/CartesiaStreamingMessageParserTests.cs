using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class CartesiaStreamingMessageParserTests
{
    [Fact]
    public void Parse_ReturnsTranscriptTextAndFinalFlag()
    {
        var transcript = CartesiaStreamingMessageParser.Parse(
            """{"type":"transcript","is_final":true,"text":"hello cartesia"}""");

        Assert.NotNull(transcript);
        Assert.Equal("hello cartesia", transcript.Text);
        Assert.True(transcript.IsFinal);
    }

    [Fact]
    public void Parse_IgnoresNonTranscriptMessages()
    {
        Assert.Null(CartesiaStreamingMessageParser.Parse("""{"type":"flush_done"}"""));
    }
}
