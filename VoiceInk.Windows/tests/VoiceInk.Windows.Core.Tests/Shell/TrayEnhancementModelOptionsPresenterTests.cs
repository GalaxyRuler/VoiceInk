using VoiceInk.Windows.Core.Shell;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Shell;

public sealed class TrayEnhancementModelOptionsPresenterTests
{
    [Fact]
    public void FromModels_ChecksSelectedKnownModel()
    {
        var options = TrayEnhancementModelOptionsPresenter.FromModels(
            ["gpt-4o-mini", "gpt-4o"],
            "gpt-4o");

        Assert.Collection(
            options,
            option => Assert.Equal(("gpt-4o-mini", "gpt-4o-mini", false, true), OptionTuple(option)),
            option => Assert.Equal(("gpt-4o", "gpt-4o", true, true), OptionTuple(option)));
    }

    [Fact]
    public void FromModels_IncludesSelectedCustomModel()
    {
        var options = TrayEnhancementModelOptionsPresenter.FromModels(
            ["gpt-4o-mini"],
            "custom-model");

        Assert.Collection(
            options,
            option => Assert.Equal(("custom-model", "Custom: custom-model", true, true), OptionTuple(option)),
            option => Assert.Equal(("gpt-4o-mini", "gpt-4o-mini", false, true), OptionTuple(option)));
    }

    [Fact]
    public void FromModels_DeduplicatesAndTrimsChoices()
    {
        var options = TrayEnhancementModelOptionsPresenter.FromModels(
            [" gpt-4o-mini ", "GPT-4O-MINI", "", "gpt-4o"],
            "gpt-4o-mini");

        Assert.Collection(
            options,
            option => Assert.Equal(("gpt-4o-mini", "gpt-4o-mini", true, true), OptionTuple(option)),
            option => Assert.Equal(("gpt-4o", "gpt-4o", false, true), OptionTuple(option)));
    }

    [Fact]
    public void FromModels_ReturnsEmptyWhenNoChoicesAndNoSelectedModel()
    {
        var options = TrayEnhancementModelOptionsPresenter.FromModels([], " ");

        Assert.Empty(options);
    }

    private static (string Id, string Label, bool IsChecked, bool IsEnabled) OptionTuple(TrayMenuOption option) =>
        (option.Id, option.Label, option.IsChecked, option.IsEnabled);
}
