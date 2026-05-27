using VoiceInk.Windows.Core.Dictionary;

namespace VoiceInk.Windows.Core.Text;

public sealed record TextPostProcessingOptions(
    bool AppendTrailingSpace = false,
    bool RemoveFillerWords = true,
    IReadOnlyList<string>? FillerWords = null,
    IReadOnlyList<WordReplacement>? WordReplacements = null,
    PunctuationCleanupMode PunctuationCleanupMode = PunctuationCleanupMode.Keep,
    bool LowercaseTranscription = false,
    bool ApplyTextFormatting = false);
