using VoiceInk.Windows.Core.PowerMode;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Text;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.PowerMode;

public sealed class PowerModeMatcherTests
{
    [Fact]
    public void Resolve_MatchesEnabledProcessAndTitleRulesCaseInsensitively()
    {
        var promptId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var settings = BaseSettings() with
        {
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Notes",
                    Emoji = "N",
                    ProcessNamePattern = "notepad",
                    WindowTitlePattern = "meeting",
                    ModelPathOverride = "C:\\Models\\notes.bin",
                    LanguageOverride = "en",
                    IsEnhancementEnabledOverride = true,
                    SelectedEnhancementPromptIdOverride = promptId,
                    AppendTrailingSpaceOverride = true,
                    RemoveFillerWordsOverride = false,
                    PunctuationCleanupModeOverride = PunctuationCleanupMode.RemoveTrailingPeriod,
                    LowercaseTranscriptionOverride = true
                }
            ]
        };

        var resolution = PowerModeMatcher.Resolve(
            settings,
            new PowerModeTarget("NOTEPAD.EXE", "Team Meeting Notes", 100));

        Assert.NotNull(resolution.Rule);
        Assert.Equal("Notes", resolution.Rule.Name);
        Assert.Equal("N", resolution.PowerModeEmoji);
        Assert.Equal("C:\\Models\\notes.bin", resolution.EffectiveSettings.ModelPath);
        Assert.Equal("en", resolution.EffectiveSettings.Language);
        Assert.True(resolution.EffectiveSettings.IsEnhancementEnabled);
        Assert.Equal(promptId, resolution.EffectiveSettings.SelectedEnhancementPromptId);
        Assert.True(resolution.EffectiveSettings.AppendTrailingSpace);
        Assert.False(resolution.EffectiveSettings.RemoveFillerWords);
        Assert.Equal(PunctuationCleanupMode.RemoveTrailingPeriod, resolution.EffectiveSettings.PunctuationCleanupMode);
        Assert.True(resolution.EffectiveSettings.LowercaseTranscription);
    }

    [Fact]
    public void Resolve_IgnoresDisabledRulesAndUsesFirstEnabledMatch()
    {
        var settings = BaseSettings() with
        {
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Disabled",
                    Emoji = "D",
                    IsEnabled = false,
                    ProcessNamePattern = "code",
                    ModelPathOverride = "disabled.bin"
                },
                new PowerModeRule
                {
                    Name = "Code",
                    Emoji = "C",
                    ProcessNamePattern = "code",
                    ModelPathOverride = "first.bin"
                },
                new PowerModeRule
                {
                    Name = "Later Code",
                    Emoji = "L",
                    ProcessNamePattern = "code",
                    ModelPathOverride = "second.bin"
                }
            ]
        };

        var resolution = PowerModeMatcher.Resolve(
            settings,
            new PowerModeTarget("Code", "Program.cs", 101));

        Assert.NotNull(resolution.Rule);
        Assert.Equal("Code", resolution.Rule.Name);
        Assert.Equal("first.bin", resolution.EffectiveSettings.ModelPath);
    }

    [Fact]
    public void Resolve_UsesDefaultOnlyWhenNoSpecificRuleMatches()
    {
        var settings = BaseSettings() with
        {
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Browser",
                    Emoji = "B",
                    ProcessNamePattern = "msedge",
                    ModelPathOverride = "browser.bin"
                },
                new PowerModeRule
                {
                    Name = "Default",
                    Emoji = "*",
                    IsDefault = true,
                    ModelPathOverride = "default.bin"
                }
            ]
        };

        var unmatched = PowerModeMatcher.Resolve(
            settings,
            new PowerModeTarget("notepad", "Untitled", 102));
        var matched = PowerModeMatcher.Resolve(
            settings,
            new PowerModeTarget("msedge", "Docs", 103));

        Assert.Equal("Default", unmatched.Rule?.Name);
        Assert.Equal("default.bin", unmatched.EffectiveSettings.ModelPath);
        Assert.Equal("Browser", matched.Rule?.Name);
        Assert.Equal("browser.bin", matched.EffectiveSettings.ModelPath);
    }

    [Fact]
    public void Resolve_PrefersExplicitSelectedEnabledRuleOverTargetAndDefaultRules()
    {
        var selectedRuleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var settings = BaseSettings() with
        {
            SelectedPowerModeRuleId = selectedRuleId,
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Id = selectedRuleId,
                    Name = "Manual",
                    Emoji = "M",
                    ModelPathOverride = "manual.bin"
                },
                new PowerModeRule
                {
                    Name = "Target",
                    Emoji = "T",
                    ProcessNamePattern = "code",
                    ModelPathOverride = "target.bin"
                },
                new PowerModeRule
                {
                    Name = "Default",
                    Emoji = "*",
                    IsDefault = true,
                    ModelPathOverride = "default.bin"
                }
            ]
        };

        var resolution = PowerModeMatcher.Resolve(settings, new PowerModeTarget("code", "Program.cs", 500));

        Assert.Equal("Manual", resolution.Rule?.Name);
        Assert.Equal("M", resolution.PowerModeEmoji);
        Assert.Equal("manual.bin", resolution.EffectiveSettings.ModelPath);
    }

    [Fact]
    public void Resolve_IgnoresMissingOrDisabledExplicitRuleAndFallsBackToTargetMatch()
    {
        var disabledRuleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var settings = BaseSettings() with
        {
            SelectedPowerModeRuleId = disabledRuleId,
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Id = disabledRuleId,
                    Name = "Disabled",
                    IsEnabled = false,
                    ModelPathOverride = "disabled.bin"
                },
                new PowerModeRule
                {
                    Name = "Target",
                    Emoji = "T",
                    ProcessNamePattern = "code",
                    ModelPathOverride = "target.bin"
                }
            ]
        };

        var resolution = PowerModeMatcher.Resolve(settings, new PowerModeTarget("code", "Program.cs", 501));

        Assert.Equal("Target", resolution.Rule?.Name);
        Assert.Equal("target.bin", resolution.EffectiveSettings.ModelPath);
    }

    [Fact]
    public void Resolve_KeepsBaseSettingsWhenNoRuleMatchesOrOverrideIsBlank()
    {
        var settings = BaseSettings() with
        {
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Chat",
                    Emoji = "C",
                    ProcessNamePattern = "teams",
                    ModelPathOverride = " ",
                    LanguageOverride = "",
                    IsEnhancementEnabledOverride = null
                }
            ]
        };

        var resolution = PowerModeMatcher.Resolve(
            settings,
            new PowerModeTarget("teams", "Chat", 104));

        Assert.Equal("C:\\Models\\base.bin", settings.ModelPath);
        Assert.Equal("auto", settings.Language);
        Assert.Equal("C:\\Models\\base.bin", resolution.EffectiveSettings.ModelPath);
        Assert.Equal("auto", resolution.EffectiveSettings.Language);
        Assert.False(resolution.EffectiveSettings.IsEnhancementEnabled);
    }

    [Fact]
    public void Resolve_WithMissingTargetFallsBackToBaseSettings()
    {
        var settings = BaseSettings() with
        {
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Default",
                    Emoji = "*",
                    IsDefault = true,
                    ModelPathOverride = "default.bin"
                }
            ]
        };

        var resolution = PowerModeMatcher.Resolve(settings, target: null);

        Assert.Null(resolution.Rule);
        Assert.Equal(settings, resolution.EffectiveSettings);
    }

    private static AppSettings BaseSettings() =>
        new()
        {
            ModelPath = "C:\\Models\\base.bin",
            Language = "auto",
            IsEnhancementEnabled = false,
            RemoveFillerWords = true,
            PunctuationCleanupMode = PunctuationCleanupMode.Keep,
            LowercaseTranscription = false
        };
}
