using System.Text.Json;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed record DeepgramStreamingTranscript(string Text, bool IsFinal);

public static class DeepgramStreamingMessageParser
{
    public static DeepgramStreamingTranscript? Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.TryGetProperty("type", out var type)
                && !string.Equals(type.GetString(), "Results", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (!root.TryGetProperty("channel", out var channel)
                || !channel.TryGetProperty("alternatives", out var alternatives)
                || alternatives.ValueKind != JsonValueKind.Array
                || alternatives.GetArrayLength() == 0)
            {
                return null;
            }

            var transcript = alternatives[0].TryGetProperty("transcript", out var transcriptElement)
                ? transcriptElement.GetString()?.Trim()
                : null;
            if (string.IsNullOrWhiteSpace(transcript))
            {
                return null;
            }

            var isFinal = root.TryGetProperty("is_final", out var finalElement)
                && finalElement.ValueKind == JsonValueKind.True;
            return new DeepgramStreamingTranscript(transcript, isFinal);
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
