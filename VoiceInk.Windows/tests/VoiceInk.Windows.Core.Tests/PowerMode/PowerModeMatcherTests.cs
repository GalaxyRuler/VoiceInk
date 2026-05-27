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
                    IsTextFormattingEnabledOverride = true,
                    PunctuationCleanupModeOverride = PunctuationCleanupMode.RemoveTrailingPeriod,
                    LowercaseTranscriptionOverride = true,
                    AutoSendKey = PowerModeAutoSendKey.ShiftEnter
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
        Assert.True(resolution.EffectiveSettings.IsTextFormattingEnabled);
        Assert.Equal(PunctuationCleanupMode.RemoveTrailingPeriod, resolution.EffectiveSettings.PunctuationCleanupMode);
        Assert.True(resolution.EffectiveSettings.LowercaseTranscription);
        Assert.Equal(PowerModeAutoSendKey.ShiftEnter, resolution.AutoSendKey);
    }

    [Fact]
    public void Resolve_ExposesNoAutoSendWhenNoRuleMatches()
    {
        var resolution = PowerModeMatcher.Resolve(BaseSettings(), target: null);

        Assert.Equal(PowerModeAutoSendKey.None, resolution.AutoSendKey);
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
    public void Resolve_MatchesBrowserUrlRulesAgainstSanitizedUrl()
    {
        var settings = BaseSettings() with
        {
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Docs Site",
                    Emoji = "D",
                    BrowserUrlPattern = "example.com/docs",
                    ModelPathOverride = "docs.bin"
                },
                new PowerModeRule
                {
                    Name = "Secret Query",
                    Emoji = "S",
                    BrowserUrlPattern = "token=secret",
                    ModelPathOverride = "secret.bin"
                }
            ]
        };

        var resolution = PowerModeMatcher.Resolve(
            settings,
            new PowerModeTarget(
                "msedge",
                "Docs",
                103,
                BrowserUrl: "https://example.com/docs?token=secret#part"));

        Assert.Equal("Docs Site", resolution.Rule?.Name);
        Assert.Equal("docs.bin", resolution.EffectiveSettings.ModelPath);
    }

    [Fact]
    public void Resolve_MatchesAnySemicolonSeparatedProcessPattern()
    {
        var settings = BaseSettings() with
        {
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Writing Apps",
                    ProcessNamePattern = "winword; notepad; obsidian",
                    ModelPathOverride = "writing.bin"
                }
            ]
        };

        var resolution = PowerModeMatcher.Resolve(
            settings,
            new PowerModeTarget("Obsidian", "Daily Note", 103));

        Assert.Equal("Writing Apps", resolution.Rule?.Name);
        Assert.Equal("writing.bin", resolution.EffectiveSettings.ModelPath);
    }

    [Fact]
    public void Resolve_MatchesAnyNewlineSeparatedBrowserUrlPatternAfterSanitizing()
    {
        var settings = BaseSettings() with
        {
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Docs Sites",
                    BrowserUrlPattern = "internal.example/docs\r\nhttps://example.com/write",
                    ModelPathOverride = "docs.bin"
                }
            ]
        };

        var resolution = PowerModeMatcher.Resolve(
            settings,
            new PowerModeTarget(
                "msedge",
                "Docs",
                103,
                BrowserUrl: "https://example.com/write?token=secret#top"));

        Assert.Equal("Docs Sites", resolution.Rule?.Name);
        Assert.Equal("docs.bin", resolution.EffectiveSettings.ModelPath);
    }

    [Fact]
    public void Resolve_DoesNotMatchBrowserUrlRulesWhenTargetHasNoUrl()
    {
        var settings = BaseSettings() with
        {
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Docs Site",
                    BrowserUrlPattern = "example.com/docs",
                    ModelPathOverride = "docs.bin"
                },
                new PowerModeRule
                {
                    Name = "Default",
                    IsDefault = true,
                    ModelPathOverride = "default.bin"
                }
            ]
        };

        var resolution = PowerModeMatcher.Resolve(
            settings,
            new PowerModeTarget("msedge", "Docs", 103));

        Assert.Equal("Default", resolution.Rule?.Name);
        Assert.Equal("default.bin", resolution.EffectiveSettings.ModelPath);
    }

    [Fact]
    public void Resolve_CombinesProcessTitleAndBrowserUrlPatterns()
    {
        var settings = BaseSettings() with
        {
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Wrong Site",
                    ProcessNamePattern = "msedge",
                    WindowTitlePattern = "Docs",
                    BrowserUrlPattern = "other.example",
                    ModelPathOverride = "wrong.bin"
                },
                new PowerModeRule
                {
                    Name = "Edge Docs",
                    ProcessNamePattern = "msedge",
                    WindowTitlePattern = "Docs",
                    BrowserUrlPattern = "https://example.com/docs",
                    ModelPathOverride = "edge-docs.bin"
                }
            ]
        };

        var resolution = PowerModeMatcher.Resolve(
            settings,
            new PowerModeTarget(
                "MSEDGE",
                "Docs - Microsoft Edge",
                103,
                BrowserUrl: "https://example.com/docs"));

        Assert.Equal("Edge Docs", resolution.Rule?.Name);
        Assert.Equal("edge-docs.bin", resolution.EffectiveSettings.ModelPath);
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

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void Resolve_AppliesOcrContextOverrideWithoutChangingBaseSettings(
        bool baseOcrContext,
        bool ruleOcrContext)
    {
        var settings = BaseSettings() with
        {
            UseOcrContext = baseOcrContext,
            PowerModeRules =
            [
                new PowerModeRule
                {
                    Name = "Visual Context",
                    ProcessNamePattern = "code",
                    UseOcrContextOverride = ruleOcrContext
                }
            ]
        };

        var resolution = PowerModeMatcher.Resolve(
            settings,
            new PowerModeTarget("code", "Program.cs", 105));

        Assert.Equal(ruleOcrContext, resolution.EffectiveSettings.UseOcrContext);
        Assert.Equal(baseOcrContext, settings.UseOcrContext);
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
