using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class DeepgramStreamingMessageParserTests
{
    [Fact]
    public void Parse_InterimResultReturnsTranscriptAndFinalFlag()
    {
        var result = DeepgramStreamingMessageParser.Parse(
            """{"type":"Results","is_final":false,"channel":{"alternatives":[{"transcript":"hello wor"}]}}""");

        Assert.NotNull(result);
        Assert.Equal("hello wor", result.Text);
        Assert.False(result.IsFinal);
    }

    [Fact]
    public void Parse_FinalResultReturnsTranscriptAndFinalFlag()
    {
        var result = DeepgramStreamingMessageParser.Parse(
            """{"type":"Results","is_final":true,"channel":{"alternatives":[{"transcript":"hello world"}]}}""");

        Assert.NotNull(result);
        Assert.Equal("hello world", result.Text);
        Assert.True(result.IsFinal);
    }

    [Fact]
    public void Parse_EmptyTranscriptReturnsNull()
    {
        var result = DeepgramStreamingMessageParser.Parse(
            """{"type":"Results","is_final":false,"channel":{"alternatives":[{"transcript":""}]}}""");

        Assert.Null(result);
    }

    [Theory]
    [InlineData("""{"type":"KeepAlive"}""")]
    [InlineData("""{"unexpected":"shape"}""")]
    [InlineData("""not json""")]
    public void Parse_UnexpectedMessagesReturnNull(string json)
    {
        Assert.Null(DeepgramStreamingMessageParser.Parse(json));
    }
}
