using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.PowerMode;
using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Recorder;

public sealed record FloatingRecorderPromptChoice(
    Guid Id,
    string Title,
    bool IsSelected,
    bool IsDisabled);

public sealed record FloatingRecorderPowerModeChoice(
    Guid? Id,
    string Title,
    string Emoji,
    bool IsSelected);

public sealed record FloatingRecorderControlState(
    string PromptHeaderTitle,
    string PromptTitle,
    bool IsEnhancementEnabled,
    bool CanToggleEnhancement,
    bool CanOpenPromptControls,
    IReadOnlyList<FloatingRecorderPromptChoice> PromptChoices,
    string PowerModeHeaderTitle,
    string PowerModeTitle,
    string PowerModeEmoji,
    string PowerModeButtonLabel,
    string PowerModeEmptyTitle,
    bool CanOpenPowerModeControls,
    IReadOnlyList<FloatingRecorderPowerModeChoice> PowerModeChoices);

public static class FloatingRecorderControlPresenter
{
    private const string AutomaticPowerModeTitle = "Auto";
    private const string AutomaticPowerModeEmoji = "*";

    public static FloatingRecorderControlState FromSettings(
        AppSettings settings,
        IReadOnlyList<EnhancementPrompt> prompts,
        IReadOnlyList<PowerModeRule> powerModeRules)
    {
        var availablePrompts = prompts.Count > 0
            ? prompts
            : EnhancementPromptCatalog.CreateDefaultPrompts();
        var selectedPromptId = EnhancementPromptLibrary.ResolveSelectedPromptId(
            settings.SelectedEnhancementPromptId,
            availablePrompts);
        var selectedPrompt = availablePrompts.FirstOrDefault(prompt => prompt.Id == selectedPromptId)
            ?? availablePrompts[0];
        var promptChoices = availablePrompts
            .Select(prompt => new FloatingRecorderPromptChoice(
                prompt.Id,
                prompt.Title,
                prompt.Id == selectedPromptId,
                !settings.IsEnhancementEnabled))
            .ToArray();

        var enabledPowerModeRules = settings.IsPowerModeEnabled
            ? powerModeRules
                .Where(rule => rule.IsEnabled)
                .ToArray()
            : [];
        var selectedRule = settings.SelectedPowerModeRuleId is { } selectedRuleId
            ? enabledPowerModeRules.FirstOrDefault(rule => rule.Id == selectedRuleId)
            : null;
        var powerModeChoices = new[]
            {
                new FloatingRecorderPowerModeChoice(
                    null,
                    AutomaticPowerModeTitle,
                    AutomaticPowerModeEmoji,
                    selectedRule is null)
            }
            .Concat(enabledPowerModeRules.Select(rule => new FloatingRecorderPowerModeChoice(
                rule.Id,
                string.IsNullOrWhiteSpace(rule.Name) ? "Power Mode" : rule.Name.Trim(),
                string.IsNullOrWhiteSpace(rule.Emoji) ? AutomaticPowerModeEmoji : rule.Emoji.Trim(),
                selectedRule?.Id == rule.Id)))
            .ToArray();

        return new FloatingRecorderControlState(
            "AI Enhancement",
            selectedPrompt.Title,
            settings.IsEnhancementEnabled,
            CanToggleEnhancement: true,
            CanOpenPromptControls: promptChoices.Length > 0,
            promptChoices,
            "Select Power Mode",
            selectedRule is null ? AutomaticPowerModeTitle : PowerModeTitle(selectedRule),
            selectedRule is null ? AutomaticPowerModeEmoji : PowerModeEmoji(selectedRule),
            PowerModeButtonLabel(
                selectedRule is null ? AutomaticPowerModeTitle : PowerModeTitle(selectedRule),
                selectedRule is null ? AutomaticPowerModeEmoji : PowerModeEmoji(selectedRule)),
            "No Power Modes Available",
            CanOpenPowerModeControls: settings.IsPowerModeEnabled && enabledPowerModeRules.Length > 0,
            powerModeChoices);
    }

    private static string PowerModeTitle(PowerModeRule rule) =>
        string.IsNullOrWhiteSpace(rule.Name) ? "Power Mode" : rule.Name.Trim();

    private static string PowerModeEmoji(PowerModeRule rule) =>
        string.IsNullOrWhiteSpace(rule.Emoji) ? AutomaticPowerModeEmoji : rule.Emoji.Trim();

    private static string PowerModeButtonLabel(string title, string emoji) =>
        string.IsNullOrWhiteSpace(emoji)
            ? title
            : $"{emoji} {title}";
}
