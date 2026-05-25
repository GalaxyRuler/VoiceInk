using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Metrics;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Text;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Core.AudioFiles;

public sealed class AudioFileTranscriptionService(
    IAudioFileImportService audioFileImportService,
    ITranscriptionService transcriptionService,
    IHistoryStore historyStore,
    ISettingsStore settingsStore,
    IDictionaryStore? dictionaryStore = null,
    TextEnhancementPipeline? enhancementPipeline = null,
    ISessionMetricStore? sessionMetricStore = null)
{
    private readonly IDictionaryStore dictionaryStore = dictionaryStore ?? EmptyDictionaryStore.Instance;

    public async Task<AudioFileTranscriptionResult> TranscribeAsync(
        string sourcePath,
        string recordingsDirectory,
        CancellationToken cancellationToken)
    {
        try
        {
            var settings = await settingsStore.LoadAsync(cancellationToken);
            var configurationError = TranscriptionConfiguration.ValidateRequiredSettings(settings);
            if (configurationError is not null)
            {
                return new AudioFileTranscriptionResult(false, configurationError);
            }

            var vocabulary = await dictionaryStore.ListVocabularyAsync(cancellationToken);
            var replacements = await dictionaryStore.ListReplacementsAsync(cancellationToken);
            var vocabularyPrompt = DictionaryService.RenderVocabularyPrompt(vocabulary);
            var audio = await audioFileImportService.PrepareAsync(sourcePath, recordingsDirectory, cancellationToken);
            var transcription = await transcriptionService.TranscribeAsync(
                audio,
                TranscriptionConfiguration.BuildOptions(settings, vocabularyPrompt),
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
                return new AudioFileTranscriptionResult(false, "File transcription produced no text");
            }

            var enhancement = enhancementPipeline is null
                ? null
                : await enhancementPipeline.EnhanceAsync(finalText, settings, vocabulary, cancellationToken);
            var item = new TranscriptionHistoryItem(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                finalText,
                TranscriptionConfiguration.ProviderName(settings),
                audio.Duration,
                transcription.Duration,
                originalText: transcription.Text,
                status: TranscriptionHistoryStatus.Completed,
                language: settings.Language,
                modelPath: TranscriptionConfiguration.ModelMetadata(settings),
                promptName: enhancement?.PromptName,
                enhancementDuration: enhancement?.EnhancementDuration,
                errorMessage: enhancement?.WarningMessage,
                audioFilePath: audio.FilePath,
                enhancedText: enhancement?.EnhancedText,
                enhancementProviderName: enhancement?.EnhancementProviderName,
                enhancementModelName: enhancement?.EnhancementModelName,
                aiRequestSystemMessage: enhancement?.SystemMessage,
                aiRequestUserMessage: enhancement?.UserMessage);
            await historyStore.SaveAsync(item, cancellationToken);
            await RecordMetricAsync(item, cancellationToken);

            return new AudioFileTranscriptionResult(true, "File transcription saved", item);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new AudioFileTranscriptionResult(false, ex.Message);
        }
    }

    private Task<SessionMetricRecorderResult> RecordMetricAsync(
        TranscriptionHistoryItem item,
        CancellationToken cancellationToken) =>
        sessionMetricStore is null
            ? Task.FromResult(new SessionMetricRecorderResult(false))
            : SessionMetricRecorder.RecordAsync(
                item,
                sessionMetricStore,
                "audio-file",
                cancellationToken);

    private sealed class EmptyDictionaryStore : IDictionaryStore
    {
        public static EmptyDictionaryStore Instance { get; } = new();

        public Task<IReadOnlyList<VocabularyWord>> ListVocabularyAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<VocabularyWord>>([]);

        public Task<IReadOnlyList<WordReplacement>> ListReplacementsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<WordReplacement>>([]);
    }
}
