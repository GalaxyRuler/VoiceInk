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
    }
}
