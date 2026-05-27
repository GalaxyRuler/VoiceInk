using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Settings;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Audio;

public sealed class AudioInputDeviceHealthPresenterTests
{
    [Fact]
    public void BuildRows_SelectedSystemDefault_ShowsWindowsGuidanceRows()
    {
        var choices = new[]
        {
            new AudioInputDeviceChoice(null, "System Default", 0),
            new AudioInputDeviceChoice(2, "USB Microphone", 1, "endpoint-usb")
        };

        var rows = AudioInputDeviceHealthPresenter.BuildRows(
            choices,
            choices[0],
            [],
            AudioInputModeSettings.SystemDefault);

        Assert.Contains(
            rows,
            row => row.Name == "Windows Sound Settings"
                && row.BadgeText == "Open Settings"
                && row.Detail == "Open ms-settings:sound to choose or test the Windows default input device.");
        Assert.Contains(
            rows,
            row => row.Name == "Microphone Privacy"
                && row.BadgeText == "Check Access"
                && row.Detail == "Open ms-settings:privacy-microphone and enable 'Let desktop apps access your microphone' if Windows blocks recording.");
    }

    [Fact]
    public void BuildRows_SelectedCustomDevice_ExposesAccessibleName()
    {
        var choices = new[]
        {
            new AudioInputDeviceChoice(null, "System Default", 0),
            new AudioInputDeviceChoice(2, "USB Microphone", 1, "endpoint-usb")
        };

        var rows = AudioInputDeviceHealthPresenter.BuildRows(
            choices,
            choices[1],
            [],
            AudioInputModeSettings.Custom);

        Assert.Equal(
            "USB Microphone, Active, Selected for recordings - 1 channel - Device 2 - Endpoint endpoint-usb",
            rows[1].AccessibleName);
    }

    [Fact]
    public void BuildRows_PrioritizedFallbackPrivacyGuidance_ExposesAccessibleName()
    {
        var choices = new[]
        {
            new AudioInputDeviceChoice(null, "System Default", 0),
            new AudioInputDeviceChoice(0, "Built-in Microphone", 2, "endpoint-built-in")
        };
        var prioritizedDevices = new[]
        {
            new PrioritizedAudioInputDevice("Dock Microphone", 0, "endpoint-dock")
        };

        var rows = AudioInputDeviceHealthPresenter.BuildRows(
            choices,
            choices[0],
            prioritizedDevices,
            AudioInputModeSettings.Prioritized);

        Assert.Contains(
            rows,
            row => row.Name == "Microphone Privacy"
                && row.AccessibleName == "Microphone Privacy, Check Access, Open ms-settings:privacy-microphone and enable 'Let desktop apps access your microphone' if Windows blocks recording.");
    }
}
