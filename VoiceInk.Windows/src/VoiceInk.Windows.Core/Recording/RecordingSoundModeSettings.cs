namespace VoiceInk.Windows.Core.Recording;

public static class RecordingSoundModeSettings
{
    public const string SystemDefault = "systemDefault";
    public const string Custom = "custom";

    public static string Normalize(string? mode)
    {
        if (string.Equals(mode?.Trim(), Custom, StringComparison.OrdinalIgnoreCase))
        {
            return Custom;
        }

        return SystemDefault;
    }
}
