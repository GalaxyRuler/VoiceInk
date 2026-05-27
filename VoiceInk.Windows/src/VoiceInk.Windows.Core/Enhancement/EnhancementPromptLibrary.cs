namespace VoiceInk.Windows.Core.Enhancement;

public static class EnhancementPromptLibrary
{
    private const string DefaultIcon = "doc.text.fill";

    public static IReadOnlyList<EnhancementPrompt> BuildPrompts(
        IReadOnlyList<EnhancementPrompt> persistedPrompts)
    {
        var defaults = EnhancementPromptCatalog.CreateDefaultPrompts();
        var byId = persistedPrompts
            .GroupBy(prompt => prompt.Id)
            .ToDictionary(group => group.Key, group => group.First());
        var prompts = new List<EnhancementPrompt>(defaults.Count + persistedPrompts.Count);

        foreach (var defaultPrompt in defaults)
        {
            prompts.Add(byId.TryGetValue(defaultPrompt.Id, out var persisted)
                ? defaultPrompt with { TriggerWords = NormalizeTriggerWords(persisted.TriggerWords) }
                : defaultPrompt);
        }

        var defaultIds = defaults.Select(prompt => prompt.Id).ToHashSet();
        foreach (var prompt in persistedPrompts)
        {
            if (prompt.IsPredefined || defaultIds.Contains(prompt.Id))
            {
                continue;
            }

            if (NormalizeCustomPrompt(prompt) is { } normalized)
            {
                prompts.Add(normalized);
            }
        }

        return prompts;
    }

    public static EnhancementPrompt CreateCustomPrompt(
        Guid id,
        string title,
        string promptText,
        string icon,
        string? description,
        string triggerWordsText,
        bool useSystemInstructions)
    {
        var normalizedTitle = title.Trim();
        var normalizedPromptText = promptText.Trim();
        if (normalizedTitle.Length == 0)
        {
            throw new ArgumentException("Prompt title is required.", nameof(title));
        }

        if (normalizedPromptText.Length == 0)
        {
            throw new ArgumentException("Prompt instructions are required.", nameof(promptText));
        }

        var normalizedDescription = description?.Trim();
        if (normalizedDescription?.Length == 0)
        {
            normalizedDescription = null;
        }

        return new EnhancementPrompt(
            id,
            normalizedTitle,
            normalizedPromptText,
            string.IsNullOrWhiteSpace(icon) ? DefaultIcon : icon.Trim(),
            normalizedDescription,
            IsPredefined: false,
            TriggerWords: NormalizeTriggerWords(ParseTriggerWords(triggerWordsText)),
            useSystemInstructions);
    }

    public static IReadOnlyList<EnhancementPrompt> UpdatePrompt(
        IReadOnlyList<EnhancementPrompt> prompts,
        EnhancementPrompt updatedPrompt)
    {
        var normalized = updatedPrompt.IsPredefined
            ? NormalizePredefinedOverride(updatedPrompt)
            : NormalizeCustomPrompt(updatedPrompt)
                ?? throw new ArgumentException("Custom prompt title and instructions are required.", nameof(updatedPrompt));
        var result = prompts
            .Where(prompt => prompt.Id != normalized.Id)
            .ToList();
        var insertIndex = prompts.ToList().FindIndex(prompt => prompt.Id == normalized.Id);
        if (insertIndex >= 0 && insertIndex <= result.Count)
        {
            result.Insert(insertIndex, normalized);
        }
        else
        {
            result.Add(normalized);
        }

        return result;
    }

    public static IReadOnlyList<EnhancementPrompt> DeletePrompt(
        IReadOnlyList<EnhancementPrompt> prompts,
        Guid promptId) =>
        prompts.Any(prompt => prompt.Id == promptId && prompt.IsPredefined)
            ? prompts
            : prompts.Where(prompt => prompt.Id != promptId).ToArray();

