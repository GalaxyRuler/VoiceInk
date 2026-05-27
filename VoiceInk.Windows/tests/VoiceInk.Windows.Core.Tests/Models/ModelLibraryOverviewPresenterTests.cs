using VoiceInk.Windows.Core.Models;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Models;

public sealed class ModelLibraryOverviewPresenterTests
{
    [Fact]
    public void Present_EmptyLibrary_ExplainsLocalModelStartingPoint()
    {
        var items = LocalWhisperModelService.BuildCatalogItems([], currentModelPath: string.Empty);

        var presentation = ModelLibraryOverviewPresenter.Present(
            items,
            localModels: [],
            selectedModelPath: string.Empty,
            unavailableImportedModelCount: 0);

        Assert.Equal("Local Whisper Library", presentation.Title);
        Assert.Equal("0 of 8 catalog models available on this device. 0 imported custom models ready. 2 recommended starter models are highlighted.", presentation.Summary);
        Assert.Equal("No default local model selected.", presentation.DefaultModelLabel);
        Assert.Equal("Import a whisper.cpp .bin file or download a recommended model to start private local transcription.", presentation.CleanupHint);
        Assert.Collection(
            presentation.StorageGuidanceRows,
            row =>
            {
                Assert.Equal("Downloaded Models", row.Title);
                Assert.Equal("App-local", row.Value);
                Assert.Equal("Catalog downloads are stored under this Windows profile's VoiceInk Models folder.", row.Detail);
                Assert.Equal("Local", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Imported Models", row.Title);
                Assert.Equal("References", row.Value);
                Assert.Equal("Imported .bin files stay where you selected them; VoiceInk stores only the local reference.", row.Detail);
                Assert.Equal("User-owned", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Settings Backup", row.Title);
                Assert.Equal("No binaries", row.Value);
                Assert.Equal("Backups include model references but do not copy large GGML model files.", row.Detail);
                Assert.Equal("Paths only", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Warmup", row.Title);
                Assert.Equal("After selection", row.Value);
                Assert.Equal("Prewarm loads the selected local model when available to reduce first-use delay.", row.Detail);
                Assert.Equal("Optional", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Expected Filename", row.Title);
                Assert.Equal("ggml-*.bin", row.Value);
                Assert.Equal("Whisper.cpp GGML models commonly use filenames such as ggml-base.en.bin.", row.Detail);
                Assert.Equal("Check import", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Compatibility Check", row.Title);
                Assert.Equal("GGML header", row.Value);
                Assert.Equal("VoiceInk validates selected .bin files before warmup and rejects files that do not look like whisper.cpp GGML models.", row.Detail);
                Assert.Equal("Preflight", row.StatusBadge);
            });
        Assert.Collection(
            presentation.ActionRows,
            row =>
            {
                Assert.Equal("Download Models", row.Title);
                Assert.Equal("0 of 8 available", row.Value);
                Assert.Equal("Download a recommended GGML model for private local transcription.", row.Detail);
                Assert.Equal("Needed", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Imported Models", row.Title);
                Assert.Equal("0 ready", row.Value);
                Assert.Equal("Import an existing whisper.cpp .bin model from disk.", row.Detail);
                Assert.Equal("Optional", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Default Model", row.Title);
                Assert.Equal("Not selected", row.Value);
                Assert.Equal("Set a downloaded or imported model as the default before recording.", row.Detail);
                Assert.Equal("Required", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Repair & Warmup", row.Title);
                Assert.Equal("After model select", row.Value);
                Assert.Equal("Warmup becomes available once the default model path is usable.", row.Detail);
                Assert.Equal("Waiting", row.StatusBadge);
            });
    }

    [Fact]
    public void Present_ShowsCompatibilityCheckGuidance()
    {
        var items = LocalWhisperModelService.BuildCatalogItems([], currentModelPath: string.Empty);

        var presentation = ModelLibraryOverviewPresenter.Present(
            items,
            localModels: [],
            selectedModelPath: string.Empty,
            unavailableImportedModelCount: 0);

        Assert.Contains(
            presentation.StorageGuidanceRows,
            row => row.Title == "Compatibility Check"
                && row.Value == "GGML header"
                && row.Detail == "VoiceInk validates selected .bin files before warmup and rejects files that do not look like whisper.cpp GGML models."
                && row.StatusBadge == "Preflight");
    }

    [Fact]
    public void Present_PopulatedLibrary_ShowsDefaultAndReadyState()
    {
        var model = new LocalWhisperModel(
            "C:\\Models\\ggml-base.en.bin",
            "ggml-base.en",
            DateTimeOffset.UnixEpoch);
        var items = LocalWhisperModelService.BuildCatalogItems([model], model.Path);

        var presentation = ModelLibraryOverviewPresenter.Present(
            items,
            localModels: [model],
            selectedModelPath: model.Path,
            unavailableImportedModelCount: 0);

        Assert.Equal("1 of 8 catalog models available on this device. 0 imported custom models ready. 2 recommended starter models are highlighted.", presentation.Summary);
        Assert.Equal("Default local model: Base (English).", presentation.DefaultModelLabel);
        Assert.Equal("Model library is ready. Downloaded and imported model references stay local to this Windows profile.", presentation.CleanupHint);
        Assert.Equal(["Available", "Optional", "Ready", "Warmup"], presentation.ActionRows.Select(row => row.StatusBadge).ToArray());
        Assert.Equal("Base (English)", presentation.ActionRows[2].Value);
    }

    [Fact]
    public void Present_UnavailableImports_ShowsCleanupHint()
    {
        var model = new LocalWhisperModel(
            "C:\\Models\\ggml-base.en.bin",
            "ggml-base.en",
            DateTimeOffset.UnixEpoch);
        var items = LocalWhisperModelService.BuildCatalogItems([model], model.Path);

        var presentation = ModelLibraryOverviewPresenter.Present(
            items,
            localModels: [],
            selectedModelPath: model.Path,
            unavailableImportedModelCount: 3);

        Assert.Equal("0 of 8 catalog models available on this device. 0 imported custom models ready. 2 recommended starter models are highlighted.", presentation.Summary);
        Assert.Equal("3 unavailable imported model references can be removed from settings without deleting model files.", presentation.CleanupHint);
        Assert.Contains(
            presentation.ActionRows,
            row => row.Title == "Repair & Warmup"
                && row.Value == "3 stale references"
                && row.StatusBadge == "Repair");
    }

    [Fact]
    public void Present_CustomImportedDefault_ShowsCustomModelCountAndDefault()
    {
        var custom = new LocalWhisperModel(
            "D:\\Models\\custom-medical.bin",
            "custom-medical",
            DateTimeOffset.UnixEpoch);
        var items = LocalWhisperModelService.BuildCatalogItems([custom], custom.Path);

        var presentation = ModelLibraryOverviewPresenter.Present(
            items,
            localModels: [custom],
            selectedModelPath: custom.Path,
            unavailableImportedModelCount: 0);

        Assert.Equal("0 of 8 catalog models available on this device. 1 imported custom model ready. 2 recommended starter models are highlighted.", presentation.Summary);
        Assert.Equal("Default local model: custom-medical.", presentation.DefaultModelLabel);
        Assert.Equal("custom-medical", presentation.ActionRows[2].Value);
    }
}
