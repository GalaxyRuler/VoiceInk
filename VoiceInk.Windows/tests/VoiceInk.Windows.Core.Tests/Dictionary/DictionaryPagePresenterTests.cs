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
