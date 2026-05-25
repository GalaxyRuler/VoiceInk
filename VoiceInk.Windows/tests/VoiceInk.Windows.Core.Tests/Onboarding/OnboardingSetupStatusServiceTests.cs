using VoiceInk.Windows.Core.Onboarding;
using VoiceInk.Windows.Core.Settings;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Onboarding;

public sealed class OnboardingSetupStatusServiceTests
{
    [Fact]
    public void Build_BlankModelPathWithDefaultShortcut_IsIncomplete()
    {
        var status = OnboardingSetupStatusService.Build(new AppSettings(), hasAudioInputChoices: true);

        Assert.False(status.HasModelPath);
        Assert.True(status.HasPrimaryShortcut);
        Assert.True(status.HasAudioInputChoices);
        Assert.False(status.CanCompleteSetup);
        Assert.Equal("Microphone detected", status.MicrophoneStatusTitle);
        Assert.Equal("VoiceInk found at least one recording input.", status.MicrophoneStatusMessage);
        Assert.Equal("Open Windows Microphone Settings", status.MicrophoneActionText);
    }

    [Fact]
    public void Build_ModelPathAndPrimaryShortcut_CanCompleteWithoutAudioDevices()
    {
        var status = OnboardingSetupStatusService.Build(
            new AppSettings
            {
                ModelPath = "C:\\Models\\ggml-base.en.bin",
                Hotkey = "Ctrl+Alt+Space"
            },
            hasAudioInputChoices: false);

        Assert.True(status.HasModelPath);
        Assert.True(status.HasPrimaryShortcut);
        Assert.False(status.HasAudioInputChoices);
        Assert.True(status.CanCompleteSetup);
        Assert.Equal("No microphone detected", status.MicrophoneStatusTitle);
        Assert.Equal(
            "Connect or enable a microphone, refresh the device list, or check Windows microphone privacy settings.",
            status.MicrophoneStatusMessage);
        Assert.Equal("Check Windows Microphone Settings", status.MicrophoneActionText);
    }

    [Fact]
    public void Build_BlankPrimaryShortcut_IsIncomplete()
    {
        var status = OnboardingSetupStatusService.Build(
            new AppSettings
            {
                ModelPath = "C:\\Models\\ggml-base.en.bin",
                Hotkey = " "
            },
            hasAudioInputChoices: true);

        Assert.True(status.HasModelPath);
        Assert.False(status.HasPrimaryShortcut);
        Assert.True(status.HasAudioInputChoices);
        Assert.False(status.CanCompleteSetup);
    }
}
