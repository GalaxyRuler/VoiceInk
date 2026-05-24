using VoiceInk.Windows.Core.Text;

namespace VoiceInk.Windows.Core.PowerMode;

public sealed record PowerModeRule
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = "New Power Mode";
    public string Emoji { get; init; } = "*";
    public bool IsEnabled { get; init; } = true;
    public bool IsDefault { get; init; }
    public PowerModeMatchKind MatchKind { get; init; } = PowerModeMatchKind.Contains;
    public string ProcessNamePattern { get; init; } = string.Empty;
    public string WindowTitlePattern { get; init; } = string.Empty;
    public string? ModelPathOverride { get; init; }
    public string? LanguageOverride { get; init; }
    public bool? IsEnhancementEnabledOverride { get; init; }
    public Guid? SelectedEnhancementPromptIdOverride { get; init; }
    public bool? AppendTrailingSpaceOverride { get; init; }
    public bool? RemoveFillerWordsOverride { get; init; }
    public PunctuationCleanupMode? PunctuationCleanupModeOverride { get; init; }
    public bool? LowercaseTranscriptionOverride { get; init; }
}
