using System.Text.Json;
using System.Text.Json.Serialization;

namespace VoiceInk.Windows.Core.PowerMode;

public sealed class PowerModeAutoSendKeyJsonConverter : JsonConverter<PowerModeAutoSendKey>
{
    public override PowerModeAutoSendKey Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var numericValue))
        {
            return Enum.IsDefined(typeof(PowerModeAutoSendKey), numericValue)
                ? (PowerModeAutoSendKey)numericValue
                : PowerModeAutoSendKey.None;
        }

        var rawValue = reader.GetString();
        return rawValue switch
        {
            "enter" or "Enter" => PowerModeAutoSendKey.Enter,
            "shiftEnter" or "ShiftEnter" => PowerModeAutoSendKey.ShiftEnter,
            "commandEnter" or "CommandEnter" => PowerModeAutoSendKey.CommandEnter,
            _ => PowerModeAutoSendKey.None
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        PowerModeAutoSendKey value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            PowerModeAutoSendKey.Enter => "enter",
            PowerModeAutoSendKey.ShiftEnter => "shiftEnter",
            PowerModeAutoSendKey.CommandEnter => "commandEnter",
            _ => "none"
        });
    }
}