    public static IReadOnlyList<EnhancementPrompt> MovePrompt(
        IReadOnlyList<EnhancementPrompt> prompts,
        Guid promptId,
        int offset)
    {
        if (offset == 0 || prompts.Count == 0)
        {
            return prompts.ToArray();
        }

        var customPrompts = prompts.Where(prompt => !prompt.IsPredefined).ToList();
        var customIndex = customPrompts.FindIndex(prompt => prompt.Id == promptId);
        if (customIndex < 0)
        {
            return prompts.ToArray();
        }

        var targetIndex = Math.Clamp(customIndex + offset, 0, customPrompts.Count - 1);
        if (targetIndex == customIndex)
        {
            return prompts.ToArray();
        }

        var movedPrompt = customPrompts[customIndex];
        customPrompts.RemoveAt(customIndex);
        customPrompts.Insert(targetIndex, movedPrompt);

        var nextCustomPromptIndex = 0;
        return prompts
            .Select(prompt => prompt.IsPredefined
                ? prompt
                : customPrompts[nextCustomPromptIndex++])
            .ToArray();
    }

    public static IReadOnlyList<EnhancementPrompt> PersistentPrompts(
        IReadOnlyList<EnhancementPrompt> prompts)
    {
        var defaultIds = EnhancementPromptCatalog.CreateDefaultPrompts()
            .Select(prompt => prompt.Id)
            .ToHashSet();
        return prompts
            .Where(prompt => !prompt.IsPredefined || prompt.TriggerWords.Count > 0)
            .Select(prompt => defaultIds.Contains(prompt.Id) || prompt.IsPredefined
                ? NormalizePredefinedOverride(prompt)
                : NormalizeCustomPrompt(prompt))
            .OfType<EnhancementPrompt>()
            .ToArray();
    }

    public static Guid ResolveSelectedPromptId(
        Guid? selectedPromptId,
        IReadOnlyList<EnhancementPrompt> prompts)
    {
        if (selectedPromptId is { } id && prompts.Any(prompt => prompt.Id == id))
        {
            return id;
        }

        return prompts.FirstOrDefault(prompt => prompt.Id == EnhancementPromptCatalog.DefaultPromptId)?.Id
            ?? prompts.FirstOrDefault()?.Id
            ?? EnhancementPromptCatalog.DefaultPromptId;
    }

    public static string TriggerWordsText(EnhancementPrompt prompt) =>
        string.Join(", ", NormalizeTriggerWords(prompt.TriggerWords));

    public static string PromptChoiceLabel(EnhancementPrompt prompt)
    {
        var triggerWords = NormalizeTriggerWords(prompt.TriggerWords);
        if (triggerWords.Count == 0)
        {
            return prompt.Title;
        }

        return triggerWords.Count == 1
            ? $"{prompt.Title} - \"{triggerWords[0]}...\""
            : $"{prompt.Title} - \"{triggerWords[0]}...\" +{triggerWords.Count - 1}";
    }

    public static IReadOnlyList<string> TriggerWordsFromText(string text) =>
        NormalizeTriggerWords(ParseTriggerWords(text));

    private static EnhancementPrompt NormalizePredefinedOverride(EnhancementPrompt prompt) =>
        prompt with
        {
            IsPredefined = true,
            TriggerWords = NormalizeTriggerWords(prompt.TriggerWords)
        };

    private static EnhancementPrompt? NormalizeCustomPrompt(EnhancementPrompt prompt)
    {
        var title = prompt.Title.Trim();
        var promptText = prompt.PromptText.Trim();
        if (title.Length == 0 || promptText.Length == 0)
        {
            return null;
        }

        var description = prompt.Description?.Trim();
        if (description?.Length == 0)
        {
            description = null;
        }

        return prompt with
        {
            Title = title,
            PromptText = promptText,
            Icon = string.IsNullOrWhiteSpace(prompt.Icon) ? DefaultIcon : prompt.Icon.Trim(),
            Description = description,
            IsPredefined = false,
            TriggerWords = NormalizeTriggerWords(prompt.TriggerWords)
        };
    }

    private static IReadOnlyList<string> ParseTriggerWords(string text) =>
        text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static IReadOnlyList<string> NormalizeTriggerWords(IEnumerable<string> triggerWords)
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var triggerWord in triggerWords)
        {
            var normalized = triggerWord.Trim();
            if (normalized.Length == 0 || !seen.Add(normalized))
            {
                continue;
            }

            result.Add(normalized);
        }

        return result;
    }
}
