namespace VoiceInk.Windows.Core.Startup;

public static class StartupLaunchCommand
{
    public const string StartupArgument = "--voiceink-startup";

    public static string Build(string? executablePath)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return string.Empty;
        }

        return $"\"{executablePath.Trim().Replace("\"", "\\\"", StringComparison.Ordinal)}\" {StartupArgument}";
    }
}
