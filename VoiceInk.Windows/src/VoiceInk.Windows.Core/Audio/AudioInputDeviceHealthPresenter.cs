using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Audio;

public sealed record AudioInputDeviceHealthRow(
    string Name,
    string Detail,
    string BadgeText,
    AudioInputDeviceSelectionNoticeKind BadgeKind,
    bool IsSelected,
    bool IsAvailable)
{
    public string AccessibleName => string.Join(
        ", ",
        new[] { Name, BadgeText, Detail }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part.Trim()));
}

public static class AudioInputDeviceHealthPresenter
{
    public static IReadOnlyList<AudioInputDeviceHealthRow> BuildRows(
        IReadOnlyList<AudioInputDeviceChoice> choices,
        AudioInputDeviceChoice? selectedChoice,
        IReadOnlyList<PrioritizedAudioInputDevice> prioritizedDevices,
        string audioInputMode)
    {
        return AudioInputModeSettings.Normalize(audioInputMode) == AudioInputModeSettings.Prioritized
            && prioritizedDevices.Count > 0
                ? BuildPrioritizedRows(choices, selectedChoice, prioritizedDevices)
                : BuildChoiceRows(choices, selectedChoice);
    }

    private static IReadOnlyList<AudioInputDeviceHealthRow> BuildChoiceRows(
        IReadOnlyList<AudioInputDeviceChoice> choices,
        AudioInputDeviceChoice? selectedChoice) =>
        choices
            .Select(choice => BuildChoiceRow(choice, selectedChoice))
            .ToArray();

    private static IReadOnlyList<AudioInputDeviceHealthRow> BuildPrioritizedRows(
        IReadOnlyList<AudioInputDeviceChoice> choices,
        AudioInputDeviceChoice? selectedChoice,
        IReadOnlyList<PrioritizedAudioInputDevice> prioritizedDevices)
    {
        var rows = prioritizedDevices
            .OrderBy(device => device.Priority)
            .Select((device, index) =>
            {
                var choice = choices.FirstOrDefault(candidate =>
                    candidate.DeviceNumber is not null
                    && DeviceMatches(candidate, device));
                return choice is null
                    ? BuildUnavailablePriorityRow(device, index + 1)
                    : BuildAvailablePriorityRow(choice, selectedChoice, index + 1);
            })
            .ToArray();

        if (selectedChoice?.DeviceNumber is null && rows.All(row => !row.IsAvailable))
        {
            return
            [
                .. rows,
                new AudioInputDeviceHealthRow(
                    "System Default Fallback",
                    "Using Windows system default because no prioritized microphones are available",
                    "Active",
                    AudioInputDeviceSelectionNoticeKind.Warning,
                    IsSelected: true,
                    IsAvailable: true),
                new AudioInputDeviceHealthRow(
                    "Windows Sound Settings",
                    "Open ms-settings:sound to choose or test the Windows default input device.",
                    "Open Settings",
                    AudioInputDeviceSelectionNoticeKind.Info,
                    IsSelected: false,
                    IsAvailable: true),
                new AudioInputDeviceHealthRow(
                    "Microphone Privacy",
                    "Open ms-settings:privacy-microphone and enable 'Let desktop apps access your microphone' if Windows blocks recording.",
                    "Check Access",
                    AudioInputDeviceSelectionNoticeKind.Info,
                    IsSelected: false,
                    IsAvailable: true)
            ];
        }

        return rows;
    }

    private static AudioInputDeviceHealthRow BuildChoiceRow(
        AudioInputDeviceChoice choice,
        AudioInputDeviceChoice? selectedChoice)
    {
        var isSelected = ChoicesMatch(choice, selectedChoice);
        if (choice.DeviceNumber is null)
        {
            return new AudioInputDeviceHealthRow(
                "System Default",
                "Follows the Windows default microphone",
                isSelected ? "Active" : "Default",
                isSelected ? AudioInputDeviceSelectionNoticeKind.Success : AudioInputDeviceSelectionNoticeKind.Info,
                isSelected,
                IsAvailable: true);
        }

        return new AudioInputDeviceHealthRow(
            choice.Name,
            $"{(isSelected ? "Selected for recordings - " : string.Empty)}{ChannelText(choice.Channels)} - Device {choice.DeviceNumber}{EndpointText(choice.EndpointId)}",
            isSelected ? "Active" : "Available",
            isSelected ? AudioInputDeviceSelectionNoticeKind.Success : AudioInputDeviceSelectionNoticeKind.Info,
            isSelected,
            IsAvailable: true);
    }

    private static AudioInputDeviceHealthRow BuildAvailablePriorityRow(
        AudioInputDeviceChoice choice,
        AudioInputDeviceChoice? selectedChoice,
        int priorityNumber)
    {
        var isSelected = ChoicesMatch(choice, selectedChoice);
        var selectedDescription = priorityNumber == 1
            ? "Selected microphone"
            : "Selected fallback microphone";
        return new AudioInputDeviceHealthRow(
            choice.Name,
            $"Priority {priorityNumber} - {(isSelected ? selectedDescription : "Available microphone")} - {ChannelText(choice.Channels)} - Device {choice.DeviceNumber}{EndpointText(choice.EndpointId)}",
            isSelected ? "Active" : "Available",
            isSelected ? AudioInputDeviceSelectionNoticeKind.Success : AudioInputDeviceSelectionNoticeKind.Info,
            isSelected,
            IsAvailable: true);
    }

    private static AudioInputDeviceHealthRow BuildUnavailablePriorityRow(
        PrioritizedAudioInputDevice device,
        int priorityNumber) =>
        new(
            device.Name,
            $"Priority {priorityNumber} - Not currently available",
            "Unavailable",
            AudioInputDeviceSelectionNoticeKind.Warning,
            IsSelected: false,
            IsAvailable: false);

    private static bool DeviceMatches(
        AudioInputDeviceChoice choice,
        PrioritizedAudioInputDevice device) =>
        (!string.IsNullOrWhiteSpace(device.EndpointId)
            && string.Equals(choice.EndpointId, device.EndpointId, StringComparison.Ordinal))
        || string.Equals(choice.Name, device.Name, StringComparison.Ordinal);

    private static bool ChoicesMatch(AudioInputDeviceChoice choice, AudioInputDeviceChoice? selectedChoice) =>
        selectedChoice is not null
        && choice.DeviceNumber == selectedChoice.DeviceNumber
        && string.Equals(choice.EndpointId, selectedChoice.EndpointId, StringComparison.Ordinal)
        && string.Equals(choice.Name, selectedChoice.Name, StringComparison.Ordinal);

    private static string ChannelText(int channels) =>
        $"{channels} channel{(channels == 1 ? string.Empty : "s")}";

    private static string EndpointText(string? endpointId)
    {
        if (string.IsNullOrWhiteSpace(endpointId))
        {
            return string.Empty;
        }

        return $" - Endpoint {ShortEndpointId(endpointId)}";
    }

    private static string ShortEndpointId(string endpointId)
    {
        var trimmedEndpointId = endpointId.Trim();
        return trimmedEndpointId.Length <= 18
            ? trimmedEndpointId
            : $"{trimmedEndpointId[..8]}...{trimmedEndpointId[^7..]}";
    }
}
