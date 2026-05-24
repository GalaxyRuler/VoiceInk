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
