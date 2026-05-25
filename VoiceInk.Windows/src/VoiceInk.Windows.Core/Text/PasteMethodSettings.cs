namespace VoiceInk.Windows.Core.Text;

public static class PasteMethodSettings
{
    public const string Default = "default";
    public const string DirectText = "directText";
    public const double MinimumClipboardRestoreDelaySeconds = 0.25;

    public static string Normalize(string? method) =>
        string.Equals(method, DirectText, StringComparison.OrdinalIgnoreCase)
            ? DirectText
            : Default;

    public static double EffectiveClipboardRestoreDelaySeconds(double configuredSeconds) =>
        Math.Max(configuredSeconds, MinimumClipboardRestoreDelaySeconds);
}
