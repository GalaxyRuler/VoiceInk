using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.PowerMode;
using VoiceInk.Windows.Core.Recorder;
using VoiceInk.Windows.Core.Settings;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Recorder;

public sealed class FloatingRecorderControlPresenterTests
{
    [Fact]
    public void FromSettings_ResolvesPromptSelectionAndKeepsPromptControlAvailableWhenEnhancementIsOff()
    {
        var prompts = EnhancementPromptCatalog.CreateDefaultPrompts();
        var selectedPrompt = prompts.Single(prompt => prompt.Title == "Chat");
        var settings = new AppSettings
        {
            IsEnhancementEnabled = false,
            SelectedEnhancementPromptId = selectedPrompt.Id
        };

        var state = FloatingRecorderControlPresenter.FromSettings(settings, prompts, []);

        Assert.True(state.CanOpenPromptControls);
        Assert.Equal("AI Enhancement", state.PromptHeaderTitle);
        Assert.True(state.CanToggleEnhancement);
        Assert.False(state.IsEnhancementEnabled);
        Assert.Equal("Chat", state.PromptTitle);
        Assert.Equal(prompts.Count, state.PromptChoices.Count);
        Assert.All(state.PromptChoices, choice => Assert.True(choice.IsDisabled));
        Assert.Equal(selectedPrompt.Id, Assert.Single(state.PromptChoices, choice => choice.IsSelected).Id);
    }

    [Fact]
    public void FromSettings_FallsBackToDefaultPromptWhenSelectedPromptIsMissing()
    {
        var prompts = EnhancementPromptCatalog.CreateDefaultPrompts();

        var state = FloatingRecorderControlPresenter.FromSettings(
            new AppSettings
            {
                IsEnhancementEnabled = true,
                SelectedEnhancementPromptId = Guid.Parse("99999999-9999-9999-9999-999999999999")
            },
            prompts,
            []);

        Assert.Equal("Default", state.PromptTitle);
        Assert.Equal(EnhancementPromptCatalog.DefaultPromptId, Assert.Single(state.PromptChoices, choice => choice.IsSelected).Id);
        Assert.All(state.PromptChoices, choice => Assert.False(choice.IsDisabled));
    }

    [Fact]
    public void FromSettings_BuildsPowerModeChoicesWithAutomaticAndEnabledRulesOnly()
    {
        var selectedRuleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var disabledRuleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var state = FloatingRecorderControlPresenter.FromSettings(
            new AppSettings { SelectedPowerModeRuleId = selectedRuleId },
            EnhancementPromptCatalog.CreateDefaultPrompts(),
            [
                new PowerModeRule
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Notes",
                    Emoji = "N",
                    IsEnabled = true
                },
                new PowerModeRule
                {
                    Id = selectedRuleId,
                    Name = "Terminal",
                    Emoji = ">",
                    IsEnabled = true
                },
                new PowerModeRule
                {
                    Id = disabledRuleId,
                    Name = "Disabled",
                    Emoji = "D",
                    IsEnabled = false
                }
            ]);

        Assert.True(state.CanOpenPowerModeControls);
        Assert.Equal("Select Power Mode", state.PowerModeHeaderTitle);
        Assert.Equal("No Power Modes Available", state.PowerModeEmptyTitle);
        Assert.Equal("Terminal", state.PowerModeTitle);
        Assert.Equal(">", state.PowerModeEmoji);
        Assert.Equal("> Terminal", state.PowerModeButtonLabel);
        Assert.Equal(new Guid?[] { null, Guid.Parse("11111111-1111-1111-1111-111111111111"), selectedRuleId }, state.PowerModeChoices.Select(choice => choice.Id).ToArray());
        Assert.Equal(selectedRuleId, Assert.Single(state.PowerModeChoices, choice => choice.IsSelected).Id);
    }

    [Fact]
    public void FromSettings_DisablesPowerModeControlWhenNoEnabledRulesExist()
    {
        var state = FloatingRecorderControlPresenter.FromSettings(
            new AppSettings { SelectedPowerModeRuleId = Guid.Parse("99999999-9999-9999-9999-999999999999") },
            EnhancementPromptCatalog.CreateDefaultPrompts(),
            [
                new PowerModeRule
                {
                    Name = "Disabled",
                    IsEnabled = false
                }
            ]);

        Assert.False(state.CanOpenPowerModeControls);
        Assert.Equal("Auto", state.PowerModeTitle);
        Assert.Equal("*", state.PowerModeEmoji);
        Assert.Equal("* Auto", state.PowerModeButtonLabel);
        var automatic = Assert.Single(state.PowerModeChoices);
        Assert.Null(automatic.Id);
        Assert.True(automatic.IsSelected);
    }
}
