using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Audio;

public static class AudioInputDeviceSelection
{
    public const string UnavailableSelectedDeviceWarning =
        "Selected audio input is unavailable; using System Default";

    public static AudioInputDeviceSelectionResult BuildChoices(
        IReadOnlyList<AudioInputDevice> devices,
        AppSettings settings)
    {
        var choices = new List<AudioInputDeviceChoice>
        {
            new(null, "System Default", 0)
        };
        choices.AddRange(devices.Select(device => new AudioInputDeviceChoice(
            device.DeviceNumber,
            device.Name,
            device.Channels)));

        if (settings.AudioInputDeviceNumber is null)
        {
            return new AudioInputDeviceSelectionResult(choices, SelectedIndex: 0, Warning: null);
        }

        var selectedIndex = choices.FindIndex(choice =>
            choice.DeviceNumber == settings.AudioInputDeviceNumber);
        if (selectedIndex >= 0)
        {
            return new AudioInputDeviceSelectionResult(choices, selectedIndex, Warning: null);
        }

        return new AudioInputDeviceSelectionResult(
            choices,
            SelectedIndex: 0,
            Warning: UnavailableSelectedDeviceWarning);
    }
}
