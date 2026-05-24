using VoiceInk.Windows.Core.Dictionary;

namespace VoiceInk.Windows.Core.Services;

public interface IDictionaryStore
{
    Task<IReadOnlyList<VocabularyWord>> ListVocabularyAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<WordReplacement>> ListReplacementsAsync(CancellationToken cancellationToken);
}
