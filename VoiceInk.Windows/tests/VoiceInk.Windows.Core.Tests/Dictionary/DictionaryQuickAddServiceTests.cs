using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Services;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Dictionary;

public sealed class DictionaryQuickAddServiceTests
{
    [Fact]
    public async Task SubmitAsync_VocabularyMode_AddsVocabularyWords()
    {
        var store = new FakeDictionaryStore();
        var service = new DictionaryQuickAddService(store);

        var result = await service.SubmitAsync(
            DictionaryQuickAddMode.Vocabulary,
            " VoiceInk, WinUI ",
            replacementOriginal: string.Empty,
            replacementText: string.Empty,
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("Vocabulary updated", result.Message);
        Assert.Equal("VoiceInk, WinUI", store.LastVocabularyInput);
    }

    [Fact]
    public async Task SubmitAsync_WordReplacementMode_AddsWordReplacement()
    {
        var store = new FakeDictionaryStore();
        var service = new DictionaryQuickAddService(store);

        var result = await service.SubmitAsync(
            DictionaryQuickAddMode.WordReplacement,
            vocabularyInput: string.Empty,
            replacementOriginal: " voice ink ",
            replacementText: " VoiceInk ",
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("Word replacements updated", result.Message);
        Assert.Equal("voice ink", store.LastReplacementOriginal);
        Assert.Equal("VoiceInk", store.LastReplacementText);
    }

    [Fact]
    public async Task SubmitAsync_VocabularyMode_RequiresVocabularyInput()
    {
        var store = new FakeDictionaryStore();
        var service = new DictionaryQuickAddService(store);

        var result = await service.SubmitAsync(
            DictionaryQuickAddMode.Vocabulary,
            "   ",
            replacementOriginal: string.Empty,
            replacementText: string.Empty,
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Enter a vocabulary word.", result.Message);
        Assert.Null(store.LastVocabularyInput);
    }

    [Fact]
    public async Task SubmitAsync_WordReplacementMode_RequiresOriginalText()
    {
        var store = new FakeDictionaryStore();
        var service = new DictionaryQuickAddService(store);

        var result = await service.SubmitAsync(
            DictionaryQuickAddMode.WordReplacement,
            vocabularyInput: string.Empty,
            replacementOriginal: " ",
            replacementText: "VoiceInk",
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Enter original text.", result.Message);
        Assert.Null(store.LastReplacementOriginal);
    }

    [Fact]
    public async Task SubmitAsync_WordReplacementMode_RequiresReplacementText()
    {
        var store = new FakeDictionaryStore();
        var service = new DictionaryQuickAddService(store);

        var result = await service.SubmitAsync(
            DictionaryQuickAddMode.WordReplacement,
            vocabularyInput: string.Empty,
            replacementOriginal: "voice ink",
            replacementText: " ",
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Enter replacement text.", result.Message);
        Assert.Null(store.LastReplacementOriginal);
    }

    [Fact]
    public async Task SubmitAsync_ReturnsStoreDuplicateErrors()
    {
        var store = new FakeDictionaryStore { VocabularyError = "'VoiceInk' is already in the vocabulary" };
        var service = new DictionaryQuickAddService(store);

        var result = await service.SubmitAsync(
            DictionaryQuickAddMode.Vocabulary,
            "VoiceInk",
            replacementOriginal: string.Empty,
            replacementText: string.Empty,
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("'VoiceInk' is already in the vocabulary", result.Message);
    }

    private sealed class FakeDictionaryStore : IWritableDictionaryStore
    {
        public string? LastVocabularyInput { get; private set; }
        public string? LastReplacementOriginal { get; private set; }
        public string? LastReplacementText { get; private set; }
        public string? VocabularyError { get; init; }

        public Task<IReadOnlyList<VocabularyWord>> ListVocabularyAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<VocabularyWord>>([]);

        public Task<IReadOnlyList<WordReplacement>> ListReplacementsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<WordReplacement>>([]);

        public Task<string?> AddVocabularyWordsAsync(string input, CancellationToken cancellationToken)
        {
            LastVocabularyInput = input;
            return Task.FromResult(VocabularyError);
        }

        public Task<string?> AddWordReplacementAsync(
            string original,
            string replacement,
            CancellationToken cancellationToken)
        {
            LastReplacementOriginal = original;
            LastReplacementText = replacement;
            return Task.FromResult<string?>(null);
        }

        public Task DeleteVocabularyWordAsync(Guid id, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task DeleteReplacementAsync(Guid id, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<string> ExportBackupAsync(CancellationToken cancellationToken) =>
            Task.FromResult("{}");

        public Task<DictionaryImportResult> ImportBackupAsync(string json, CancellationToken cancellationToken) =>
            Task.FromResult(new DictionaryImportResult(0, 0, 0));
    }
}
