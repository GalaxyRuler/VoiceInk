using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class AssemblyAIStreamingMessageParserTests
{
    [Fact]
    public void Parse_TurnTranscriptReturnsPartial()
    {
        var result = AssemblyAIStreamingMessageParser.Parse(
            """{"type":"Turn","transcript":"hello liv","end_of_turn":false}""");

        Assert.NotNull(result);
        Assert.Equal("hello liv", result.Text);
        Assert.False(result.IsFinal);
    }

    [Fact]
    public void Parse_EndOfTurnTranscriptReturnsFinal()
    {
        var result = AssemblyAIStreamingMessageParser.Parse(
            """{"type":"Turn","transcript":"hello live","end_of_turn":true}""");

        Assert.NotNull(result);
        Assert.Equal("hello live", result.Text);
        Assert.True(result.IsFinal);
    }

    [Theory]
    [InlineData("""{"message_type":"PartialTranscript","text":"draft text"}""", "draft text", false)]
    [InlineData("""{"message_type":"FinalTranscript","text":"final text"}""", "final text", true)]
    public void Parse_LegacyTranscriptShapesReturnTranscript(string json, string expected, bool isFinal)
    {
        var result = AssemblyAIStreamingMessageParser.Parse(json);

        Assert.NotNull(result);
        Assert.Equal(expected, result.Text);
        Assert.Equal(isFinal, result.IsFinal);
    }

    [Theory]
    [InlineData("""{"type":"Begin"}""")]
    [InlineData("""{"type":"Turn","transcript":""}""")]
    [InlineData("""{"unexpected":"shape"}""")]
    [InlineData("""not json""")]
    public void Parse_UnexpectedMessagesReturnNull(string json)
    {
        Assert.Null(AssemblyAIStreamingMessageParser.Parse(json));
    }
}
