using System.Text.Json;
using System.Text.Json.Serialization;

namespace VoiceInk.Windows.Core.Dictionary;

public sealed record DictionaryBackupData(
    IReadOnlyList<string> VocabularyWords,
    IReadOnlyList<(string OriginalText, string ReplacementText)> WordReplacements);

public static class DictionaryBackup
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Export(
        IEnumerable<VocabularyWord> vocabulary,
        IEnumerable<WordReplacement> replacements)
    {
        var backup = new BackupFile(
            Version: "1.0",
            VocabularyWords: vocabulary
                .Select(word => word.Word.Trim())
                .Where(word => word.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Order(StringComparer.OrdinalIgnoreCase)
                .Select(word => new WordBackup(word))
                .ToArray(),
            WordReplacements: ExportableReplacements(replacements));

        return JsonSerializer.Serialize(backup, JsonOptions);
    }

    public static DictionaryBackupData Parse(string json)
    {
        var backup = JsonSerializer.Deserialize<BackupFile>(json, JsonOptions);
        if (backup is null)
        {
            return new DictionaryBackupData([], []);
        }

        var vocabulary = backup.VocabularyWords
            .Select(word => word.Word.Trim())
            .Where(word => word.Length > 0)
            .ToArray();
        var replacements = backup.WordReplacements
            .Select(pair => (OriginalText: pair.Key.Trim(), ReplacementText: pair.Value.Trim()))
            .Where(pair => pair.OriginalText.Length > 0 && pair.ReplacementText.Length > 0)
            .ToArray();

        return new DictionaryBackupData(vocabulary, replacements);
    }

    private sealed record BackupFile(
        [property: JsonPropertyName("version")] string Version,
        [property: JsonPropertyName("vocabularyWords")] IReadOnlyList<WordBackup> VocabularyWords,
        [property: JsonPropertyName("wordReplacements")] IReadOnlyDictionary<string, string> WordReplacements)
    {
        public BackupFile()
            : this("1.0", [], new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase))
        {
        }
    }

    private sealed record WordBackup([property: JsonPropertyName("word")] string Word);

    private static IReadOnlyDictionary<string, string> ExportableReplacements(
        IEnumerable<WordReplacement> replacements) =>
        replacements
            .Where(replacement => replacement.IsEnabled)
            .Select(replacement => new
            {
                OriginalText = replacement.OriginalText.Trim(),
                ReplacementText = replacement.ReplacementText.Trim()
            })
            .Where(replacement =>
                replacement.OriginalText.Length > 0
                && replacement.ReplacementText.Length > 0)
            .GroupBy(replacement => replacement.OriginalText, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Last().OriginalText,
                group => group.Last().ReplacementText,
                StringComparer.OrdinalIgnoreCase);
}
