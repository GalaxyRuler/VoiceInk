using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Text;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Core.AudioFiles;

public sealed class AudioFileTranscriptionService(
    IAudioFileImportService audioFileImportService,
    ITranscriptionService transcriptionService,
    IHistoryStore historyStore,
    ISettingsStore settingsStore,
    IDictionaryStore? dictionaryStore = null)
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
            if (string.IsNullOrWhiteSpace(settings.ModelPath))
            {
                return new AudioFileTranscriptionResult(false, "Local whisper model path is required.");
            }

            var vocabulary = await dictionaryStore.ListVocabularyAsync(cancellationToken);
            var replacements = await dictionaryStore.ListReplacementsAsync(cancellationToken);
            var vocabularyPrompt = DictionaryService.RenderVocabularyPrompt(vocabulary);
            var audio = await audioFileImportService.PrepareAsync(sourcePath, recordingsDirectory, cancellationToken);
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
                return new AudioFileTranscriptionResult(false, "File transcription produced no text");
            }

            var item = new TranscriptionHistoryItem(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                finalText,
                transcription.ProviderName,
                audio.Duration,
                transcription.Duration,
                originalText: transcription.Text,
                status: TranscriptionHistoryStatus.Completed,
                language: settings.Language,
                modelPath: settings.ModelPath,
                audioFilePath: audio.FilePath);
            await historyStore.SaveAsync(item, cancellationToken);

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

    private sealed class EmptyDictionaryStore : IDictionaryStore
    {
        public static EmptyDictionaryStore Instance { get; } = new();

        public Task<IReadOnlyList<VocabularyWord>> ListVocabularyAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<VocabularyWord>>([]);

        public Task<IReadOnlyList<WordReplacement>> ListReplacementsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<WordReplacement>>([]);
    }
}
