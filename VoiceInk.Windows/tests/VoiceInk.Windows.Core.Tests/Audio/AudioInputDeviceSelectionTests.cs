using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Settings;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Audio;

public sealed class AudioInputDeviceSelectionTests
{
    [Fact]
    public void DeviceHealthRows_ShowActiveBadgeForSelectedCustomDevice()
    {
        var choices = new[]
        {
            new AudioInputDeviceChoice(null, "System Default", 0),
            new AudioInputDeviceChoice(0, "Built-in Microphone", 2, "endpoint-built-in"),
            new AudioInputDeviceChoice(2, "USB Microphone", 1, "endpoint-usb")
        };

        var rows = AudioInputDeviceHealthPresenter.BuildRows(
            choices,
            choices[2],
            [],
            AudioInputModeSettings.Custom);

        Assert.Collection(
            rows,
            row =>
            {
                Assert.Equal("System Default", row.Name);
                Assert.Equal("Default", row.BadgeText);
                Assert.Equal(AudioInputDeviceSelectionNoticeKind.Info, row.BadgeKind);
                Assert.False(row.IsSelected);
                Assert.True(row.IsAvailable);
            },
            row =>
            {
                Assert.Equal("Built-in Microphone", row.Name);
                Assert.Equal("Available", row.BadgeText);
                Assert.Equal(AudioInputDeviceSelectionNoticeKind.Info, row.BadgeKind);
                Assert.Equal("2 channels - Device 0 - Endpoint endpoint-built-in", row.Detail);
                Assert.False(row.IsSelected);
                Assert.True(row.IsAvailable);
            },
            row =>
            {
                Assert.Equal("USB Microphone", row.Name);
                Assert.Equal("Active", row.BadgeText);
                Assert.Equal(AudioInputDeviceSelectionNoticeKind.Success, row.BadgeKind);
                Assert.Equal("Selected for recordings - 1 channel - Device 2 - Endpoint endpoint-usb", row.Detail);
                Assert.True(row.IsSelected);
                Assert.True(row.IsAvailable);
            });
    }

    [Fact]
    public void DeviceHealthRows_ShowUnavailablePrioritizedDevices()
    {
        var choices = new[]
        {
            new AudioInputDeviceChoice(null, "System Default", 0),
            new AudioInputDeviceChoice(1, "USB Microphone", 1, "endpoint-usb")
        };
        var prioritizedDevices = new[]
        {
            new PrioritizedAudioInputDevice("Dock Microphone", 0, "endpoint-dock"),
            new PrioritizedAudioInputDevice("USB Microphone", 1, "endpoint-usb")
        };

        var rows = AudioInputDeviceHealthPresenter.BuildRows(
            choices,
            choices[1],
            prioritizedDevices,
            AudioInputModeSettings.Prioritized);

        Assert.Collection(
            rows,
            row =>
            {
                Assert.Equal("Dock Microphone", row.Name);
                Assert.Equal("Unavailable", row.BadgeText);
                Assert.Equal(AudioInputDeviceSelectionNoticeKind.Warning, row.BadgeKind);
                Assert.Equal("Priority 1 - Not currently available", row.Detail);
                Assert.False(row.IsSelected);
                Assert.False(row.IsAvailable);
            },
            row =>
            {
                Assert.Equal("USB Microphone", row.Name);
                Assert.Equal("Active", row.BadgeText);
                Assert.Equal(AudioInputDeviceSelectionNoticeKind.Success, row.BadgeKind);
                Assert.Equal("Priority 2 - Selected fallback microphone - 1 channel - Device 1 - Endpoint endpoint-usb", row.Detail);
                Assert.True(row.IsSelected);
                Assert.True(row.IsAvailable);
            });
    }

    [Fact]
    public void DeviceHealthRows_DescribeFirstPrioritySelectionWithoutFallbackCopy()
    {
        var choices = new[]
        {
            new AudioInputDeviceChoice(null, "System Default", 0),
            new AudioInputDeviceChoice(2, "Dock Microphone", 1, "endpoint-dock")
        };
        var prioritizedDevices = new[]
        {
            new PrioritizedAudioInputDevice("Dock Microphone", 0, "endpoint-dock")
        };

        var rows = AudioInputDeviceHealthPresenter.BuildRows(
            choices,
            choices[1],
            prioritizedDevices,
            AudioInputModeSettings.Prioritized);

        var row = Assert.Single(rows);
        Assert.Equal("Active", row.BadgeText);
        Assert.Equal("Priority 1 - Selected microphone - 1 channel - Device 2 - Endpoint endpoint-dock", row.Detail);
    }

