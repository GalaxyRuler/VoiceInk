namespace VoiceInk.Windows.Core.Shell;

public static class TrayIconRecoveryPolicy
{
    public static bool ShouldRecover(int message, int taskbarCreatedMessage) =>
        taskbarCreatedMessage != 0 && message == taskbarCreatedMessage;
}
