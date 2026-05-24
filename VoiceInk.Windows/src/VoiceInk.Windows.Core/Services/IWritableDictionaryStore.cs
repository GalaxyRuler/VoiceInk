namespace VoiceInk.Windows.Core.Services;

public interface IWritableDictionaryStore : IDictionaryStore
{
    Task<string?> AddVocabularyWordsAsync(string input, CancellationToken cancellationToken);
    Task<string?> AddWordReplacementAsync(string original, string replacement, CancellationToken cancellationToken);
    Task DeleteVocabularyWordAsync(Guid id, CancellationToken cancellationToken);
    Task DeleteReplacementAsync(Guid id, CancellationToken cancellationToken);
}
