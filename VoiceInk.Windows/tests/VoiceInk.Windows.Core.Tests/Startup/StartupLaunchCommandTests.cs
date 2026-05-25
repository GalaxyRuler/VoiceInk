using VoiceInk.Windows.Core.Startup;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Startup;

public sealed class StartupLaunchCommandTests
{
    [Fact]
    public void Build_QuotesExecutablePathAndAddsStartupArgument()
    {
        var command = StartupLaunchCommand.Build(@"C:\Program Files\VoiceInk\VoiceInk.Windows.App.exe");

        Assert.Equal(
            @"""C:\Program Files\VoiceInk\VoiceInk.Windows.App.exe"" --voiceink-startup",
            command);
    }

    [Fact]
    public void Build_EscapesEmbeddedQuotes()
    {
        var command = StartupLaunchCommand.Build(@"C:\Tools\Voice""Ink\VoiceInk.Windows.App.exe");

        Assert.Equal(
            @"""C:\Tools\Voice\""Ink\VoiceInk.Windows.App.exe"" --voiceink-startup",
            command);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Build_ReturnsEmptyCommandForMissingExecutable(string? executablePath)
    {
        var command = StartupLaunchCommand.Build(executablePath);

        Assert.Equal(string.Empty, command);
    }
}
