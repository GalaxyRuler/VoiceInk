using VoiceInk.Windows.Core.PowerMode;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.PowerMode;

public sealed class PowerModeRuleValidatorTests
{
    [Fact]
    public void ValidateRule_EnabledSpecificRuleRequiresAtLeastOneMatchField()
    {
        var result = PowerModeRuleValidator.Validate(new PowerModeRule
        {
            Name = "Empty",
            IsEnabled = true,
            IsDefault = false
        });

        Assert.False(result.IsValid);
        Assert.Contains(
            "Enabled Power Mode rules need a process, window title, or browser URL match.",
            result.Errors);
    }

    [Fact]
    public void ValidateRule_DefaultRuleAllowsEmptyMatchFields()
    {
        var result = PowerModeRuleValidator.Validate(new PowerModeRule
        {
            Name = "Fallback",
            IsEnabled = true,
            IsDefault = true
        });

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateRule_DefaultRuleWarnsThatMatchFieldsAreIgnored()
    {
        var result = PowerModeRuleValidator.Validate(new PowerModeRule
        {
            Name = "Fallback",
            IsEnabled = true,
            IsDefault = true,
            ProcessNamePattern = "teams"
        });

        Assert.True(result.IsValid);
        Assert.Contains(
            "Default Power Mode rules ignore process, window title, and browser URL matches.",
            result.Warnings);
    }

    [Fact]
    public void ValidateRules_ReportsMultipleEnabledDefaultRules()
    {
        var result = PowerModeRuleValidator.Validate(
        [
            new PowerModeRule { Name = "Default A", IsEnabled = true, IsDefault = true },
            new PowerModeRule { Name = "Default B", IsEnabled = true, IsDefault = true }
        ]);

        Assert.False(result.IsValid);
        Assert.Contains(
            "Only one enabled Power Mode rule can be the default fallback.",
            result.Errors);
    }

    [Fact]
    public void ValidateRules_WarnsAboutDuplicateEnabledSpecificMatchers()
    {
        var result = PowerModeRuleValidator.Validate(
        [
            new PowerModeRule { Name = "Teams A", IsEnabled = true, ProcessNamePattern = "teams", WindowTitlePattern = "chat" },
            new PowerModeRule { Name = "Teams B", IsEnabled = true, ProcessNamePattern = " TEAMS ", WindowTitlePattern = "Chat" }
        ]);

        Assert.True(result.IsValid);
        Assert.Contains(
            "Teams B duplicates the match fields from Teams A; only the first matching rule will apply.",
            result.Warnings);
    }
}
