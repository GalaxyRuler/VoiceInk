using System.Text;
using System.Text.Json;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed record SonioxStreamingTranscript(string Text, bool IsFinal);

public static class SonioxStreamingMessageParser
{
    public static SonioxStreamingTranscript? Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("tokens", out var tokens)
                || tokens.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var builder = new StringBuilder();
            var allFinal = true;
            foreach (var token in tokens.EnumerateArray())
            {
                if (!token.TryGetProperty("text", out var textElement)
                    || textElement.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var text = textElement.GetString();
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                builder.Append(text);
                if (!token.TryGetProperty("is_final", out var isFinalElement)
                    || isFinalElement.ValueKind != JsonValueKind.True)
                {
                    allFinal = false;
                }
            }

            var transcript = builder.ToString().Trim();
            return transcript.Length == 0
                ? null
                : new SonioxStreamingTranscript(transcript, allFinal);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
