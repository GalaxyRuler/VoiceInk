namespace VoiceInk.Windows.Core.Audio;

public sealed record AudioInputDeviceChoice(int? DeviceNumber, string Name, int Channels)
{
    public string DisplayText => DeviceNumber is null
        ? "System Default"
        : $"{Name} ({DeviceNumber})";

    public override string ToString() => DisplayText;
}
