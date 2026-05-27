using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Shell;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Shell;

public sealed class ShellStartupVisibilityTests
{
    [Fact]
    public void ShouldStartHidden_ReturnsTrueForLoginStartup()
    {
        Assert.True(ShellStartupVisibility.ShouldStartHidden(
            isLoginStartupLaunch: true,
            new AppSettings()));
    }

    [Fact]
    public void ShouldStartHidden_ReturnsTrueForCompletedOnboardingAndTrayFirstSetting()
    {
        Assert.True(ShellStartupVisibility.ShouldStartHidden(
            isLoginStartupLaunch: false,
            new AppSettings
            {
                HasCompletedOnboarding = true,
                StartHiddenToTray = true
            }));
    }

    [Fact]
    public void ShouldStartHidden_DoesNotHideFirstRunOnboarding()
    {
        Assert.False(ShellStartupVisibility.ShouldStartHidden(
            isLoginStartupLaunch: false,
            new AppSettings
            {
                HasCompletedOnboarding = false,
                StartHiddenToTray = true
            }));
    }
}
