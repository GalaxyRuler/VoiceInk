using VoiceInk.Windows.Core.Dictionary;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Dictionary;

public sealed class DictionarySortServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 24, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void SortVocabulary_WordDescendingSortsCaseInsensitively()
    {
        var words = new[]
        {
            new VocabularyWord(Guid.NewGuid(), "alpha", Now),
            new VocabularyWord(Guid.NewGuid(), "VoiceInk", Now),
            new VocabularyWord(Guid.NewGuid(), "beta", Now)
        };

        var result = DictionarySortService.SortVocabulary(words, DictionarySortModes.VocabularyWordDescending);

        Assert.Equal(["VoiceInk", "beta", "alpha"], result.Select(word => word.Word).ToArray());
    }

    [Fact]
    public void SortReplacements_ReplacementDescendingSortsCaseInsensitively()
    {
        var replacements = new[]
        {
            new WordReplacement(Guid.NewGuid(), "a", "alpha", Now),
            new WordReplacement(Guid.NewGuid(), "v", "VoiceInk", Now),
            new WordReplacement(Guid.NewGuid(), "b", "beta", Now)
        };

        var result = DictionarySortService.SortReplacements(
            replacements,
            DictionarySortModes.ReplacementTextDescending);

        Assert.Equal(["VoiceInk", "beta", "alpha"], result.Select(item => item.ReplacementText).ToArray());
    }

    [Fact]
    public void SortReplacements_UnknownModeFallsBackToOriginalAscending()
    {
        var replacements = new[]
        {
            new WordReplacement(Guid.NewGuid(), "Voice ink", "VoiceInk", Now),
            new WordReplacement(Guid.NewGuid(), "codex", "Codex", Now),
            new WordReplacement(Guid.NewGuid(), "assembly", "AssemblyAI", Now)
        };

        var result = DictionarySortService.SortReplacements(replacements, "unknown");

        Assert.Equal(["assembly", "codex", "Voice ink"], result.Select(item => item.OriginalText).ToArray());
    }

    [Fact]
    public void SortReplacements_UsesStableTieBreakerForReplacementTextSort()
    {
        var replacements = new[]
        {
            new WordReplacement(Guid.NewGuid(), "voice", "VoiceInk", Now),
            new WordReplacement(Guid.NewGuid(), "assembly", "VoiceInk", Now),
            new WordReplacement(Guid.NewGuid(), "codex", "Codex", Now)
        };

        var result = DictionarySortService.SortReplacements(
            replacements,
            DictionarySortModes.ReplacementTextAscending);

        Assert.Equal(["codex", "assembly", "voice"], result.Select(item => item.OriginalText).ToArray());
    }
}
