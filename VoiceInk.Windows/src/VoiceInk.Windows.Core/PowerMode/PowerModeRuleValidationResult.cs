namespace VoiceInk.Windows.Core.PowerMode;

public sealed record PowerModeRuleValidationResult(
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings)
{
    public bool IsValid => Errors.Count == 0;

    public static PowerModeRuleValidationResult Success { get; } = new([], []);
}
