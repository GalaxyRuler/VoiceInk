using System.Text;
using System.Text.Json;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed record SpeechmaticsStreamingTranscript(string Text, bool IsFinal);

public static class SpeechmaticsStreamingMessageParser
{
    public static SpeechmaticsStreamingTranscript? Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (!root.TryGetProperty("message", out var messageElement)
                || messageElement.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            var message = messageElement.GetString();
            var isFinal = string.Equals(message, "AddTranscript", StringComparison.OrdinalIgnoreCase);
            if (!isFinal && !string.Equals(message, "AddPartialTranscript", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (!root.TryGetProperty("results", out var results)
                || results.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var text = BuildText(results).Trim();
            return string.IsNullOrWhiteSpace(text)
                ? null
                : new SpeechmaticsStreamingTranscript(text, isFinal);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string BuildText(JsonElement results)
    {
        var builder = new StringBuilder();
        foreach (var result in results.EnumerateArray())
        {
            if (!result.TryGetProperty("alternatives", out var alternatives)
                || alternatives.ValueKind != JsonValueKind.Array
                || alternatives.GetArrayLength() == 0)
            {
                continue;
            }

            var content = alternatives[0].TryGetProperty("content", out var contentElement)
                && contentElement.ValueKind == JsonValueKind.String
                    ? contentElement.GetString()
                    : null;
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            var isPunctuation = result.TryGetProperty("type", out var typeElement)
                && string.Equals(typeElement.GetString(), "punctuation", StringComparison.OrdinalIgnoreCase);
            if (builder.Length > 0 && !isPunctuation)
            {
                builder.Append(' ');
            }

            builder.Append(content);
        }

        return builder.ToString();
    }
}
