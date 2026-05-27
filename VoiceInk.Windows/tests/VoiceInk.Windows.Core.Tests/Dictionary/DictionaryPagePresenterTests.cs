using VoiceInk.Windows.Core.Dictionary;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Dictionary;

public sealed class DictionaryPagePresenterTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Present_BuildsMacStyleSectionLabelsAndRows()
    {
        var vocabulary = new[]
        {
            new VocabularyWord(Guid.NewGuid(), "VoiceInk", Now),
            new VocabularyWord(Guid.NewGuid(), "AssemblyAI", Now)
        };
        var replacements = new[]
        {
            new WordReplacement(Guid.NewGuid(), "Voice ink", "VoiceInk", Now, true),
            new WordReplacement(Guid.NewGuid(), "old name", "New Name", Now, false)
        };

        var presentation = DictionaryPagePresenter.Present(vocabulary, replacements);

        Assert.Equal("Dictionary Settings", presentation.HeroTitle);
        Assert.Equal(
            "Enhance VoiceInk's transcription accuracy by teaching it your vocabulary",
            presentation.HeroDescription);
        Assert.Equal("Vocabulary Words (2)", presentation.VocabularyCountLabel);
        Assert.Equal("Word Replacements (2)", presentation.ReplacementCountLabel);
        Assert.Equal(
            "2 vocabulary words help AI enhancement and supported transcription prompts. 1 active replacement runs after transcription; 1 disabled replacement is kept for later.",
            presentation.OverviewSummary);
        Assert.Equal(
            "Import and export use local VoiceInk dictionary JSON only.",
            presentation.LocalBackupGuidance);
        Assert.Collection(
            presentation.SummaryRows,
            row =>
            {
                Assert.Equal("Vocabulary", row.Title);
                Assert.Equal("2 words", row.Value);
                Assert.Equal("Helps prompts recognize names, terms, and product words.", row.Detail);
                Assert.Equal("Active", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Active Replacements", row.Title);
                Assert.Equal("1", row.Value);
                Assert.Equal("Runs after transcription before insertion.", row.Detail);
                Assert.Equal("Enabled", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Disabled Replacements", row.Title);
                Assert.Equal("1", row.Value);
                Assert.Equal("Kept locally and skipped until re-enabled.", row.Detail);
                Assert.Equal("Paused", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Local Backup", row.Title);
                Assert.Equal("JSON", row.Value);
                Assert.Equal("Import and export stay on this Windows profile unless you choose a file.", row.Detail);
                Assert.Equal("Local", row.StatusBadge);
            });
        Assert.Collection(
            presentation.RuleGuidanceRows,
            row =>
            {
                Assert.Equal("Vocabulary", row.Title);
                Assert.Equal("Prompt support", row.Value);
                Assert.Equal("Vocabulary words are included in enhancement and supported transcription prompts.", row.Detail);
                Assert.Equal("Context", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Enabled Replacements", row.Title);
                Assert.Equal("After transcription", row.Value);
                Assert.Equal("Enabled replacements run after transcription cleanup and before insertion.", row.Detail);
                Assert.Equal("Automatic", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Processing Order", row.Title);
                Assert.Equal("Vocabulary then replacements", row.Value);
                Assert.Equal("Vocabulary guides recognition and prompts; replacements rewrite the final text after transcription.", row.Detail);
                Assert.Equal("Deterministic", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Disabled Replacements", row.Title);
                Assert.Equal("Saved only", row.Value);
                Assert.Equal("Disabled replacements stay in the local dictionary and are skipped until re-enabled.", row.Detail);
                Assert.Equal("Paused", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Quick Add", row.Title);
                Assert.Equal("Tray or shortcut", row.Value);
                Assert.Equal("Use the notification-area menu or Quick Add shortcut to add vocabulary and replacements without opening the full Dictionary page.", row.Detail);
                Assert.Equal("Fast path", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Import / Export", row.Title);
                Assert.Equal("Local JSON", row.Value);
                Assert.Equal("Dictionary import and export use local files and do not sync automatically.", row.Detail);
                Assert.Equal("Local", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Import Conflicts", row.Title);
                Assert.Equal("Skip duplicates", row.Value);
                Assert.Equal("Duplicate vocabulary words and replacement keys are skipped so existing local entries stay in place.", row.Detail);
                Assert.Equal("Review", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Backup Workflow", row.Title);
                Assert.Equal("Edit or restore", row.Value);
                Assert.Equal("Export before bulk edits so you can review, edit, or restore dictionary entries later.", row.Detail);
                Assert.Equal("Backup", row.StatusBadge);
            });
        Assert.Equal(string.Empty, presentation.VocabularyEmptyText);
        Assert.Equal(string.Empty, presentation.ReplacementEmptyText);
        Assert.Equal(["VoiceInk", "AssemblyAI"], presentation.VocabularyRows.Select(row => row.DisplayText).ToArray());
        Assert.All(
            presentation.VocabularyRows,
            row =>
            {
                Assert.Equal("Vocabulary", row.StatusBadge);
                Assert.Equal("Used to help AI enhancement and supported transcription prompts recognize this term.", row.DetailText);
            });
        Assert.Collection(
            presentation.ReplacementRows,
            row =>
            {
                Assert.Equal("Voice ink", row.OriginalText);
                Assert.Equal("VoiceInk", row.ReplacementText);
                Assert.Equal("Voice ink -> VoiceInk", row.DisplayText);
                Assert.Equal("Enabled", row.StatusBadge);
                Assert.Equal("Runs after transcription and before insertion.", row.DetailText);
                Assert.True(row.IsEnabled);
            },
            row =>
            {
                Assert.Equal("old name -> New Name (disabled)", row.DisplayText);
                Assert.Equal("Disabled", row.StatusBadge);
                Assert.Equal("Kept locally but skipped during replacement cleanup.", row.DetailText);
                Assert.False(row.IsEnabled);
            });
    }

    [Fact]
    public void Present_ShowsQuickAddGuidance()
    {
        var presentation = DictionaryPagePresenter.Present([], []);

        Assert.Contains(
            presentation.RuleGuidanceRows,
            row => row.Title == "Quick Add"
                && row.Value == "Tray or shortcut"
                && row.Detail == "Use the notification-area menu or Quick Add shortcut to add vocabulary and replacements without opening the full Dictionary page."
                && row.StatusBadge == "Fast path");
    }

    [Fact]
    public void Present_BuildsMacStyleEmptyStateText()
    {
        var presentation = DictionaryPagePresenter.Present([], []);

        Assert.Equal("Vocabulary Words (0)", presentation.VocabularyCountLabel);
        Assert.Equal("Word Replacements (0)", presentation.ReplacementCountLabel);
        Assert.Equal(
            "No dictionary entries yet. Add vocabulary for names, terms, and product words; add replacements for repeated misrecognitions.",
            presentation.OverviewSummary);
        Assert.Equal(
            "Import and export use local VoiceInk dictionary JSON only.",
            presentation.LocalBackupGuidance);
        Assert.Equal("0 words", presentation.SummaryRows[0].Value);
        Assert.Equal("0", presentation.SummaryRows[1].Value);
        Assert.Equal("Add words to help VoiceInk recognize them properly.", presentation.VocabularyEmptyText);
        Assert.Equal(
            "Define word replacements to automatically replace specific words or phrases.",
            presentation.ReplacementEmptyText);
        Assert.Empty(presentation.VocabularyRows);
        Assert.Empty(presentation.ReplacementRows);
    }

    [Fact]
    public void Present_AllReplacementsDisabled_ShowsDisabledGuidance()
    {
        var replacements = new[]
        {
            new WordReplacement(Guid.NewGuid(), "Voice ink", "VoiceInk", Now, false)
        };

        var presentation = DictionaryPagePresenter.Present([], replacements);

        Assert.Equal(
            "0 vocabulary words help AI enhancement and supported transcription prompts. No active replacements; 1 disabled replacement is kept for later.",
            presentation.OverviewSummary);
    }
}
