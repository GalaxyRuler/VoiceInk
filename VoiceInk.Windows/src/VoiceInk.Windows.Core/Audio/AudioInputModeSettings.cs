namespace VoiceInk.Windows.Core.Audio;

public static class AudioInputModeSettings
{
    public const string SystemDefault = "systemDefault";
    public const string Custom = "custom";
    public const string Prioritized = "prioritized";

    public static bool IsValid(string? mode) =>
        mode is SystemDefault or Custom or Prioritized;

    public static string Normalize(string? mode) =>
        IsValid(mode) ? mode! : SystemDefault;
}
