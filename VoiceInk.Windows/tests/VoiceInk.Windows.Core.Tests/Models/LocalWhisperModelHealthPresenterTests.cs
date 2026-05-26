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
}
