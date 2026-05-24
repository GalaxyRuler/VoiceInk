using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.PowerMode;

public sealed record PowerModeResolution(
    PowerModeRule? Rule,
    AppSettings EffectiveSettings,
    PowerModeTarget? Target)
{
    public string? PowerModeName => TrimToNull(Rule?.Name);
    public string? PowerModeEmoji => TrimToNull(Rule?.Emoji);

    private static string? TrimToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
