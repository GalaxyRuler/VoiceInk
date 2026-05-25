using VoiceInk.Windows.Core.PowerMode;
using VoiceInk.Windows.Core.Settings;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.PowerMode;

public sealed class PowerModeShortcutCyclerTests
{
    private static readonly Guid FirstRuleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SecondRuleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid DisabledRuleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public void NextRuleId_SelectsFirstEnabledRuleFromAutomatic()
    {
        var nextRuleId = PowerModeShortcutCycler.NextRuleId(
            new AppSettings(),
            Rules());

        Assert.Equal(FirstRuleId, nextRuleId);
    }

    [Fact]
    public void NextRuleId_CyclesThroughEnabledRulesThenAutomatic()
    {
        var nextFromFirst = PowerModeShortcutCycler.NextRuleId(
            new AppSettings { SelectedPowerModeRuleId = FirstRuleId },
            Rules());
        var nextFromSecond = PowerModeShortcutCycler.NextRuleId(
            new AppSettings { SelectedPowerModeRuleId = SecondRuleId },
            Rules());

        Assert.Equal(SecondRuleId, nextFromFirst);
        Assert.Null(nextFromSecond);
    }

    [Fact]
    public void NextRuleId_TreatsDisabledOrMissingSelectionAsAutomatic()
    {
        var nextFromDisabled = PowerModeShortcutCycler.NextRuleId(
            new AppSettings { SelectedPowerModeRuleId = DisabledRuleId },
            Rules());
        var nextFromMissing = PowerModeShortcutCycler.NextRuleId(
            new AppSettings { SelectedPowerModeRuleId = Guid.Parse("99999999-9999-9999-9999-999999999999") },
            Rules());

        Assert.Equal(FirstRuleId, nextFromDisabled);
        Assert.Equal(FirstRuleId, nextFromMissing);
    }

    [Fact]
    public void NextRuleId_ReturnsAutomaticWhenNoRulesAreEnabled()
    {
        var nextRuleId = PowerModeShortcutCycler.NextRuleId(
            new AppSettings { SelectedPowerModeRuleId = DisabledRuleId },
            [new PowerModeRule { Id = DisabledRuleId, IsEnabled = false }]);

        Assert.Null(nextRuleId);
    }

    private static PowerModeRule[] Rules() =>
    [
        new PowerModeRule { Id = FirstRuleId, Name = "First", IsEnabled = true },
        new PowerModeRule { Id = DisabledRuleId, Name = "Disabled", IsEnabled = false },
        new PowerModeRule { Id = SecondRuleId, Name = "Second", IsEnabled = true }
    ];
}
