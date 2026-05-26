using System.Text.Json.Serialization;

namespace VoiceInk.Windows.Core.PowerMode;

[JsonConverter(typeof(PowerModeAutoSendKeyJsonConverter))]
public enum PowerModeAutoSendKey
{
    None,
    Enter,
    ShiftEnter,
    CommandEnter
}
