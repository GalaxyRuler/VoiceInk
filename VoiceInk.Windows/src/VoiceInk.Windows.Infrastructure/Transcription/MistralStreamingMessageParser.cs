using System.Text.Json;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed record MistralStreamingTranscript(string Text, bool IsFinal);

public static class MistralStreamingMessageParser
{
    public static MistralStreamingTranscript? Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (!root.TryGetProperty("type", out var typeElement)
                || typeElement.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            var type = typeElement.GetString();
            var isFinal = string.Equals(type, "transcription.done", StringComparison.OrdinalIgnoreCase);
            if (!isFinal
                && !string.Equals(type, "transcription.text.delta", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var text = root.TryGetProperty("text", out var textElement)
                && textElement.ValueKind == JsonValueKind.String
                ? textElement.GetString()
                : null;
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            return new MistralStreamingTranscript(text, isFinal);
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
