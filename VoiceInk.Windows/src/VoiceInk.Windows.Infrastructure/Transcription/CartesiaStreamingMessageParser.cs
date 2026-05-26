using System.Text.Json;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed record CartesiaStreamingTranscript(string Text, bool IsFinal);

public static class CartesiaStreamingMessageParser
{
    public static CartesiaStreamingTranscript? Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (!root.TryGetProperty("type", out var type)
                || !string.Equals(type.GetString(), "transcript", StringComparison.OrdinalIgnoreCase))
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

            var isFinal = root.TryGetProperty("is_final", out var finalElement)
                && finalElement.ValueKind == JsonValueKind.True;
            return new CartesiaStreamingTranscript(text, isFinal);
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
