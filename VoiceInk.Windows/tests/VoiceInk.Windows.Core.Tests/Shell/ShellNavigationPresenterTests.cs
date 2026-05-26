using VoiceInk.Windows.Core.Shell;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Shell;

public sealed class ShellNavigationPresenterTests
{
    [Fact]
    public void BuildItems_ReturnsImplementedWindowsSectionsInMacOrder()
    {
        var items = ShellNavigationPresenter.BuildItems();

        Assert.Collection(
            items,
            item => Assert.Equal(("Dashboard", "Dashboard", "Home", true), (item.Tag, item.Label, item.Icon, item.IsEnabled)),
            item => Assert.Equal(("Transcribe Audio", "Transcribe Audio", "Audio", true), (item.Tag, item.Label, item.Icon, item.IsEnabled)),
            item => Assert.Equal(("History", "History", "Document", true), (item.Tag, item.Label, item.Icon, item.IsEnabled)),
            item => Assert.Equal(("Metrics", "Metrics", "Calculator", true), (item.Tag, item.Label, item.Icon, item.IsEnabled)),
            item => Assert.Equal(("AI Models", "AI Models", "Library", true), (item.Tag, item.Label, item.Icon, item.IsEnabled)),
            item => Assert.Equal(("Enhancement", "Enhancement", "Edit", true), (item.Tag, item.Label, item.Icon, item.IsEnabled)),
            item => Assert.Equal(("Power Mode", "Power Mode", "LightningBolt", true), (item.Tag, item.Label, item.Icon, item.IsEnabled)),
            item => Assert.Equal(("Permissions", "Permissions", "Permissions", true), (item.Tag, item.Label, item.Icon, item.IsEnabled)),
            item => Assert.Equal(("Audio Input", "Audio Input", "Microphone", true), (item.Tag, item.Label, item.Icon, item.IsEnabled)),
            item => Assert.Equal(("Dictionary", "Dictionary", "Character", true), (item.Tag, item.Label, item.Icon, item.IsEnabled)),
            item => Assert.Equal(("Settings", "Settings", "Setting", true), (item.Tag, item.Label, item.Icon, item.IsEnabled)),
            item => Assert.Equal(("About", "About / Open Source", "Help", true), (item.Tag, item.Label, item.Icon, item.IsEnabled)));
    }

    [Fact]
    public void BuildItems_ReplacesCommercialVoiceInkProWithAboutOpenSource()
    {
        var labels = ShellNavigationPresenter.BuildItems().Select(item => item.Label);

        Assert.Contains("About / Open Source", labels);
        Assert.DoesNotContain("VoiceInk Pro", labels);
    }
}
