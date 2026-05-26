namespace VoiceInk.Windows.Core.Shortcuts;

public static class RecordingShortcutModeSettings
{
    public const string Toggle = "toggle";
    public const string PushToTalk = "pushToTalk";
    public const string Hybrid = "hybrid";

    public static string Normalize(string? mode) =>
        mode switch
        {
            PushToTalk => PushToTalk,
            Hybrid => Hybrid,
            _ => Toggle
        };
}
