using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Settings;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Audio;

public sealed class AudioInputDeviceSelectionTests
{
    [Fact]
    public void BuildChoices_SelectsSavedCustomDeviceWhenAvailable()
    {
        var devices = new[]
        {
            new AudioInputDevice(0, "Built-in Microphone", 2),
            new AudioInputDevice(2, "USB Microphone", 1)
        };
        var settings = new AppSettings
        {
            AudioInputDeviceNumber = 2,
            AudioInputDeviceName = "USB Microphone"
        };

        var result = AudioInputDeviceSelection.BuildChoices(devices, settings);

        Assert.Null(result.Warning);
        Assert.Equal(2, result.SelectedIndex);
        Assert.Equal("System Default", result.Choices[0].DisplayText);
        Assert.Equal("USB Microphone (2)", result.Choices[result.SelectedIndex].DisplayText);
        Assert.Equal(2, result.Choices[result.SelectedIndex].DeviceNumber);
        Assert.Equal(2, result.SelectedChoice?.DeviceNumber);
        Assert.Equal(AudioInputDeviceSelectionNoticeKind.Success, result.Notice.Kind);
        Assert.Equal("USB Microphone", result.Notice.Title);
        Assert.Equal("VoiceInk is set to use this microphone for recordings.", result.Notice.Message);
        Assert.Equal("1 channel", result.Notice.ActionText);
    }

    [Fact]
    public void BuildChoices_FallsBackToSystemDefaultWhenSavedCustomDeviceIsUnavailable()
    {
        var devices = new[]
        {
            new AudioInputDevice(0, "Built-in Microphone", 2)
        };
        var settings = new AppSettings
        {
            AudioInputDeviceNumber = 4,
            AudioInputDeviceName = "Dock Microphone"
        };

        var result = AudioInputDeviceSelection.BuildChoices(devices, settings);

        Assert.Equal(0, result.SelectedIndex);
        Assert.Equal("Selected audio input is unavailable; using System Default", result.Warning);
        Assert.Null(result.Choices[result.SelectedIndex].DeviceNumber);
        Assert.Null(result.SelectedChoice?.DeviceNumber);
        Assert.Equal(AudioInputDeviceSelectionNoticeKind.Warning, result.Notice.Kind);
        Assert.Equal("Saved microphone unavailable", result.Notice.Title);
        Assert.Equal("Dock Microphone is not currently available. VoiceInk will use the Windows system default microphone.", result.Notice.Message);
        Assert.Equal("Refresh or choose another input", result.Notice.ActionText);
    }

    [Fact]
    public void BuildChoices_FallsBackWhenSavedNumberNowBelongsToDifferentDevice()
    {
        var devices = new[]
        {
            new AudioInputDevice(2, "Conference Room Microphone", 2)
        };
        var settings = new AppSettings
        {
            AudioInputDeviceNumber = 2,
            AudioInputDeviceName = "USB Microphone"
        };

        var result = AudioInputDeviceSelection.BuildChoices(devices, settings);

        Assert.Equal(0, result.SelectedIndex);
        Assert.Equal("Selected audio input is unavailable; using System Default", result.Warning);
        Assert.Null(result.Choices[result.SelectedIndex].DeviceNumber);
        Assert.Null(result.SelectedChoice?.DeviceNumber);
        Assert.Equal(AudioInputDeviceSelectionNoticeKind.Warning, result.Notice.Kind);
        Assert.Equal("Saved microphone unavailable", result.Notice.Title);
        Assert.Equal("USB Microphone is not currently available. VoiceInk will use the Windows system default microphone.", result.Notice.Message);
    }

    [Fact]
    public void BuildChoices_RebindsSavedNameWhenDeviceNumberChanges()
    {
        var devices = new[]
        {
            new AudioInputDevice(0, "Built-in Microphone", 2),
            new AudioInputDevice(4, "USB Microphone", 1)
        };
        var settings = new AppSettings
        {
            AudioInputDeviceNumber = 2,
            AudioInputDeviceName = "USB Microphone"
        };

        var result = AudioInputDeviceSelection.BuildChoices(devices, settings);

        Assert.Equal(2, result.SelectedIndex);
        Assert.Equal("Selected audio input device number changed; using saved device name", result.Warning);
        Assert.Equal(4, result.Choices[result.SelectedIndex].DeviceNumber);
        Assert.Equal(4, result.SelectedChoice?.DeviceNumber);
        Assert.Equal(AudioInputDeviceSelectionNoticeKind.Warning, result.Notice.Kind);
        Assert.Equal("USB Microphone reconnected", result.Notice.Title);
        Assert.Equal("VoiceInk found the saved microphone by name after its Windows device number changed.", result.Notice.Message);
        Assert.Equal("Using device 4", result.Notice.ActionText);
    }

