using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Shell;

public static class ShellStartupVisibility
{
    public static bool ShouldStartHidden(bool isLoginStartupLaunch, AppSettings settings) =>
        isLoginStartupLaunch
        || (settings.StartHiddenToTray && settings.HasCompletedOnboarding);
}
