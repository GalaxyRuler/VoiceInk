using System.Text.Json;
using System.Text.Json.Serialization;
using VoiceInk.Windows.Core.Text;

namespace VoiceInk.Windows.Infrastructure.Settings;

internal sealed class PunctuationCleanupModeJsonConverter : JsonConverter<PunctuationCleanupMode>
{
    public override PunctuationCleanupMode Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var numericValue))
        {
            return numericValue switch
            {
                0 => PunctuationCleanupMode.Keep,
                1 => PunctuationCleanupMode.RemoveAll,
                2 => PunctuationCleanupMode.RemoveTrailingPeriod,
                _ => throw new JsonException($"Unknown punctuation cleanup mode value: {numericValue}.")
            };
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Punctuation cleanup mode must be a string or legacy number.");
        }

        return reader.GetString() switch
        {
            "keep" => PunctuationCleanupMode.Keep,
            "removeAll" => PunctuationCleanupMode.RemoveAll,
            "removeTrailingPeriod" => PunctuationCleanupMode.RemoveTrailingPeriod,
            "Keep" => PunctuationCleanupMode.Keep,
            "RemoveAll" => PunctuationCleanupMode.RemoveAll,
            "RemoveTrailingPeriod" => PunctuationCleanupMode.RemoveTrailingPeriod,
            var value => throw new JsonException($"Unknown punctuation cleanup mode value: {value}.")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        PunctuationCleanupMode value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            PunctuationCleanupMode.Keep => "keep",
            PunctuationCleanupMode.RemoveAll => "removeAll",
            PunctuationCleanupMode.RemoveTrailingPeriod => "removeTrailingPeriod",
            _ => "keep"
        });
    }
}
