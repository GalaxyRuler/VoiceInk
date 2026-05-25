using VoiceInk.Windows.Core.Startup;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Startup;

public sealed class StartupLaunchModeTests
{
    [Theory]
    [InlineData("--voiceink-startup")]
    [InlineData("--VOICEINK-STARTUP")]
    [InlineData("/voiceink-startup")]
    public void IsLoginStartup_ReturnsTrueForStartupArgument(string argument)
    {
        var isStartup = StartupLaunchMode.IsLoginStartup(["--another", argument]);

        Assert.True(isStartup);
    }

    [Theory]
    [InlineData()]
    [InlineData("--another")]
    [InlineData("voiceink-startup")]
    public void IsLoginStartup_ReturnsFalseForNormalLaunch(params string[] arguments)
    {
        var isStartup = StartupLaunchMode.IsLoginStartup(arguments);

        Assert.False(isStartup);
    }
}
