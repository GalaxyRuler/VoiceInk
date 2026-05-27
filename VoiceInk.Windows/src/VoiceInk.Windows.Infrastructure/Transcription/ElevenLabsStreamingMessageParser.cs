using System.Text.Json;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed record ElevenLabsStreamingTranscript(string Text, bool IsFinal);

public static class ElevenLabsStreamingMessageParser
{
    public static ElevenLabsStreamingTranscript? Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (!root.TryGetProperty("message_type", out var typeElement)
                || typeElement.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            var messageType = typeElement.GetString();
            var isFinal = messageType is "committed_transcript" or "committed_transcript_with_timestamps";
            if (!isFinal && !string.Equals(messageType, "partial_transcript", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var text = root.TryGetProperty("text", out var textElement)
                && textElement.ValueKind == JsonValueKind.String
                ? textElement.GetString()?.Trim()
                : null;
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return new ElevenLabsStreamingTranscript(text, isFinal);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }
}
