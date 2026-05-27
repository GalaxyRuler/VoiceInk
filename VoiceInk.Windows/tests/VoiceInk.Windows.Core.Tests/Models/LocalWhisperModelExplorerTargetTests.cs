using VoiceInk.Windows.Core.Models;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Models;

public sealed class LocalWhisperModelExplorerTargetTests
{
    [Fact]
    public void Create_BlankPath_AsksUserToSelectModel()
    {
        var target = LocalWhisperModelExplorerTarget.Create(
            "   ",
            fileExists: _ => true);

        Assert.False(target.CanOpen);
        Assert.Equal("Select a local model to show in Explorer", target.StatusMessage);
        Assert.Equal(string.Empty, target.FileName);
        Assert.Equal(string.Empty, target.Arguments);
    }

    [Fact]
    public void Create_MissingPath_ReturnsFriendlyStatus()
    {
        var target = LocalWhisperModelExplorerTarget.Create(
            "C:\\Models\\missing.bin",
            fileExists: _ => false);

        Assert.False(target.CanOpen);
        Assert.Equal("Model file not found: C:\\Models\\missing.bin", target.StatusMessage);
    }

    [Fact]
    public void Create_ExistingPath_BuildsExplorerSelectTarget()
    {
        var target = LocalWhisperModelExplorerTarget.Create(
            "  C:\\Models\\ggml-base.en.bin  ",
            fileExists: _ => true);

        Assert.True(target.CanOpen);
        Assert.Equal("explorer.exe", target.FileName);
        Assert.Equal("/select,\"C:\\Models\\ggml-base.en.bin\"", target.Arguments);
        Assert.Equal("Model file opened", target.StatusMessage);
    }
}
