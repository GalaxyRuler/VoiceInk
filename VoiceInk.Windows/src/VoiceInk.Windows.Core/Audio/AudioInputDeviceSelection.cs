using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Audio;

public static class AudioInputDeviceSelection
{
    public const string UnavailableSelectedDeviceWarning =
        "Selected audio input is unavailable; using System Default";

    public const string ReboundSelectedDeviceWarning =
        "Selected audio input device number changed; using saved device name";

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
            choice.DeviceNumber == settings.AudioInputDeviceNumber
            && NamesMatch(choice.Name, settings.AudioInputDeviceName));
        if (selectedIndex >= 0)
        {
            return new AudioInputDeviceSelectionResult(choices, selectedIndex, Warning: null);
        }

        var nameMatches = choices
            .Select((choice, index) => new { Choice = choice, Index = index })
            .Where(candidate =>
                candidate.Choice.DeviceNumber is not null
                && NamesMatch(candidate.Choice.Name, settings.AudioInputDeviceName))
            .ToArray();
        if (nameMatches.Length == 1)
        {
            return new AudioInputDeviceSelectionResult(
                choices,
                nameMatches[0].Index,
                Warning: ReboundSelectedDeviceWarning);
        }

        return new AudioInputDeviceSelectionResult(
            choices,
            SelectedIndex: 0,
            Warning: UnavailableSelectedDeviceWarning);
    }

    private static bool NamesMatch(string currentName, string savedName) =>
        !string.IsNullOrWhiteSpace(savedName)
        && string.Equals(currentName, savedName, StringComparison.Ordinal);
}
