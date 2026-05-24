using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Core.Dictionary;

public sealed class DictionaryQuickAddService(IWritableDictionaryStore dictionaryStore)
{
    public async Task<DictionaryQuickAddResult> SubmitAsync(
        DictionaryQuickAddMode mode,
        string vocabularyInput,
        string replacementOriginal,
        string replacementText,
        CancellationToken cancellationToken)
    {
        return mode switch
        {
            DictionaryQuickAddMode.WordReplacement => await AddWordReplacementAsync(
                replacementOriginal,
                replacementText,
                cancellationToken),
            _ => await AddVocabularyAsync(vocabularyInput, cancellationToken)
        };
    }

    private async Task<DictionaryQuickAddResult> AddVocabularyAsync(
        string input,
        CancellationToken cancellationToken)
    {
        var vocabulary = input.Trim();
        if (vocabulary.Length == 0)
        {
            return new DictionaryQuickAddResult(false, "Enter a vocabulary word.");
        }

        var error = await dictionaryStore.AddVocabularyWordsAsync(vocabulary, cancellationToken);
        return error is null
            ? new DictionaryQuickAddResult(true, "Vocabulary updated")
            : new DictionaryQuickAddResult(false, error);
    }

    private async Task<DictionaryQuickAddResult> AddWordReplacementAsync(
        string original,
        string replacement,
        CancellationToken cancellationToken)
    {
        var trimmedOriginal = original.Trim();
        if (trimmedOriginal.Length == 0)
        {
            return new DictionaryQuickAddResult(false, "Enter original text.");
        }

        var trimmedReplacement = replacement.Trim();
        if (trimmedReplacement.Length == 0)
        {
            return new DictionaryQuickAddResult(false, "Enter replacement text.");
        }

        var error = await dictionaryStore.AddWordReplacementAsync(
            trimmedOriginal,
            trimmedReplacement,
            cancellationToken);
        return error is null
            ? new DictionaryQuickAddResult(true, "Word replacements updated")
            : new DictionaryQuickAddResult(false, error);
    }
}
