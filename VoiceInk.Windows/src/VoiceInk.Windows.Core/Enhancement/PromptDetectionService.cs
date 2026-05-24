namespace VoiceInk.Windows.Core.Enhancement;

public static class PromptDetectionService
{
    public static PromptDetectionResult Analyze(
        string text,
        IReadOnlyList<EnhancementPrompt> prompts,
        bool isEnhancementEnabled,
        Guid? selectedPromptId)
    {
        foreach (var prompt in prompts)
        {
            var triggerWords = prompt.TriggerWords
                .Select(word => word.Trim())
                .Where(word => word.Length > 0)
                .OrderByDescending(word => word.Length)
                .ToArray();

            foreach (var triggerWord in triggerWords)
            {
                if (DetectAndStripTrigger(text, triggerWord) is { } processedText)
                {
                    return new PromptDetectionResult(
                        ShouldEnableEnhancement: true,
                        SelectedPromptId: prompt.Id,
                        ProcessedText: processedText,
                        DetectedTriggerWord: triggerWord,
                        OriginalEnhancementEnabled: isEnhancementEnabled,
                        OriginalPromptId: selectedPromptId);
                }
            }
        }

        return new PromptDetectionResult(
            ShouldEnableEnhancement: false,
            SelectedPromptId: selectedPromptId,
            ProcessedText: text,
            DetectedTriggerWord: null,
            OriginalEnhancementEnabled: isEnhancementEnabled,
            OriginalPromptId: selectedPromptId);
    }

    private static string? DetectAndStripTrigger(string text, string triggerWord)
    {
        if (StripTrailingTriggerWord(text, triggerWord) is { } afterTrailing)
        {
            return NormalizeSentenceStart(
                StripLeadingTriggerWord(afterTrailing, triggerWord) ?? afterTrailing);
        }

        if (StripLeadingTriggerWord(text, triggerWord) is { } afterLeading)
        {
            return NormalizeSentenceStart(
                StripTrailingTriggerWord(afterLeading, triggerWord) ?? afterLeading);
        }

        return null;
    }

    private static string? StripLeadingTriggerWord(string text, string triggerWord)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith(triggerWord, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (trimmed.Length > triggerWord.Length
            && char.IsLetterOrDigit(trimmed[triggerWord.Length]))
        {
            return null;
        }

        var remaining = trimmed.Length == triggerWord.Length
            ? string.Empty
            : trimmed[triggerWord.Length..];
        return TrimBoundaryPunctuation(remaining);
    }

    private static string? StripTrailingTriggerWord(string text, string triggerWord)
    {
        var trimmed = text.Trim().TrimEnd(',', '.', '!', '?', ';', ':');
        if (!trimmed.EndsWith(triggerWord, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var triggerStart = trimmed.Length - triggerWord.Length;
        if (triggerStart > 0 && char.IsLetterOrDigit(trimmed[triggerStart - 1]))
        {
            return null;
        }

        var remaining = triggerStart == 0 ? string.Empty : trimmed[..triggerStart];
        return TrimBoundaryPunctuation(remaining);
    }

    private static string TrimBoundaryPunctuation(string value) =>
        value.Trim().Trim(',', '.', '!', '?', ';', ':').Trim();

    private static string NormalizeSentenceStart(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        return char.ToUpperInvariant(trimmed[0]) + trimmed[1..];
    }
}

public sealed record PromptDetectionResult(
    bool ShouldEnableEnhancement,
    Guid? SelectedPromptId,
    string ProcessedText,
    string? DetectedTriggerWord,
    bool OriginalEnhancementEnabled,
    Guid? OriginalPromptId);
