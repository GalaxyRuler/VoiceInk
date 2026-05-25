namespace VoiceInk.Windows.Core.Startup;

public static class StartupLaunchMode
{
    public static bool IsLoginStartup(IEnumerable<string> arguments) =>
        arguments.Any(argument =>
            string.Equals(argument, StartupLaunchCommand.StartupArgument, StringComparison.OrdinalIgnoreCase)
            || string.Equals(argument, "/voiceink-startup", StringComparison.OrdinalIgnoreCase));
}
