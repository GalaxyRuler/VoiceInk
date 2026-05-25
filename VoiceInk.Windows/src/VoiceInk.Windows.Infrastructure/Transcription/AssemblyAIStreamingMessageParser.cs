using System.Text.Json;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed record AssemblyAIStreamingTranscript(string Text, bool IsFinal);

public static class AssemblyAIStreamingMessageParser
{
    public static AssemblyAIStreamingTranscript? Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var transcript = TryGetString(root, "transcript")
                ?? TryGetString(root, "text");
            if (string.IsNullOrWhiteSpace(transcript))
            {
                return null;
            }

            var isFinal = TryGetBoolean(root, "end_of_turn") == true
                || IsMessageType(root, "FinalTranscript")
                || IsMessageType(root, "Final");
            return new AssemblyAIStreamingTranscript(transcript.Trim(), isFinal);
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

    private static string? TryGetString(JsonElement root, string propertyName) =>
        root.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static bool? TryGetBoolean(JsonElement root, string propertyName) =>
        root.TryGetProperty(propertyName, out var value)
            ? value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => null
            }
            : null;

    private static bool IsMessageType(JsonElement root, string expected)
    {
        var type = TryGetString(root, "message_type")
            ?? TryGetString(root, "type");
        return string.Equals(type, expected, StringComparison.OrdinalIgnoreCase);
    }
}
