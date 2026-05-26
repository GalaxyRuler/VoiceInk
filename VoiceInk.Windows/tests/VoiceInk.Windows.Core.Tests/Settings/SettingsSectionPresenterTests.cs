using VoiceInk.Windows.Core.Settings;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Settings;

public sealed class SettingsSectionPresenterTests
{
    [Fact]
    public void Present_ReturnsMacStyleSettingsSectionCopy()
    {
        var presentation = SettingsSectionPresenter.Present();

        Assert.Equal("Settings", presentation.HeroTitle);
        Assert.Equal("Tune the everyday behavior of VoiceInk.", presentation.HeroDescription);
        Assert.Equal(
            "Settings, backups, cleanup, and diagnostics stay local to this Windows profile.",
            presentation.OverviewSummary);
        Assert.Equal(
            "Backups exclude API keys. Diagnostic exports use sanitized local logs and are never sent automatically.",
            presentation.DataSafetyGuidance);
        Assert.Equal(
            [
                "Shortcuts",
                "Recording Feedback",
                "Interface",
                "Clipboard",
                "Cleanup",
                "Privacy",
                "General",
                "Backup",
                "Diagnostics"
            ],
            presentation.Sections.Select(section => section.Title).ToArray());
        Assert.Contains(
            presentation.Sections,
            section => section.Title == "Backup"
                && section.Description == "Export settings locally, or choose specific categories when importing a backup. API keys are never included.");
        Assert.Contains(
            presentation.Sections,
            section => section.Title == "Diagnostics"
                && section.Description == "Export local logs for troubleshooting without sending telemetry.");
    }
}
