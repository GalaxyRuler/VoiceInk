using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Text;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Core.History;

public sealed class HistoryRetryService(
    ITranscriptionService transcriptionService,
    IHistoryStore historyStore,
    ISettingsStore settingsStore,
    IDictionaryStore? dictionaryStore = null)
{
    private readonly IDictionaryStore dictionaryStore = dictionaryStore ?? EmptyDictionaryStore.Instance;

    public async Task<HistoryRetryResult> RetryLatestAsync(CancellationToken cancellationToken)
    {
        var source = await historyStore.GetLatestCompletedWithAudioAsync(cancellationToken);
        return source is null
            ? new HistoryRetryResult(false, "No transcription available")
            : await RetryAsync(source, cancellationToken);
    }

    public async Task<HistoryRetryResult> RetryAsync(
        TranscriptionHistoryItem source,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(source.AudioFilePath)
            || !File.Exists(source.AudioFilePath))
        {
            return new HistoryRetryResult(false, "Audio file not found");
        }

        var settings = await settingsStore.LoadAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(settings.ModelPath))
        {
            return new HistoryRetryResult(false, "Local whisper model path is required.");
        }

        var vocabulary = await dictionaryStore.ListVocabularyAsync(cancellationToken);
        var replacements = await dictionaryStore.ListReplacementsAsync(cancellationToken);
        var vocabularyPrompt = DictionaryService.RenderVocabularyPrompt(vocabulary);

        var audio = new AudioCaptureResult(
            source.AudioFilePath,
            source.AudioDuration,
            SampleRate: 16000,
            ChannelCount: 1);
        var transcription = await transcriptionService.TranscribeAsync(
            audio,
            new TranscriptionOptions(settings.ModelPath, settings.Language, vocabularyPrompt),
            cancellationToken);
        var finalText = TextPostProcessor.Process(
            transcription.Text,
            new TextPostProcessingOptions(
                AppendTrailingSpace: settings.AppendTrailingSpace,
                RemoveFillerWords: settings.RemoveFillerWords,
                WordReplacements: replacements,
                PunctuationCleanupMode: settings.PunctuationCleanupMode,
                LowercaseTranscription: settings.LowercaseTranscription));

        if (finalText.Length == 0)
        {
            return new HistoryRetryResult(false, "Retry produced no transcription");
        }

        var item = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            finalText,
            transcription.ProviderName,
            source.AudioDuration,
            transcription.Duration,
            originalText: transcription.Text,
            status: TranscriptionHistoryStatus.Completed,
            language: settings.Language,
            modelPath: settings.ModelPath,
            audioFilePath: source.AudioFilePath);

        await historyStore.SaveAsync(item, cancellationToken);
        return new HistoryRetryResult(true, "Retry transcription saved", item);
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
