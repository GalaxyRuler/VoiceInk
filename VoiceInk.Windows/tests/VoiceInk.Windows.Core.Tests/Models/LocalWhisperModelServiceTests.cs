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
    public void RemoveModel_AppLocalModelRemovesReferenceAndPlansFileDeletion()
    {
        var modelsDirectory = "C:\\Users\\Admin\\AppData\\Local\\VoiceInk.Windows\\Models";
        var appLocalModel = new LocalWhisperModel(
            Path.Combine(modelsDirectory, "ggml-base.en.bin"),
            "ggml-base.en",
            DateTimeOffset.UnixEpoch);
        var externalModel = new LocalWhisperModel(
            "D:\\Models\\custom-medical.bin",
            "custom-medical",
            DateTimeOffset.UnixEpoch);

        var result = LocalWhisperModelService.RemoveModel(
            [appLocalModel, externalModel],
            appLocalModel.Path,
            currentModelPath: appLocalModel.Path,
            modelsDirectory);

        Assert.True(result.Removed);
        Assert.Equal([externalModel], result.ImportedModels);
        Assert.Equal(string.Empty, result.ModelPath);
        Assert.Equal(appLocalModel.Path, result.FilePathToDelete);
    }

    [Fact]
    public void RemoveModel_ExternalModelRemovesReferenceWithoutDeletingFile()
    {
        var externalModel = new LocalWhisperModel(
            "D:\\Models\\custom-medical.bin",
            "custom-medical",
            DateTimeOffset.UnixEpoch);

        var result = LocalWhisperModelService.RemoveModel(
            [externalModel],
            externalModel.Path,
            currentModelPath: "C:\\Models\\ggml-base.en.bin",
            appModelsDirectory: "C:\\Users\\Admin\\AppData\\Local\\VoiceInk.Windows\\Models");

        Assert.True(result.Removed);
        Assert.Empty(result.ImportedModels);
        Assert.Equal("C:\\Models\\ggml-base.en.bin", result.ModelPath);
        Assert.Null(result.FilePathToDelete);
    }

    [Fact]
    public void RemoveModel_MissingModelLeavesStateUnchanged()
    {
        var model = new LocalWhisperModel(
            "D:\\Models\\custom-medical.bin",
            "custom-medical",
            DateTimeOffset.UnixEpoch);

        var result = LocalWhisperModelService.RemoveModel(
            [model],
            "D:\\Models\\other.bin",
            currentModelPath: model.Path,
            appModelsDirectory: "C:\\Users\\Admin\\AppData\\Local\\VoiceInk.Windows\\Models");

        Assert.False(result.Removed);
        Assert.Equal([model], result.ImportedModels);
        Assert.Equal(model.Path, result.ModelPath);
        Assert.Null(result.FilePathToDelete);
    }

    [Fact]
    public void RemoveModel_CurrentOnlyModelClearsModelPathAndPlansAppLocalFileDeletion()
    {
        var modelsDirectory = "C:\\Users\\Admin\\AppData\\Local\\VoiceInk.Windows\\Models";
        var currentOnlyPath = Path.Combine(modelsDirectory, "ggml-large-v3-turbo-q5_0.bin");

        var result = LocalWhisperModelService.RemoveModel(
            [],
            currentOnlyPath,
            currentModelPath: currentOnlyPath,
            modelsDirectory);

        Assert.True(result.Removed);
        Assert.Empty(result.ImportedModels);
        Assert.Equal(string.Empty, result.ModelPath);
        Assert.Equal(currentOnlyPath, result.FilePathToDelete);
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
    public void ImportMany_AddsBinModelsAndReportsSkippedPaths()
    {
        var existing = new[]
        {
            new LocalWhisperModel("C:\\Models\\ggml-base.en.bin", "ggml-base.en", DateTimeOffset.UnixEpoch)
        };
        var importedAt = DateTimeOffset.Parse("2026-05-27T10:00:00Z");

        var result = LocalWhisperModelService.ImportMany(
            [
                "C:\\Models\\ggml-small.en.bin",
                "C:\\Models\\notes.txt",
                "c:\\models\\GGML-BASE.EN.BIN",
                "C:\\Models\\ggml-large-v3-turbo-q5_0.bin"
            ],
            existing,
            importedAt);

        Assert.Equal(2, result.ImportedCount);
        Assert.Equal(1, result.SkippedDuplicateCount);
        Assert.Equal(1, result.SkippedInvalidCount);
        Assert.Equal(
            ["ggml-base.en", "ggml-small.en", "ggml-large-v3-turbo-q5_0"],
            result.ImportedModels.Select(model => model.DisplayName).ToArray());
        Assert.All(result.ImportedModels.Skip(1), model => Assert.Equal(importedAt, model.ImportedAt));
    }

    [Theory]
    [InlineData("", LocalWhisperModelHealthStatus.NotSelected, false, "No local model selected.")]
    [InlineData("C:\\Models\\model.txt", LocalWhisperModelHealthStatus.InvalidExtension, false, "Choose a whisper.cpp .bin model file.")]
    [InlineData("C:\\Models\\missing.bin", LocalWhisperModelHealthStatus.Missing, false, "Model file not found: C:\\Models\\missing.bin")]
    [InlineData("C:\\Models\\empty.bin", LocalWhisperModelHealthStatus.Empty, false, "Model file is empty: C:\\Models\\empty.bin")]
    [InlineData("C:\\Models\\tiny.bin", LocalWhisperModelHealthStatus.SuspiciouslySmall, false, "Model file looks too small for a whisper.cpp model: C:\\Models\\tiny.bin")]
    [InlineData("C:\\Models\\ggml-base.en.bin", LocalWhisperModelHealthStatus.Ready, true, "Default Model: ggml-base.en")]
    public void CheckPathHealth_ClassifiesSelectedModelPath(
        string path,
        LocalWhisperModelHealthStatus expectedStatus,
        bool expectedCanUse,
        string expectedMessage)
    {
        var lengths = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            ["C:\\Models\\empty.bin"] = 0,
            ["C:\\Models\\tiny.bin"] = 1024,
            ["C:\\Models\\ggml-base.en.bin"] = 142L * 1024 * 1024
        };

        var health = LocalWhisperModelService.CheckPathHealth(
            path,
            fileExists: item => lengths.ContainsKey(item),
            fileLength: item => lengths[item]);

        Assert.Equal(expectedStatus, health.Status);
        Assert.Equal(expectedCanUse, health.CanUse);
        Assert.Equal(expectedMessage, health.Message);
    }

    [Fact]
    public void CheckPathHealth_WithHeaderReaderRejectsLargeNonGgmlBin()
    {
        var path = "C:\\Models\\not-a-whisper-model.bin";

        var health = LocalWhisperModelService.CheckPathHealth(
            path,
            fileExists: item => item == path,
            fileLength: _ => 142L * 1024 * 1024,
            readHeader: _ => [0x50, 0x4B, 0x03, 0x04]);

        Assert.Equal(LocalWhisperModelHealthStatus.InvalidHeader, health.Status);
        Assert.False(health.CanUse);
        Assert.Equal("Model file is not a whisper.cpp GGML model: C:\\Models\\not-a-whisper-model.bin", health.Message);
    }

    [Fact]
    public void CheckPathHealth_WithHeaderReaderAcceptsGgmlMagic()
    {
        var path = "C:\\Models\\ggml-base.en.bin";

        var health = LocalWhisperModelService.CheckPathHealth(
            path,
            fileExists: item => item == path,
            fileLength: _ => 142L * 1024 * 1024,
            readHeader: _ => [0x6C, 0x6D, 0x67, 0x67]);

        Assert.Equal(LocalWhisperModelHealthStatus.Ready, health.Status);
        Assert.True(health.CanUse);
        Assert.Equal("Default Model: ggml-base.en", health.Message);
    }

    [Fact]
    public void RemoveUnavailableImportedModels_RemovesOnlyUnusablePaths()
    {
        var usable = new LocalWhisperModel("C:\\Models\\ggml-base.en.bin", "ggml-base.en", DateTimeOffset.UnixEpoch);
        var missing = new LocalWhisperModel("C:\\Models\\missing.bin", "missing", DateTimeOffset.UnixEpoch);
        var wrongExtension = new LocalWhisperModel("C:\\Models\\notes.txt", "notes", DateTimeOffset.UnixEpoch);
        var tiny = new LocalWhisperModel("C:\\Models\\tiny.bin", "tiny", DateTimeOffset.UnixEpoch);
        var lengths = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            [usable.Path] = 142L * 1024 * 1024,
            [tiny.Path] = 1024
        };

        var cleaned = LocalWhisperModelService.RemoveUnavailableImportedModels(
            [usable, missing, wrongExtension, tiny],
            fileExists: item => lengths.ContainsKey(item),
            fileLength: item => lengths[item],
            out var removedCount);

        var remaining = Assert.Single(cleaned);
        Assert.Equal(usable, remaining);
        Assert.Equal(3, removedCount);
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
