namespace VoiceInk.Windows.Core.Enhancement;

public sealed record TextEnhancementPipelineResult(
    string OriginalText,
    string FinalText,
    bool AttemptedEnhancement,
    string? EnhancedText = null,
    string? PromptName = null,
    string? EnhancementProviderName = null,
    string? EnhancementModelName = null,
    TimeSpan? EnhancementDuration = null,
    string? WarningMessage = null,
    string? SystemMessage = null,
    string? UserMessage = null);
