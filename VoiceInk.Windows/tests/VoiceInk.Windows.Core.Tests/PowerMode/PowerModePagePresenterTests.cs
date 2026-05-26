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
        Assert.Equal(
            "Switch modes from the recorder, tray, global shortcuts, or direct rule shortcuts. Enabled rules keep their list order for number-slot selection.",
            presentation.ManualSwitchingSummary);
        Assert.Equal("2 Power Modes (1 enabled, 1 disabled)", presentation.CountLabel);
        Assert.False(presentation.IsEmpty);
        Assert.Equal(string.Empty, presentation.EmptyTitle);
        Assert.Equal(string.Empty, presentation.EmptyDescription);
        Assert.Collection(
            presentation.RuleRows,
            row =>
            {
                Assert.Equal("Writing", row.Title);
                Assert.Equal("No target", row.TargetSummary);
                Assert.Equal("No overrides", row.OverrideSummary);
                Assert.Equal("No direct shortcut", row.ShortcutSummary);
                Assert.Equal("Enabled", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Archive", row.Title);
                Assert.Equal("No target", row.TargetSummary);
                Assert.Equal("No overrides", row.OverrideSummary);
                Assert.Equal("No direct shortcut", row.ShortcutSummary);
                Assert.Equal("Disabled", row.StatusBadge);
            });
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
        Assert.Empty(presentation.RuleRows);
    }

    [Fact]
    public void Present_BuildsRuleRowsWithTargetsOverridesAndShortcuts()
    {
        var presentation = PowerModePagePresenter.Present(
        [
            new PowerModeRule
            {
                Name = "Writing",
                Emoji = "W",
                ProcessNamePattern = "WINWORD",
                BrowserUrlPattern = "docs.example.com",
                ModelPathOverride = @"C:\Models\ggml-base.en.bin",
                LanguageOverride = "en",
                IsEnhancementEnabledOverride = true,
                AppendTrailingSpaceOverride = false,
                AutoSendKey = PowerModeAutoSendKey.Enter,
                Shortcut = "Ctrl+Alt+1"
            }
        ]);

        var row = Assert.Single(presentation.RuleRows);
        Assert.Equal("W Writing", row.Title);
        Assert.Equal("Process: WINWORD; URL: docs.example.com", row.TargetSummary);
        Assert.Equal("5 overrides: model, language, enhancement, trailing space, auto-send", row.OverrideSummary);
        Assert.Equal("Shortcut: Ctrl+Alt+1", row.ShortcutSummary);
        Assert.Equal("Enabled", row.StatusBadge);
    }
}
