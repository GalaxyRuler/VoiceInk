using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Core.History;

public sealed class HistoryReenhancementService(
    IHistoryStore historyStore,
    ISettingsStore settingsStore,
    TextEnhancementPipeline enhancementPipeline,
    IDictionaryStore? dictionaryStore = null)
{
    private readonly IDictionaryStore dictionaryStore = dictionaryStore ?? EmptyDictionaryStore.Instance;

    public async Task<HistoryReenhancementResult> ReenhanceAsync(
        TranscriptionHistoryItem source,
        CancellationToken cancellationToken)
    {
        if (source.Status != TranscriptionHistoryStatus.Completed)
        {
            return new HistoryReenhancementResult(false, "Only completed transcriptions can be re-enhanced");
        }

        var text = TextForReenhancement(source);
        if (text.Length == 0)
        {
            return new HistoryReenhancementResult(false, "No transcription text available");
        }

        var settings = await settingsStore.LoadAsync(cancellationToken);
        if (!settings.IsEnhancementEnabled)
        {
            return new HistoryReenhancementResult(false, "AI enhancement is disabled");
        }

        var vocabulary = await dictionaryStore.ListVocabularyAsync(cancellationToken);
        var result = await enhancementPipeline.EnhanceAsync(
            text,
            settings,
            vocabulary,
            cancellationToken);

        if (!result.AttemptedEnhancement || result.WarningMessage is not null)
        {
            return new HistoryReenhancementResult(
                false,
                result.WarningMessage ?? "AI enhancement did not run");
        }

        if (string.IsNullOrWhiteSpace(result.EnhancedText))
        {
            return new HistoryReenhancementResult(false, "AI enhancement returned no text.");
        }

        var item = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            result.FinalText,
            source.ProviderName,
            source.AudioDuration,
            source.TranscriptionDuration,
            originalText: result.OriginalText,
            enhancedText: result.EnhancedText,
            status: TranscriptionHistoryStatus.Completed,
            language: source.Language,
            modelPath: source.ModelPath,
            promptName: result.PromptName,
            enhancementDuration: result.EnhancementDuration,
            audioFilePath: source.AudioFilePath,
            enhancementProviderName: result.EnhancementProviderName,
            enhancementModelName: result.EnhancementModelName,
            aiRequestSystemMessage: result.SystemMessage,
            aiRequestUserMessage: result.UserMessage,
            powerModeName: source.PowerModeName,
            powerModeEmoji: source.PowerModeEmoji);

        await historyStore.SaveAsync(item, cancellationToken);
        return new HistoryReenhancementResult(true, "Re-enhanced transcription saved", item);
    }

    private static string TextForReenhancement(TranscriptionHistoryItem source)
    {
        var originalText = source.OriginalText.Trim();
        return originalText.Length > 0
            ? originalText
            : source.Text.Trim();
    }

    private sealed class EmptyDictionaryStore : IDictionaryStore
    {
        public static EmptyDictionaryStore Instance { get; } = new();

        public Task<IReadOnlyList<VocabularyWord>> ListVocabularyAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<VocabularyWord>>([]);

        public Task<IReadOnlyList<WordReplacement>> ListReplacementsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<WordReplacement>>([]);
    }
}
