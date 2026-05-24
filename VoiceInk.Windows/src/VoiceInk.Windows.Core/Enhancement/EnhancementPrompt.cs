namespace VoiceInk.Windows.Core.Enhancement;

public sealed record EnhancementPrompt(
    Guid Id,
    string Title,
    string PromptText,
    string Icon,
    string? Description,
    bool IsPredefined,
    IReadOnlyList<string> TriggerWords,
    bool UseSystemInstructions);
