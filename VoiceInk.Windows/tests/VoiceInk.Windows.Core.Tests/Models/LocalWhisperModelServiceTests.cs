using VoiceInk.Windows.Core.Models;
using VoiceInk.Windows.Core.Settings;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Models;

public sealed class LocalWhisperModelServiceTests
{
    [Fact]
    public void Import_BinPath_AddsModelWithFileDisplayName()
    {
        var importedAt = DateTimeOffset.Parse("2026-05-24T12:00:00Z");

        var models = LocalWhisperModelService.Import(
            "  C:\\Models\\ggml-base.en.bin  ",
            [],
            importedAt,
            out var error);

        var model = Assert.Single(models);
        Assert.Null(error);
        Assert.Equal("C:\\Models\\ggml-base.en.bin", model.Path);
        Assert.Equal("ggml-base.en", model.DisplayName);
        Assert.Equal(importedAt, model.ImportedAt);
    }

    [Fact]
    public void Import_NonBinPath_ReturnsError()
    {
        var models = LocalWhisperModelService.Import(
            "C:\\Models\\model.txt",
            [],
            DateTimeOffset.UnixEpoch,
            out var error);

        Assert.Empty(models);
        Assert.Equal("Choose a whisper.cpp .bin model file.", error);
    }

    [Fact]
    public void Import_DuplicatePathCaseInsensitive_ReturnsError()
    {
        var existing = new[]
        {
            new LocalWhisperModel("C:\\Models\\ggml-base.en.bin", "ggml-base.en", DateTimeOffset.UnixEpoch)
        };

        var models = LocalWhisperModelService.Import(
            "c:\\models\\GGML-BASE.EN.BIN",
            existing,
            DateTimeOffset.Parse("2026-05-24T12:00:00Z"),
            out var error);

        Assert.Equal(existing, models);
        Assert.Equal("Model is already imported.", error);
    }

    [Fact]
    public void BuildChoices_IncludesCurrentModelPathWhenNotImported()
    {
        var settings = new AppSettings
        {
            ModelPath = "C:\\Models\\ggml-base.en.bin",
            ImportedWhisperModels =
            [
                new LocalWhisperModel("C:\\Models\\ggml-small.en.bin", "ggml-small.en", DateTimeOffset.UnixEpoch)
            ]
        };

        var choices = LocalWhisperModelService.BuildChoices(settings);

        Assert.Collection(
            choices,
            item => Assert.Equal("ggml-small.en", item.DisplayName),
            item =>
            {
                Assert.Equal("ggml-base.en", item.DisplayName);
                Assert.Equal(settings.ModelPath, item.Path);
            });
    }

    [Fact]
    public void BuildChoices_DoesNotDuplicateCurrentModelPathWhenAlreadyImported()
    {
        var importedModel = new LocalWhisperModel(
            "C:\\Models\\ggml-base.en.bin",
            "ggml-base.en",
            DateTimeOffset.UnixEpoch);
        var settings = new AppSettings
        {
            ModelPath = "c:\\models\\GGML-BASE.EN.BIN",
            ImportedWhisperModels = [importedModel]
        };

        var choices = LocalWhisperModelService.BuildChoices(settings);

        Assert.Equal([importedModel], choices);
    }

    [Fact]
    public void BuildChoices_ReturnsImportedModelsWhenCurrentModelPathIsBlank()
    {
        var importedModel = new LocalWhisperModel(
            "C:\\Models\\ggml-base.en.bin",
            "ggml-base.en",
            DateTimeOffset.UnixEpoch);
        var settings = new AppSettings
        {
            ModelPath = "   ",
            ImportedWhisperModels = [importedModel]
        };

        var choices = LocalWhisperModelService.BuildChoices(settings);

        Assert.Equal([importedModel], choices);
    }
}
