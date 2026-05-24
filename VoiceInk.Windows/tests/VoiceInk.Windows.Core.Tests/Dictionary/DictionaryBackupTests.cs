using VoiceInk.Windows.Core.Dictionary;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Dictionary;

public sealed class DictionaryBackupTests
{
    [Fact]
    public void Export_WritesMacStyleDictionaryFields()
    {
        var json = DictionaryBackup.Export(
            [
                new VocabularyWord(Guid.NewGuid(), "VoiceInk", DateTimeOffset.UnixEpoch),
                new VocabularyWord(Guid.NewGuid(), "Whisper", DateTimeOffset.UnixEpoch)
            ],
            [
                new WordReplacement(Guid.NewGuid(), "Voice ink, Voicing", "VoiceInk", DateTimeOffset.UnixEpoch),
                new WordReplacement(Guid.NewGuid(), "disabled", "ignored", DateTimeOffset.UnixEpoch, IsEnabled: false)
            ]);

        Assert.Contains("\"vocabularyWords\"", json);
        Assert.Contains("\"word\": \"VoiceInk\"", json);
        Assert.Contains("\"wordReplacements\"", json);
        Assert.Contains("\"Voice ink, Voicing\": \"VoiceInk\"", json);
        Assert.DoesNotContain("disabled", json);
    }

    [Fact]
    public void Parse_ReadsMacStyleDictionaryFields()
    {
        var parsed = DictionaryBackup.Parse(
            """
            {
              "version": "1.0",
              "vocabularyWords": [{ "word": "VoiceInk" }, { "word": "Whisper" }],
              "wordReplacements": {
                "Voice ink, Voicing": "VoiceInk"
              }
            }
            """);

        Assert.Equal(["VoiceInk", "Whisper"], parsed.VocabularyWords);
        Assert.Equal(("Voice ink, Voicing", "VoiceInk"), parsed.WordReplacements.Single());
    }

    [Fact]
    public void Parse_IgnoresBlankDictionaryValues()
    {
        var parsed = DictionaryBackup.Parse(
            """
            {
              "vocabularyWords": [{ "word": " " }, { "word": "VoiceInk" }],
              "wordReplacements": {
                "": "Ignored",
                "Voice ink": "",
                "Whisper": "Whisper"
              }
            }
            """);

        Assert.Equal(["VoiceInk"], parsed.VocabularyWords);
        Assert.Equal(("Whisper", "Whisper"), parsed.WordReplacements.Single());
    }
}
