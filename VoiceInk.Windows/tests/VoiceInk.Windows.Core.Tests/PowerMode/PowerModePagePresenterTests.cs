using VoiceInk.Windows.Core.PowerMode;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.PowerMode;

public sealed class PowerModePagePresenterTests
{
    [Fact]
    public void Present_BuildsMacStylePowerModePageCopyAndCounts()
    {
        var rules = new[]
        {
            new PowerModeRule { Name = "Writing", IsEnabled = true },
            new PowerModeRule { Name = "Archive", IsEnabled = false }
        };

        var presentation = PowerModePagePresenter.Present(rules);

        Assert.Equal("Power Modes", presentation.Title);
        Assert.Equal("Automate your workflows with context-aware configurations.", presentation.Description);
        Assert.Equal("2 Power Modes (1 enabled, 1 disabled)", presentation.CountLabel);
        Assert.False(presentation.IsEmpty);
        Assert.Equal(string.Empty, presentation.EmptyTitle);
        Assert.Equal(string.Empty, presentation.EmptyDescription);
    }

    [Fact]
    public void Present_BuildsMacStyleEmptyState()
    {
        var presentation = PowerModePagePresenter.Present([]);

        Assert.True(presentation.IsEmpty);
        Assert.Equal("No Power Modes Yet", presentation.EmptyTitle);
        Assert.Equal(
            "Create your first power mode to automate your VoiceInk workflow based on apps and websites.",
            presentation.EmptyDescription);
        Assert.Equal("0 Power Modes", presentation.CountLabel);
    }
}
