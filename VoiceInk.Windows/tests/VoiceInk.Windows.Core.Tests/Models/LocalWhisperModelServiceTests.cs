using VoiceInk.Windows.Core.Models;
using VoiceInk.Windows.Core.Settings;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Models;

public sealed class LocalWhisperModelServiceTests
{
    [Fact]
    public void Catalog_All_ReturnsMacOsWhisperModelsInRegistryOrder()
    {
        Assert.Equal(
            [
                "ggml-tiny",
                "ggml-tiny.en",
                "ggml-base",
                "ggml-base.en",
                "ggml-large-v2",
                "ggml-large-v3",
                "ggml-large-v3-turbo",
                "ggml-large-v3-turbo-q5_0"
            ],
            WhisperModelCatalog.All.Select(model => model.Name));
    }

    [Fact]
    public void Catalog_BaseEnglish_UsesMacOsMetadataAndWhisperCppDownloadUrl()
    {
        var model = WhisperModelCatalog.All.Single(model => model.Name == "ggml-base.en");

        Assert.Equal("Base (English)", model.DisplayName);
        Assert.Equal("English-only", model.LanguageDisplay);
        Assert.Equal("142 MB", model.Size);
        Assert.Equal("Base model optimized for English, good balance between speed and accuracy", model.Description);
        Assert.Equal(8.5, model.SpeedScore);
        Assert.Equal(7.5, model.AccuracyScore);
        Assert.Equal("ggml-base.en.bin", model.FileName);
        Assert.Equal(
            "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.en.bin",
            model.DownloadUri.AbsoluteUri);
    }

    [Fact]
    public void Catalog_Recommended_ReturnsMacOsRecommendedLocalWhisperModels()
    {
        Assert.Equal(
            ["ggml-base.en", "ggml-large-v3-turbo-q5_0"],
            WhisperModelCatalog.Recommended.Select(model => model.Name));
    }

    [Fact]
    public void BuildCatalogItems_MarksRecommendedDownloadedAndDefaultModels()
    {
        var modelsDirectory = "C:\\Users\\Admin\\AppData\\Local\\VoiceInk.Windows\\Models";
        var downloadedModel = new LocalWhisperModel(
            Path.Combine(modelsDirectory, "ggml-base.en.bin"),
            "ggml-base.en",
            DateTimeOffset.UnixEpoch);
        var importedModel = new LocalWhisperModel(
            "D:\\Models\\custom-medical.bin",
            "custom-medical",
            DateTimeOffset.UnixEpoch);

        var items = LocalWhisperModelService.BuildCatalogItems(
            [downloadedModel, importedModel],
            downloadedModel.Path);

        var baseEnglish = items.Single(item => item.Name == "ggml-base.en");
        var largeTurboQuantized = items.Single(item => item.Name == "ggml-large-v3-turbo-q5_0");
        var tiny = items.Single(item => item.Name == "ggml-tiny");

        Assert.True(baseEnglish.IsRecommended);
        Assert.True(baseEnglish.IsDownloaded);
        Assert.True(baseEnglish.IsDefault);
        Assert.Equal(downloadedModel.Path, baseEnglish.LocalPath);
        Assert.Equal("Default Model", baseEnglish.Status);
        Assert.Equal("Set as Default", baseEnglish.PrimaryActionLabel);
        Assert.True(largeTurboQuantized.IsRecommended);
        Assert.False(largeTurboQuantized.IsDownloaded);
        Assert.False(tiny.IsRecommended);
        Assert.Equal("Download", tiny.PrimaryActionLabel);
    }

    [Fact]
    public void AddOrReplaceCatalogModel_ReplacesExistingModelWithSameDisplayName()
    {
        var previous = new LocalWhisperModel(
            "D:\\Models\\ggml-base.en.bin",
            "ggml-base.en",
            DateTimeOffset.UnixEpoch);
        var other = new LocalWhisperModel(
            "D:\\Models\\custom-medical.bin",
            "custom-medical",
            DateTimeOffset.UnixEpoch);
        var downloaded = new LocalWhisperModel(
            "C:\\Users\\Admin\\AppData\\Local\\VoiceInk.Windows\\Models\\ggml-base.en.bin",
            "ggml-base.en",
            DateTimeOffset.Parse("2026-05-25T12:00:00Z"));

        var models = LocalWhisperModelService.AddOrReplaceCatalogModel([previous, other], downloaded);

        Assert.Collection(
            models,
            item => Assert.Equal(downloaded, item),
            item => Assert.Equal(other, item));
    }

    [Fact]
    public void LanguageChoices_ForEnglishOnlyCatalogModel_ReturnsEnglishOnly()
    {
        var choices = WhisperLanguageCatalog.ChoicesForModelPath(
            "C:\\Models\\ggml-base.en.bin",
            []);

        var choice = Assert.Single(choices);
        Assert.Equal("en", choice.Code);
        Assert.Equal("English", choice.DisplayName);
    }

    [Fact]
    public void LanguageChoices_ForMultilingualCatalogModel_ReturnsAutoFirstAndWhisperLanguages()
    {
        var choices = WhisperLanguageCatalog.ChoicesForModelPath(
            "C:\\Models\\ggml-base.bin",
            []);

        Assert.Equal("auto", choices[0].Code);
        Assert.Equal("Auto-detect", choices[0].DisplayName);
        Assert.Contains(choices, item => item.Code == "en" && item.DisplayName == "English");
        Assert.Contains(choices, item => item.Code == "fr" && item.DisplayName == "French");
        Assert.Contains(choices, item => item.Code == "de" && item.DisplayName == "German");
        Assert.Contains(choices, item => item.Code == "ja" && item.DisplayName == "Japanese");
        Assert.Contains(choices, item => item.Code == "zh" && item.DisplayName == "Chinese");
    }

    [Fact]
    public void LanguageChoices_ForImportedUnknownModel_TreatsModelAsMultilingual()
    {
        var imported = new LocalWhisperModel(
            "D:\\Models\\custom-medical.bin",
            "custom-medical",
            DateTimeOffset.UnixEpoch);

        var choices = WhisperLanguageCatalog.ChoicesForModelPath(imported.Path, [imported]);

        Assert.Equal("auto", choices[0].Code);
        Assert.Contains(choices, item => item.Code == "en");
        Assert.Contains(choices, item => item.Code == "es");
    }

    [Theory]
    [InlineData("C:\\Models\\ggml-base.bin", "fr", "fr")]
    [InlineData("C:\\Models\\ggml-base.bin", "", "auto")]
    [InlineData("C:\\Models\\ggml-base.bin", "zz", "auto")]
    [InlineData("C:\\Models\\ggml-base.en.bin", "fr", "en")]
    [InlineData("C:\\Models\\ggml-base.en.bin", "auto", "en")]
    public void CompatibleLanguageOrFallback_UsesModelCapabilities(
        string modelPath,
        string selectedLanguage,
        string expectedLanguage)
    {
        var language = WhisperLanguageCatalog.CompatibleLanguageOrFallback(
            modelPath,
            [],
            selectedLanguage);

        Assert.Equal(expectedLanguage, language);
    }

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