    [Fact]
    public void DeviceHealthRows_TruncateLongEndpointIdentifiers()
    {
        var choices = new[]
        {
            new AudioInputDeviceChoice(null, "System Default", 0),
            new AudioInputDeviceChoice(
                3,
                "Conference Microphone",
                1,
                "{0.0.1.00000000}.{a3b9fba0-1e2f-4c21-a123-123456789abc}")
        };

        var rows = AudioInputDeviceHealthPresenter.BuildRows(
            choices,
            choices[1],
            [],
            AudioInputModeSettings.Custom);

        Assert.Equal(
            "Selected for recordings - 1 channel - Device 3 - Endpoint {0.0.1.0...789abc}",
            rows[1].Detail);
    }

    [Fact]
    public void DeviceHealthRows_ShowSystemDefaultFallbackWhenPrioritizedDevicesAreUnavailable()
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

        Assert.Collection(
            rows,
            row =>
            {
                Assert.Equal("Dock Microphone", row.Name);
                Assert.Equal("Unavailable", row.BadgeText);
                Assert.False(row.IsAvailable);
            },
            row =>
            {
                Assert.Equal("System Default Fallback", row.Name);
                Assert.Equal("Active", row.BadgeText);
                Assert.Equal(AudioInputDeviceSelectionNoticeKind.Warning, row.BadgeKind);
                Assert.Equal("Using Windows system default because no prioritized microphones are available", row.Detail);
                Assert.True(row.IsSelected);
                Assert.True(row.IsAvailable);
            },
            row =>
            {
                Assert.Equal("Windows Sound Settings", row.Name);
                Assert.Equal("Open Settings", row.BadgeText);
                Assert.Equal(AudioInputDeviceSelectionNoticeKind.Info, row.BadgeKind);
                Assert.Equal("Open ms-settings:sound to choose or test the Windows default input device.", row.Detail);
                Assert.False(row.IsSelected);
                Assert.True(row.IsAvailable);
            },
            row =>
            {
                Assert.Equal("Microphone Privacy", row.Name);
                Assert.Equal("Check Access", row.BadgeText);
                Assert.Equal(AudioInputDeviceSelectionNoticeKind.Info, row.BadgeKind);
                Assert.Equal("Open ms-settings:privacy-microphone if Windows blocks desktop microphone access.", row.Detail);
                Assert.False(row.IsSelected);
                Assert.True(row.IsAvailable);
            });
    }

    [Fact]
    public void BuildChoices_SelectsFirstAvailablePrioritizedDevice()
    {
        var devices = new[]
        {
            new AudioInputDevice(0, "Built-in Microphone", 2),
            new AudioInputDevice(1, "USB Microphone", 1),
            new AudioInputDevice(2, "Dock Microphone", 1)
        };
        var settings = new AppSettings
        {
            AudioInputMode = AudioInputModeSettings.Prioritized,
            PrioritizedAudioInputDevices =
            [
                new PrioritizedAudioInputDevice("Dock Microphone", 0),
                new PrioritizedAudioInputDevice("USB Microphone", 1)
            ]
        };

        var result = AudioInputDeviceSelection.BuildChoices(devices, settings);

        Assert.Null(result.Warning);
        Assert.Equal(3, result.SelectedIndex);
        Assert.Equal(2, result.SelectedChoice?.DeviceNumber);
        Assert.Equal(AudioInputDeviceSelectionNoticeKind.Success, result.Notice.Kind);
        Assert.Equal("Dock Microphone", result.Notice.Title);
        Assert.Equal("VoiceInk selected the highest-priority available microphone.", result.Notice.Message);
        Assert.Equal("Priority 1", result.Notice.ActionText);
    }

    [Fact]
    public void BuildChoices_SkipsUnavailablePrioritizedDevices()
    {
        var devices = new[]
        {
            new AudioInputDevice(1, "USB Microphone", 1)
        };
        var settings = new AppSettings
        {
            AudioInputMode = AudioInputModeSettings.Prioritized,
            PrioritizedAudioInputDevices =
            [
                new PrioritizedAudioInputDevice("Dock Microphone", 0),
                new PrioritizedAudioInputDevice("USB Microphone", 1)
            ]
        };

        var result = AudioInputDeviceSelection.BuildChoices(devices, settings);

        Assert.Equal(1, result.SelectedIndex);
        Assert.Equal(1, result.SelectedChoice?.DeviceNumber);
        Assert.Equal("Selected prioritized audio input is unavailable; using next available priority", result.Warning);
        Assert.Equal(AudioInputDeviceSelectionNoticeKind.Warning, result.Notice.Kind);
        Assert.Equal("USB Microphone priority fallback", result.Notice.Title);
        Assert.Equal("VoiceInk skipped unavailable higher-priority microphones and selected this device.", result.Notice.Message);
        Assert.Equal("Priority 2", result.Notice.ActionText);
    }

    [Fact]
    public void BuildChoices_FallsBackToSystemDefaultWhenNoPrioritizedDevicesAreAvailable()
    {
        var devices = new[]
        {
            new AudioInputDevice(0, "Built-in Microphone", 2)
        };
        var settings = new AppSettings
        {
            AudioInputMode = AudioInputModeSettings.Prioritized,
            PrioritizedAudioInputDevices =
            [
                new PrioritizedAudioInputDevice("Dock Microphone", 0)
            ]
        };

        var result = AudioInputDeviceSelection.BuildChoices(devices, settings);

        Assert.Equal(0, result.SelectedIndex);
        Assert.Null(result.SelectedChoice?.DeviceNumber);
        Assert.Equal("Selected prioritized audio inputs are unavailable; using System Default", result.Warning);
        Assert.Equal(AudioInputDeviceSelectionNoticeKind.Warning, result.Notice.Kind);
        Assert.Equal("Prioritized microphones unavailable", result.Notice.Title);
        Assert.Equal("None of the prioritized microphones are currently available. VoiceInk will use the Windows system default microphone.", result.Notice.Message);
        Assert.Equal("Refresh or adjust priority list", result.Notice.ActionText);
    }

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
    public void BuildChoices_RebindsSavedCustomDeviceByEndpointIdWhenDeviceNumberChanges()
    {
        var devices = new[]
        {
            new AudioInputDevice(0, "Built-in Microphone", 2, "endpoint-built-in"),
            new AudioInputDevice(4, "Renamed USB Microphone", 1, "endpoint-usb")
        };
        var settings = new AppSettings
        {
            AudioInputDeviceNumber = 2,
            AudioInputDeviceName = "USB Microphone",
            AudioInputEndpointId = "endpoint-usb"
        };

        var result = AudioInputDeviceSelection.BuildChoices(devices, settings);

        Assert.Equal(2, result.SelectedIndex);
        Assert.Equal("Selected audio input endpoint reconnected; using saved endpoint ID", result.Warning);
        Assert.Equal(4, result.SelectedChoice?.DeviceNumber);
        Assert.Equal("endpoint-usb", result.SelectedChoice?.EndpointId);
        Assert.Equal(AudioInputDeviceSelectionNoticeKind.Warning, result.Notice.Kind);
        Assert.Equal("Renamed USB Microphone reconnected", result.Notice.Title);
        Assert.Equal("VoiceInk found the saved microphone by its Windows endpoint ID.", result.Notice.Message);
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
    public void BuildChoices_SelectsPrioritizedDeviceByEndpointIdBeforeName()
    {
        var devices = new[]
        {
            new AudioInputDevice(0, "USB Microphone", 2, "endpoint-old"),
            new AudioInputDevice(2, "Renamed Dock", 1, "endpoint-dock")
        };
        var settings = new AppSettings
        {
            AudioInputMode = AudioInputModeSettings.Prioritized,
            PrioritizedAudioInputDevices =
            [
                new PrioritizedAudioInputDevice("Dock Microphone", 0, "endpoint-dock"),
                new PrioritizedAudioInputDevice("USB Microphone", 1, "endpoint-old")
            ]
        };

        var result = AudioInputDeviceSelection.BuildChoices(devices, settings);

        Assert.Null(result.Warning);
        Assert.Equal(2, result.SelectedChoice?.DeviceNumber);
        Assert.Equal("endpoint-dock", result.SelectedChoice?.EndpointId);
        Assert.Equal("Renamed Dock", result.Notice.Title);
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
