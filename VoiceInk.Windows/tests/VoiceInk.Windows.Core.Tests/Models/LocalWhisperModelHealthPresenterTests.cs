using VoiceInk.Windows.Core.Models;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Models;

public sealed class LocalWhisperModelHealthPresenterTests
{
    [Theory]
    [InlineData(LocalWhisperModelHealthStatus.NotSelected, "Choose a local Whisper model", "Download a recommended GGML model or import an existing whisper.cpp .bin file.", "Import .bin Model")]
    [InlineData(LocalWhisperModelHealthStatus.InvalidExtension, "Model file type is not supported", "Choose a whisper.cpp GGML .bin model file.", "Import .bin Model")]
    [InlineData(LocalWhisperModelHealthStatus.Missing, "Model file is missing", "Re-import the model from its new location or download a fresh GGML model.", "Repair Model Path")]
    [InlineData(LocalWhisperModelHealthStatus.Empty, "Model file is empty", "Delete this broken file and download or import a complete GGML model.", "Replace Model")]
    [InlineData(LocalWhisperModelHealthStatus.SuspiciouslySmall, "Model file looks incomplete", "Replace it with a complete whisper.cpp GGML model before recording.", "Replace Model")]
    [InlineData(LocalWhisperModelHealthStatus.InvalidHeader, "Model file is not GGML", "Replace it with a whisper.cpp GGML .bin model before recording.", "Replace Model")]
    [InlineData(LocalWhisperModelHealthStatus.Ready, "Local model is ready", "You can warm up this model to reduce first transcription latency.", "Warm Up Model")]
    public void Present_MapsHealthStatusToRepairGuidance(
        LocalWhisperModelHealthStatus status,
        string expectedTitle,
        string expectedGuidance,
        string expectedAction)
    {
        var health = new LocalWhisperModelHealth(status, "health message", status == LocalWhisperModelHealthStatus.Ready);

        var presentation = LocalWhisperModelHealthPresenter.Present(health);

        Assert.Equal(expectedTitle, presentation.Title);
        Assert.Equal(expectedGuidance, presentation.Guidance);
        Assert.Equal(expectedAction, presentation.ActionLabel);
        Assert.Equal(status == LocalWhisperModelHealthStatus.Ready, presentation.CanWarmup);
        Assert.Equal(
            status == LocalWhisperModelHealthStatus.Ready
                ? LocalWhisperModelRepairAction.Warmup
                : LocalWhisperModelRepairAction.ImportReplacement,
            presentation.RepairAction);
    }

    [Fact]
    public void Present_ReadyModel_ShowsWarmupAndStorageGuidanceRows()
    {
        var health = new LocalWhisperModelHealth(
            LocalWhisperModelHealthStatus.Ready,
            "Default Model: ggml-base.en",
            CanUse: true);

        var presentation = LocalWhisperModelHealthPresenter.Present(health);

        Assert.Collection(
            presentation.GuidanceRows,
            row =>
            {
                Assert.Equal("Model File", row.Title);
                Assert.Equal("Ready", row.Value);
                Assert.Equal("The selected whisper.cpp .bin file can be used for local transcription.", row.Detail);
                Assert.Equal("Usable", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Filename", row.Title);
                Assert.Equal("ggml-*.bin", row.Value);
                Assert.Equal("Whisper.cpp models usually use names like ggml-base.en.bin.", row.Detail);
                Assert.Equal("Expected", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Warmup", row.Title);
                Assert.Equal("Available", row.Value);
                Assert.Equal("Prewarm can load this model before the first recording to reduce startup latency.", row.Detail);
                Assert.Equal("Optional", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Storage", row.Title);
                Assert.Equal("Local path", row.Value);
                Assert.Equal("VoiceInk keeps the model on this Windows profile and does not upload it for local transcription.", row.Detail);
                Assert.Equal("Local", row.StatusBadge);
            });
    }

    [Fact]
    public void Present_BrokenModel_ShowsRepairGuidanceRows()
    {
        var health = new LocalWhisperModelHealth(
            LocalWhisperModelHealthStatus.Missing,
            "Model file not found: C:\\Models\\missing.bin",
            CanUse: false);

        var presentation = LocalWhisperModelHealthPresenter.Present(health);

        Assert.Collection(
            presentation.GuidanceRows,
            row =>
            {
                Assert.Equal("Model File", row.Title);
                Assert.Equal("Unavailable", row.Value);
                Assert.Equal("VoiceInk cannot use this model until the path points to a complete .bin file.", row.Detail);
                Assert.Equal("Repair", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Filename", row.Title);
                Assert.Equal("ggml-*.bin", row.Value);
                Assert.Equal("Import a whisper.cpp GGML file such as ggml-base.en.bin.", row.Detail);
                Assert.Equal("Expected", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Repair Picker", row.Title);
                Assert.Equal("User selected", row.Value);
                Assert.Equal("Repair validates the .bin file you choose and does not scan folders automatically.", row.Detail);
                Assert.Equal("Explicit", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Warmup", row.Title);
                Assert.Equal("Blocked", row.Value);
                Assert.Equal("Warmup is disabled until the selected model path is usable.", row.Detail);
                Assert.Equal("Waiting", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Storage", row.Title);
                Assert.Equal("User-owned path", row.Value);
                Assert.Equal("Repairing the reference does not delete model files from disk.", row.Detail);
                Assert.Equal("Safe repair", row.StatusBadge);
            });
    }
}
