namespace VoiceInk.Windows.Core.PowerMode;

public sealed record PowerModeInstalledApplicationChoice(
    string DisplayName,
    string ProcessName,
    string Source)
{
    public string DisplayLabel =>
        string.IsNullOrWhiteSpace(ProcessName)
            ? DisplayName.Trim()
            : $"{DisplayName.Trim()} ({ProcessName.Trim()})";
}
