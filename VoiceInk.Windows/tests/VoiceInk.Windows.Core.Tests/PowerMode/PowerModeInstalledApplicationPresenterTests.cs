using VoiceInk.Windows.Core.PowerMode;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.PowerMode;

public sealed class PowerModeInstalledApplicationPresenterTests
{
    [Fact]
    public void Present_SortsDeduplicatesAndDropsChoicesWithoutProcessNames()
    {
        var choices = PowerModeInstalledApplicationPresenter.Present(
            [
                new PowerModeInstalledApplicationChoice("Zoom Workplace", "Zoom.exe", "Start Menu"),
                new PowerModeInstalledApplicationChoice("Empty", "", "Start Menu"),
                new PowerModeInstalledApplicationChoice("Microsoft Teams", "ms-teams.exe", "Start Menu"),
                new PowerModeInstalledApplicationChoice("Zoom", "zoom", "Start Menu")
            ]);

        Assert.Equal(["Microsoft Teams", "Zoom"], choices.Select(choice => choice.DisplayName).ToArray());
        Assert.Equal(["ms-teams", "zoom"], choices.Select(choice => choice.ProcessName).ToArray());
        Assert.Equal("Microsoft Teams (ms-teams)", choices[0].DisplayLabel);
    }

    [Fact]
    public void AppendChoice_AppendsProcessNameWithoutDuplicatingExistingTargets()
    {
        var fields = PowerModeInstalledApplicationPresenter.AppendChoice(
            new PowerModeTargetFields("teams; zoom", "chat", "example.com"),
            new PowerModeInstalledApplicationChoice("Zoom Workplace", "Zoom.exe", "Start Menu"));

        Assert.Equal("teams; zoom", fields.ProcessNamePattern);
        Assert.Equal("chat", fields.WindowTitlePattern);
        Assert.Equal("example.com", fields.BrowserUrlPattern);

        var appended = PowerModeInstalledApplicationPresenter.AppendChoice(
            fields,
            new PowerModeInstalledApplicationChoice("Slack", "slack.exe", "Start Menu"));

        Assert.Equal("teams; zoom; slack", appended.ProcessNamePattern);
    }
}
