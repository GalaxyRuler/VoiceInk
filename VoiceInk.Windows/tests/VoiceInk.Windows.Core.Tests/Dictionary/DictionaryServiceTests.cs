using VoiceInk.Windows.Core.Dictionary;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Dictionary;

public sealed class DictionaryServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 24, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AddVocabularyWords_AddsTrimmedCommaSeparatedUniqueWords()
    {
        var existing = new[]
        {
            new VocabularyWord(Guid.NewGuid(), "VoiceInk", Now.AddDays(-1))
        };

        var added = DictionaryService.AddVocabularyWords(
            " VoiceInk, WinUI, whisper.cpp, winui ",
            existing,
            Now,
            out var error);

        Assert.Null(error);
        Assert.Collection(
            added,
            word => Assert.Equal("WinUI", word.Word),
            word => Assert.Equal("whisper.cpp", word.Word));
        Assert.All(added, word => Assert.Equal(Now, word.DateAdded));
    }

    [Fact]
    public void AddVocabularyWords_ReturnsMacStyleDuplicateMessageForSingleExistingWord()
    {
        var existing = new[]
        {
            new VocabularyWord(Guid.NewGuid(), "VoiceInk", Now)
        };

        var added = DictionaryService.AddVocabularyWords(" voiceink ", existing, Now, out var error);

        Assert.Empty(added);
        Assert.Equal("'voiceink' is already in the vocabulary", error);
    }

    [Fact]
    public void AddWordReplacement_RejectsDuplicateVariantAcrossExistingEntries()
    {
        var existing = new[]
        {
            new WordReplacement(Guid.NewGuid(), "Voice ink, Voicing", "VoiceInk", Now, IsEnabled: true)
        };

        var added = DictionaryService.AddWordReplacement(
            " dictation, voicing ",
            "VoiceInk",
            existing,
            Now,
            out var error);

        Assert.Null(added);
        Assert.Equal("'voicing' already exists in word replacements", error);
    }

    [Fact]
    public void AddWordReplacement_CreatesReplacementWithTrimmedInput()
    {
        var added = DictionaryService.AddWordReplacement(
            " my website link ",
            " https://tryvoiceink.com ",
            Array.Empty<WordReplacement>(),
            Now,
            out var error);

        Assert.Null(error);
        Assert.NotNull(added);
        Assert.Equal("my website link", added.OriginalText);
        Assert.Equal("https://tryvoiceink.com", added.ReplacementText);
        Assert.True(added.IsEnabled);
        Assert.Equal(Now, added.DateAdded);
    }

    [Fact]
    public void ApplyReplacements_UsesLongestEnabledVariantsFirst()
    {
        var replacements = new[]
        {
            new WordReplacement(Guid.NewGuid(), "voice", "audio", Now, IsEnabled: true),
            new WordReplacement(Guid.NewGuid(), "voice ink, voicing", "VoiceInk", Now, IsEnabled: true),
            new WordReplacement(Guid.NewGuid(), "disabled", "ignored", Now, IsEnabled: false)
        };

        var result = DictionaryService.ApplyReplacements(
            "I use voice ink for voice notes. disabled stays.",
            replacements);

        Assert.Equal("I use VoiceInk for audio notes. disabled stays.", result);
    }

    [Fact]
    public void ApplyReplacements_UsesCaseInsensitiveWordBoundariesForSpacedLanguages()
    {
        var replacements = new[]
        {
            new WordReplacement(Guid.NewGuid(), "cat", "dog", Now, IsEnabled: true)
        };

        var result = DictionaryService.ApplyReplacements("Cat scatter cat, cat.", replacements);

        Assert.Equal("dog scatter dog, dog.", result);
    }

    [Fact]
    public void ApplyReplacements_UsesSubstringMatchingForNonSpacedScripts()
    {
        var replacements = new[]
        {
            new WordReplacement(Guid.NewGuid(), "東京", "Tokyo", Now, IsEnabled: true)
        };

        var result = DictionaryService.ApplyReplacements("東京都に行く", replacements);

        Assert.Equal("Tokyo都に行く", result);
    }

    [Fact]
    public void RenderVocabularyPrompt_ReturnsMacStylePromptText()
    {
        var words = new[]
        {
            new VocabularyWord(Guid.NewGuid(), "VoiceInk", Now),
            new VocabularyWord(Guid.NewGuid(), "WinUI", Now)
        };

        var result = DictionaryService.RenderVocabularyPrompt(words);

        Assert.Equal("Important Vocabulary: VoiceInk, WinUI", result);
    }

    [Fact]
    public void RenderVocabularyPrompt_ReturnsEmptyStringWhenNoWordsExist()
    {
        var result = DictionaryService.RenderVocabularyPrompt(Array.Empty<VocabularyWord>());

        Assert.Equal(string.Empty, result);
    }
}