    [Fact]
    public void BuildChoices_FallsBackWhenSavedNameMatchesMultipleShiftedDevices()
    {
        var devices = new[]
        {
            new AudioInputDevice(4, "USB Microphone", 1),
            new AudioInputDevice(5, "USB Microphone", 1)
        };
        var settings = new AppSettings
        {
            AudioInputDeviceNumber = 2,
            AudioInputDeviceName = "USB Microphone"
        };

        var result = AudioInputDeviceSelection.BuildChoices(devices, settings);

        Assert.Equal(0, result.SelectedIndex);
        Assert.Equal("Selected audio input is unavailable; using System Default", result.Warning);
        Assert.Null(result.Choices[result.SelectedIndex].DeviceNumber);
        Assert.Null(result.SelectedChoice?.DeviceNumber);
        Assert.Equal(AudioInputDeviceSelectionNoticeKind.Warning, result.Notice.Kind);
        Assert.Equal("Saved microphone unavailable", result.Notice.Title);
    }

    [Fact]
    public void BuildChoices_SelectsSystemDefaultWhenNoCustomDeviceIsSaved()
    {
        var devices = new[]
        {
            new AudioInputDevice(0, "Built-in Microphone", 2)
        };

        var result = AudioInputDeviceSelection.BuildChoices(devices, new AppSettings());

        Assert.Null(result.Warning);
        Assert.Equal(0, result.SelectedIndex);
        Assert.Equal("System Default", result.Choices[result.SelectedIndex].DisplayText);
        Assert.Null(result.SelectedChoice?.DeviceNumber);
        Assert.Equal(AudioInputDeviceSelectionNoticeKind.Info, result.Notice.Kind);
        Assert.Equal("System Default", result.Notice.Title);
        Assert.Equal("VoiceInk will follow the Windows default microphone.", result.Notice.Message);
        Assert.Equal("1 input available", result.Notice.ActionText);
    }

    [Fact]
    public void BuildChoices_ShowsErrorNoticeWhenNoPhysicalInputsAreAvailable()
    {
        var result = AudioInputDeviceSelection.BuildChoices([], new AppSettings());

        Assert.Equal(0, result.SelectedIndex);
        Assert.Null(result.Warning);
        Assert.Equal(AudioInputDeviceSelectionNoticeKind.Error, result.Notice.Kind);
        Assert.Equal("No microphone detected", result.Notice.Title);
        Assert.Equal("Connect or enable a microphone, then refresh audio inputs.", result.Notice.Message);
        Assert.Equal("Refresh after connecting a microphone", result.Notice.ActionText);
    }

    [Fact]
    public void BuildNotice_RecomputesNoticeAfterUserSelectsAvailableCustomDevice()
    {
        var choices = new[]
        {
            new AudioInputDeviceChoice(null, "System Default", 0),
            new AudioInputDeviceChoice(2, "USB Microphone", 1)
        };
        var settings = new AppSettings
        {
            AudioInputDeviceNumber = 2,
            AudioInputDeviceName = "USB Microphone"
        };

        var notice = AudioInputDeviceSelection.BuildNotice(choices, choices[1], settings);

        Assert.Equal(AudioInputDeviceSelectionNoticeKind.Success, notice.Kind);
        Assert.Equal("USB Microphone", notice.Title);
        Assert.Equal("VoiceInk is set to use this microphone for recordings.", notice.Message);
    }

    [Fact]
    public void BuildNotice_RecomputesNoticeAfterUserSelectsSystemDefault()
    {
        var choices = new[]
        {
            new AudioInputDeviceChoice(null, "System Default", 0),
            new AudioInputDeviceChoice(2, "USB Microphone", 1)
        };

        var notice = AudioInputDeviceSelection.BuildNotice(choices, choices[0], new AppSettings());

        Assert.Equal(AudioInputDeviceSelectionNoticeKind.Info, notice.Kind);
        Assert.Equal("System Default", notice.Title);
        Assert.Equal("1 input available", notice.ActionText);
    }
}
