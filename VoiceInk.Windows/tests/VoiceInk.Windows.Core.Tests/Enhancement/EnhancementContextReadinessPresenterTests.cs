using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.Settings;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Enhancement;

public sealed class EnhancementContextReadinessPresenterTests
{
    [Fact]
    public void Present_WithDefaultSettings_ShowsLocalContextSources()
    {
        var presentation = EnhancementContextReadinessPresenter.Present(new AppSettings());

        Assert.Equal("Context Awareness", presentation.Title);
        Assert.Equal(
            "VoiceInk can add local app, selection, clipboard, and screen text context to enhancement prompts.",
            presentation.Description);
        Assert.Collection(
            presentation.Rows,
            row =>
            {
                Assert.Equal("Clipboard Context", row.Title);
                Assert.Equal("Off", row.Value);
                Assert.Equal("Clipboard text is skipped unless you enable it.", row.Detail);
                Assert.Equal("Off", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Selected Text", row.Title);
                Assert.Equal("Automatic", row.Value);
                Assert.Equal("Reads selected text with clipboard fallback when Windows allows it.", row.Detail);
                Assert.Equal("Best effort", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Active App & Site", row.Title);
                Assert.Equal("Automatic", row.Value);
                Assert.Equal("Adds active window details and sanitized browser URLs when available.", row.Detail);
                Assert.Equal("Best effort", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Screen OCR", row.Title);
                Assert.Equal("Off", row.Value);
                Assert.Equal("Screen text is skipped until OCR context is enabled.", row.Detail);
                Assert.Equal("Off", row.StatusBadge);
            });
    }

    [Fact]
    public void Present_WithClipboardAndFullScreenOcr_ShowsEnabledSources()
    {
        var presentation = EnhancementContextReadinessPresenter.Present(
            new AppSettings
            {
                UseClipboardContext = true,
                UseOcrContext = true,
                UseOcrCaptureRegion = false
            });

        Assert.Contains(
            presentation.Rows,
            row => row.Title == "Clipboard Context"
                && row.Value == "Enabled"
                && row.StatusBadge == "On");
        Assert.Contains(
            presentation.Rows,
            row => row.Title == "Screen OCR"
                && row.Value == "Full screen"
                && row.Detail == "Captures visible screen text locally when enhancement runs."
                && row.StatusBadge == "On");
    }

    [Fact]
    public void Present_WithValidOcrRegion_ShowsRegionSize()
    {
        var presentation = EnhancementContextReadinessPresenter.Present(
            new AppSettings
            {
                UseOcrContext = true,
                UseOcrCaptureRegion = true,
                OcrCaptureRegionWidth = 640,
                OcrCaptureRegionHeight = 480
            });

        Assert.Contains(
            presentation.Rows,
            row => row.Title == "Screen OCR"
                && row.Value == "Region 640 x 480"
                && row.Detail == "Captures only the selected screen region."
                && row.StatusBadge == "Region");
    }

    [Fact]
    public void Present_WithMissingOcrRegion_AsksForRegion()
    {
        var presentation = EnhancementContextReadinessPresenter.Present(
            new AppSettings
            {
                UseOcrContext = true,
                UseOcrCaptureRegion = true
            });

        Assert.Contains(
            presentation.Rows,
            row => row.Title == "Screen OCR"
                && row.Value == "Region required"
                && row.Detail == "Select a screen region before using constrained OCR context."
                && row.StatusBadge == "Needs setup");
    }
}
