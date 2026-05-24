using System.Text.Json;
using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Infrastructure.Dictionary;

public sealed class JsonDictionaryStore(string filePath) : IWritableDictionaryStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly SemaphoreSlim gate = new(1, 1);

    public async Task<IReadOnlyList<VocabularyWord>> ListVocabularyAsync(CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            var data = await LoadUnlockedAsync(cancellationToken);
            return data.Vocabulary
                .OrderBy(word => word.Word, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<IReadOnlyList<WordReplacement>> ListReplacementsAsync(CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            var data = await LoadUnlockedAsync(cancellationToken);
            return data.Replacements
                .OrderBy(replacement => replacement.OriginalText, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<string?> AddVocabularyWordsAsync(string input, CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            var data = await LoadUnlockedAsync(cancellationToken);
            var additions = DictionaryService.AddVocabularyWords(
                input,
                data.Vocabulary,
                DateTimeOffset.UtcNow,
                out var error);

            if (error is not null || additions.Count == 0)
            {
                return error;
            }

            data.Vocabulary.AddRange(additions);
            await SaveUnlockedAsync(data, cancellationToken);
            return null;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<string?> AddWordReplacementAsync(
        string original,
        string replacement,
        CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            var data = await LoadUnlockedAsync(cancellationToken);
            var addition = DictionaryService.AddWordReplacement(
                original,
                replacement,
                data.Replacements,
                DateTimeOffset.UtcNow,
                out var error);

            if (error is not null || addition is null)
            {
                return error;
            }

            data.Replacements.Add(addition);
            await SaveUnlockedAsync(data, cancellationToken);
            return null;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task DeleteVocabularyWordAsync(Guid id, CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            var data = await LoadUnlockedAsync(cancellationToken);
            data.Vocabulary.RemoveAll(word => word.Id == id);
            await SaveUnlockedAsync(data, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task DeleteReplacementAsync(Guid id, CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            var data = await LoadUnlockedAsync(cancellationToken);
            data.Replacements.RemoveAll(replacement => replacement.Id == id);
            await SaveUnlockedAsync(data, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<string> ExportBackupAsync(CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            var data = await LoadUnlockedAsync(cancellationToken);
            return DictionaryBackup.Export(data.Vocabulary, data.Replacements);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<DictionaryImportResult> ImportBackupAsync(string json, CancellationToken cancellationToken)
    {
        var backup = DictionaryBackup.Parse(json);

        await gate.WaitAsync(cancellationToken);
        try
        {
            var data = await LoadUnlockedAsync(cancellationToken);
            var importedVocabularyCount = 0;
            var importedReplacementCount = 0;
            var skippedDuplicateCount = 0;
            var now = DateTimeOffset.UtcNow;

            foreach (var word in backup.VocabularyWords)
            {
                var additions = DictionaryService.AddVocabularyWords(
                    word,
                    data.Vocabulary,
                    now,
                    out var error);

                if (error is not null)
                {
                    skippedDuplicateCount++;
                    continue;
                }

                data.Vocabulary.AddRange(additions);
                importedVocabularyCount += additions.Count;
            }

            foreach (var replacement in backup.WordReplacements)
            {
                var addition = DictionaryService.AddWordReplacement(
                    replacement.OriginalText,
                    replacement.ReplacementText,
                    data.Replacements,
                    now,
                    out var error);

                if (error is not null)
                {
                    skippedDuplicateCount++;
                    continue;
                }

                if (addition is not null)
                {
                    data.Replacements.Add(addition);
                    importedReplacementCount++;
                }
            }

            if (importedVocabularyCount > 0 || importedReplacementCount > 0)
            {
                await SaveUnlockedAsync(data, cancellationToken);
            }

            return new DictionaryImportResult(
                importedVocabularyCount,
                importedReplacementCount,
                skippedDuplicateCount);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<DictionaryData> LoadUnlockedAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return new DictionaryData();
        }

        await using var stream = File.OpenRead(filePath);
        var data = await JsonSerializer.DeserializeAsync<DictionaryData>(stream, JsonOptions, cancellationToken);
        return data ?? new DictionaryData();
    }

    private async Task SaveUnlockedAsync(DictionaryData data, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempDirectory = string.IsNullOrWhiteSpace(directory) ? "." : directory;
        var tempPath = Path.Combine(
            tempDirectory,
            $".{Path.GetFileName(filePath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var stream = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                options: FileOptions.Asynchronous))
            {
                await JsonSerializer.SerializeAsync(stream, data, JsonOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            File.Move(tempPath, filePath, overwrite: true);
        }
        catch
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            throw;
        }
    }

    private sealed record DictionaryData
    {
        public List<VocabularyWord> Vocabulary { get; init; } = [];
        public List<WordReplacement> Replacements { get; init; } = [];
    }
}
