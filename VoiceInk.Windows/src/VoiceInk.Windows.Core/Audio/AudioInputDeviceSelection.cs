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
            return new AudioInputDeviceSelectionResult(
                choices,
                SelectedIndex: 0,
                Warning: null,
                Notice: BuildSystemDefaultNotice(devices.Count));
        }

        var selectedIndex = choices.FindIndex(choice =>
            choice.DeviceNumber == settings.AudioInputDeviceNumber
            && NamesMatch(choice.Name, settings.AudioInputDeviceName));
        if (selectedIndex >= 0)
        {
            return new AudioInputDeviceSelectionResult(
                choices,
                selectedIndex,
                Warning: null,
                Notice: BuildCustomDeviceNotice(choices[selectedIndex]));
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
                Warning: ReboundSelectedDeviceWarning,
                Notice: BuildReboundDeviceNotice(nameMatches[0].Choice));
        }

        return new AudioInputDeviceSelectionResult(
            choices,
            SelectedIndex: 0,
            Warning: UnavailableSelectedDeviceWarning,
            Notice: BuildUnavailableDeviceNotice(settings.AudioInputDeviceName));
    }

    public static AudioInputDeviceSelectionNotice BuildNotice(
        IReadOnlyList<AudioInputDeviceChoice> choices,
        AudioInputDeviceChoice? selectedChoice,
        AppSettings settings)
    {
        var physicalDeviceCount = choices.Count(choice => choice.DeviceNumber is not null);
        if (selectedChoice?.DeviceNumber is null)
        {
            return settings.AudioInputDeviceNumber is null
                ? BuildSystemDefaultNotice(physicalDeviceCount)
                : BuildUnavailableDeviceNotice(settings.AudioInputDeviceName);
        }

        return BuildCustomDeviceNotice(selectedChoice);
    }

    private static bool NamesMatch(string currentName, string savedName) =>
        !string.IsNullOrWhiteSpace(savedName)
        && string.Equals(currentName, savedName, StringComparison.Ordinal);

    private static AudioInputDeviceSelectionNotice BuildSystemDefaultNotice(int physicalDeviceCount) =>
        physicalDeviceCount <= 0
            ? new AudioInputDeviceSelectionNotice(
                AudioInputDeviceSelectionNoticeKind.Error,
                "No microphone detected",
                "Connect or enable a microphone, then refresh audio inputs.",
                "Refresh after connecting a microphone")
            : new AudioInputDeviceSelectionNotice(
                AudioInputDeviceSelectionNoticeKind.Info,
                "System Default",
                "VoiceInk will follow the Windows default microphone.",
                $"{physicalDeviceCount} input{(physicalDeviceCount == 1 ? string.Empty : "s")} available");

    private static AudioInputDeviceSelectionNotice BuildCustomDeviceNotice(AudioInputDeviceChoice choice) =>
        new(
            AudioInputDeviceSelectionNoticeKind.Success,
            choice.Name,
            "VoiceInk is set to use this microphone for recordings.",
            $"{choice.Channels} channel{(choice.Channels == 1 ? string.Empty : "s")}");

    private static AudioInputDeviceSelectionNotice BuildReboundDeviceNotice(AudioInputDeviceChoice choice) =>
        new(
            AudioInputDeviceSelectionNoticeKind.Warning,
            $"{choice.Name} reconnected",
            "VoiceInk found the saved microphone by name after its Windows device number changed.",
            $"Using device {choice.DeviceNumber}");

    private static AudioInputDeviceSelectionNotice BuildUnavailableDeviceNotice(string savedName)
    {
        var microphoneName = string.IsNullOrWhiteSpace(savedName)
            ? "The saved microphone"
            : savedName;
        return new AudioInputDeviceSelectionNotice(
            AudioInputDeviceSelectionNoticeKind.Warning,
            "Saved microphone unavailable",
            $"{microphoneName} is not currently available. VoiceInk will use the Windows system default microphone.",
            "Refresh or choose another input");
    }
}
