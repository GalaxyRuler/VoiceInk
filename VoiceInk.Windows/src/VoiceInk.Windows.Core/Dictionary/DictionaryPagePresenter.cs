namespace VoiceInk.Windows.Core.Dictionary;

public sealed record DictionaryPagePresentation(
    string HeroTitle,
    string HeroDescription,
    string OverviewSummary,
    string LocalBackupGuidance,
    IReadOnlyList<DictionarySummaryRow> SummaryRows,
    IReadOnlyList<DictionaryRuleGuidanceRow> RuleGuidanceRows,
    string VocabularySectionTitle,
    string VocabularySectionDescription,
    string VocabularyCountLabel,
    string VocabularyEmptyText,
    IReadOnlyList<DictionaryVocabularyRow> VocabularyRows,
    string ReplacementSectionTitle,
    string ReplacementSectionDescription,
    string ReplacementCountLabel,
    string ReplacementEmptyText,
    IReadOnlyList<DictionaryReplacementRow> ReplacementRows);

public sealed record DictionaryVocabularyRow(
    Guid Id,
    string Word,
    string DisplayText,
    string DetailText,
    string StatusBadge);

public sealed record DictionarySummaryRow(
    string Title,
    string Value,
    string Detail,
    string StatusBadge);

public sealed record DictionaryRuleGuidanceRow(
    string Title,
    string Value,
    string Detail,
    string StatusBadge);

public sealed record DictionaryReplacementRow(
    Guid Id,
    string OriginalText,
    string ReplacementText,
    bool IsEnabled,
    string DisplayText,
    string DetailText,
    string StatusBadge);

public static class DictionaryPagePresenter
{
    public static DictionaryPagePresentation Present(
        IEnumerable<VocabularyWord> vocabulary,
        IEnumerable<WordReplacement> replacements)
    {
        ArgumentNullException.ThrowIfNull(vocabulary);
        ArgumentNullException.ThrowIfNull(replacements);

        var vocabularyRows = vocabulary
            .Select(item => new DictionaryVocabularyRow(
                item.Id,
                item.Word,
                item.Word,
                "Used to help AI enhancement and supported transcription prompts recognize this term.",
                "Vocabulary"))
            .ToArray();
        var replacementRows = replacements
            .Select(item => new DictionaryReplacementRow(
                item.Id,
                item.OriginalText,
                item.ReplacementText,
                item.IsEnabled,
                ReplacementDisplayText(item),
                item.IsEnabled
                    ? "Runs after transcription and before insertion."
                    : "Kept locally but skipped during replacement cleanup.",
                item.IsEnabled ? "Enabled" : "Disabled"))
            .ToArray();

        return new DictionaryPagePresentation(
            "Dictionary Settings",
            "Enhance VoiceInk's transcription accuracy by teaching it your vocabulary",
            OverviewSummary(vocabularyRows.Length, replacementRows.Count(item => item.IsEnabled), replacementRows.Count(item => !item.IsEnabled)),
            "Import and export use local VoiceInk dictionary JSON only.",
            SummaryRows(
                vocabularyRows.Length,
                replacementRows.Count(item => item.IsEnabled),
                replacementRows.Count(item => !item.IsEnabled)),
            RuleGuidanceRows(),
            "Vocabulary",
            "Add words to help VoiceInk recognize them properly. (Requires AI enhancement)",
            $"Vocabulary Words ({vocabularyRows.Length})",
            vocabularyRows.Length == 0 ? "Add words to help VoiceInk recognize them properly." : string.Empty,
            vocabularyRows,
            "Word Replacements",
            "Automatically replace specific words/phrases with custom formatted text",
            $"Word Replacements ({replacementRows.Length})",
            replacementRows.Length == 0
                ? "Define word replacements to automatically replace specific words or phrases."
                : string.Empty,
            replacementRows);
    }

    private static IReadOnlyList<DictionarySummaryRow> SummaryRows(
        int vocabularyCount,
        int enabledReplacementCount,
        int disabledReplacementCount) =>
    [
        new(
            "Vocabulary",
            $"{vocabularyCount} {Pluralize(vocabularyCount, "word", "words")}",
            "Helps prompts recognize names, terms, and product words.",
            vocabularyCount > 0 ? "Active" : "Empty"),
        new(
            "Active Replacements",
            enabledReplacementCount.ToString(),
            "Runs after transcription before insertion.",
            enabledReplacementCount > 0 ? "Enabled" : "None"),
        new(
            "Disabled Replacements",
            disabledReplacementCount.ToString(),
            "Kept locally and skipped until re-enabled.",
            disabledReplacementCount > 0 ? "Paused" : "None"),
        new(
            "Local Backup",
            "JSON",
            "Import and export stay on this Windows profile unless you choose a file.",
            "Local")
    ];

    private static IReadOnlyList<DictionaryRuleGuidanceRow> RuleGuidanceRows() =>
    [
        new(
            "Vocabulary",
            "Prompt support",
            "Vocabulary words are included in enhancement and supported transcription prompts.",
            "Context"),
        new(
            "Enabled Replacements",
            "After transcription",
            "Enabled replacements run after transcription cleanup and before insertion.",
            "Automatic"),
        new(
            "Processing Order",
            "Vocabulary then replacements",
            "Vocabulary guides recognition and prompts; replacements rewrite the final text after transcription.",
            "Deterministic"),
        new(
            "Disabled Replacements",
            "Saved only",
            "Disabled replacements stay in the local dictionary and are skipped until re-enabled.",
            "Paused"),
        new(
            "Import / Export",
            "Local JSON",
            "Dictionary import and export use local files and do not sync automatically.",
            "Local"),
        new(
            "Import Conflicts",
            "Skip duplicates",
            "Duplicate vocabulary words and replacement keys are skipped so existing local entries stay in place.",
            "Review"),
        new(
            "Backup Workflow",
            "Edit or restore",
            "Export before bulk edits so you can review, edit, or restore dictionary entries later.",
            "Backup")
    ];

    private static string ReplacementDisplayText(WordReplacement replacement)
    {
        var display = $"{replacement.OriginalText} -> {replacement.ReplacementText}";
        return replacement.IsEnabled ? display : $"{display} (disabled)";
    }

    private static string OverviewSummary(
        int vocabularyCount,
        int enabledReplacementCount,
        int disabledReplacementCount)
    {
        if (vocabularyCount == 0 && enabledReplacementCount == 0 && disabledReplacementCount == 0)
        {
            return "No dictionary entries yet. Add vocabulary for names, terms, and product words; add replacements for repeated misrecognitions.";
        }

        return $"{vocabularyCount} {Pluralize(vocabularyCount, "vocabulary word", "vocabulary words")} help AI enhancement and supported transcription prompts. "
            + $"{ActiveReplacementText(enabledReplacementCount)}; {DisabledReplacementText(disabledReplacementCount)}.";
    }

    private static string ActiveReplacementText(int count) =>
        count == 0
            ? "No active replacements"
            : count == 1
                ? "1 active replacement runs after transcription"
                : $"{count} active replacements run after transcription";

    private static string DisabledReplacementText(int count) =>
        count == 0
            ? "no disabled replacements"
            : count == 1
                ? "1 disabled replacement is kept for later"
                : $"{count} disabled replacements are kept for later";

    private static string Pluralize(int count, string singular, string plural) =>
        count == 1 ? singular : plural;
}
