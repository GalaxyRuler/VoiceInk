namespace VoiceInk.Windows.Core.Recorder;

public static class RecorderStyleSettings
{
    public const string Mini = "mini";
    public const string Notch = "notch";

    public static string Normalize(string? style) =>
        string.Equals(style?.Trim(), Notch, StringComparison.OrdinalIgnoreCase)
            ? Notch
            : Mini;
}
