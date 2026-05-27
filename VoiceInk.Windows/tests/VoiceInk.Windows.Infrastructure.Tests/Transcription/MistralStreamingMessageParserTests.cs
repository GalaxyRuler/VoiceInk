using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class MistralStreamingMessageParserTests
{
    [Fact]
    public void Parse_ReturnsTextDelta()
    {
        var result = MistralStreamingMessageParser.Parse(
            """{"type":"transcription.text.delta","text":"hello "}""");

        Assert.NotNull(result);
        Assert.Equal("hello ", result.Text);
        Assert.False(result.IsFinal);
    }

    [Fact]
    public void Parse_ReturnsDoneTextAsFinal()
    {
        var result = MistralStreamingMessageParser.Parse(
            """{"type":"transcription.done","text":"hello world","model":"voxtral-mini-transcribe-realtime-2602","usage":{"prompt_tokens":0,"completion_tokens":0,"total_tokens":0},"language":"en"}""");

        Assert.NotNull(result);
        Assert.Equal("hello world", result.Text);
        Assert.True(result.IsFinal);
    }

    [Fact]
    public void Parse_IgnoresIrrelevantOrInvalidMessages()
    {
        Assert.Null(MistralStreamingMessageParser.Parse("""{"type":"session.created"}"""));
        Assert.Null(MistralStreamingMessageParser.Parse("""{"type":"transcription.text.delta","text":""}"""));
        Assert.Null(MistralStreamingMessageParser.Parse("not json"));
    }
}
