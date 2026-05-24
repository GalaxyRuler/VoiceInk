using VoiceInk.Windows.Infrastructure.Dictionary;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Dictionary;

public sealed class JsonDictionaryStoreTests
{
    [Fact]
    public async Task AddVocabularyWordsAsync_PersistsCommaSeparatedWordsAndDeduplicatesWithinInput()
    {
        using var temp = new TempDirectory();
        var store = new JsonDictionaryStore(Path.Combine(temp.Path, "dictionary.json"));

        var error = await store.AddVocabularyWordsAsync("VoiceInk, Whisper, voiceink", CancellationToken.None);

        Assert.Null(error);
        var words = await store.ListVocabularyAsync(CancellationToken.None);
        Assert.Equal(["VoiceInk", "Whisper"], words.Select(word => word.Word).ToArray());
    }

    [Fact]
    public async Task AddVocabularyWordsAsync_ReturnsDuplicateErrorForSingleExistingWord()
    {
        using var temp = new TempDirectory();
        var store = new JsonDictionaryStore(Path.Combine(temp.Path, "dictionary.json"));

        await store.AddVocabularyWordsAsync("VoiceInk", CancellationToken.None);
        var error = await store.AddVocabularyWordsAsync("voiceink", CancellationToken.None);

        Assert.Equal("'voiceink' is already in the vocabulary", error);
        var words = await store.ListVocabularyAsync(CancellationToken.None);
        Assert.Single(words);
    }

    [Fact]
    public async Task AddWordReplacementAsync_PersistsReplacementAndRejectsDuplicateVariant()
    {
        using var temp = new TempDirectory();
        var store = new JsonDictionaryStore(Path.Combine(temp.Path, "dictionary.json"));

        var firstError = await store.AddWordReplacementAsync("Voice ink, Voicing", "VoiceInk", CancellationToken.None);
        var duplicateError = await store.AddWordReplacementAsync("voicing", "VoiceInk", CancellationToken.None);

        Assert.Null(firstError);
        Assert.Equal("'voicing' already exists in word replacements", duplicateError);
        var replacements = await store.ListReplacementsAsync(CancellationToken.None);
        var replacement = Assert.Single(replacements);
        Assert.Equal("Voice ink, Voicing", replacement.OriginalText);
        Assert.Equal("VoiceInk", replacement.ReplacementText);
    }

    [Fact]
    public async Task DeleteVocabularyWordAsync_RemovesMatchingWord()
    {
        using var temp = new TempDirectory();
        var store = new JsonDictionaryStore(Path.Combine(temp.Path, "dictionary.json"));
        await store.AddVocabularyWordsAsync("VoiceInk, Whisper", CancellationToken.None);
        var word = (await store.ListVocabularyAsync(CancellationToken.None)).Single(item => item.Word == "VoiceInk");

        await store.DeleteVocabularyWordAsync(word.Id, CancellationToken.None);

        var words = await store.ListVocabularyAsync(CancellationToken.None);
        Assert.Equal(["Whisper"], words.Select(item => item.Word).ToArray());
    }

    [Fact]
    public async Task DeleteReplacementAsync_RemovesMatchingReplacement()
    {
        using var temp = new TempDirectory();
        var store = new JsonDictionaryStore(Path.Combine(temp.Path, "dictionary.json"));
        await store.AddWordReplacementAsync("Voice ink", "VoiceInk", CancellationToken.None);
        var replacement = Assert.Single(await store.ListReplacementsAsync(CancellationToken.None));

        await store.DeleteReplacementAsync(replacement.Id, CancellationToken.None);

        Assert.Empty(await store.ListReplacementsAsync(CancellationToken.None));
    }

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"voiceink-{Guid.NewGuid():N}");

        public TempDirectory()
        {
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
