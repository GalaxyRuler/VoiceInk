using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class ElevenLabsStreamingMessageParserTests
{
    [Theory]
    [InlineData("""{"message_type":"partial_transcript","text":"hello elev"}""", "hello elev", false)]
    [InlineData("""{"message_type":"committed_transcript","text":"hello eleven"}""", "hello eleven", true)]
    [InlineData("""{"message_type":"committed_transcript_with_timestamps","text":"hello elevenlabs","words":[]}""", "hello elevenlabs", true)]
    public void Parse_ReturnsTranscriptEvents(string json, string expectedText, bool expectedFinal)
    {
        var transcript = ElevenLabsStreamingMessageParser.Parse(json);

        Assert.NotNull(transcript);
        Assert.Equal(expectedText, transcript.Text);
        Assert.Equal(expectedFinal, transcript.IsFinal);
    }

    [Theory]
    [InlineData("""{"message_type":"session_started","session_id":"abc"}""")]
    [InlineData("""{"message_type":"partial_transcript","text":"   "}""")]
    [InlineData("""{"message_type":"error","error":"bad_request"}""")]
    [InlineData("not json")]
    public void Parse_IgnoresNonTranscriptEvents(string json)
    {
        Assert.Null(ElevenLabsStreamingMessageParser.Parse(json));
    }
}
