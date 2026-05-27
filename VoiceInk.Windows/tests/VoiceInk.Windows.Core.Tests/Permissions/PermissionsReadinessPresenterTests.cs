using VoiceInk.Windows.Core.Permissions;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Text;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Permissions;

public sealed class PermissionsReadinessPresenterTests
{
    [Fact]
    public void Build_ReturnsMacParityPermissionCardsWithWindowsActions()
    {
        var settings = new AppSettings
        {
            Hotkey = "Ctrl+Alt+Space",
            PasteMethod = PasteMethodSettings.DirectText,
            UseOcrContext = true,
            UseOcrCaptureRegion = true,
            OcrCaptureRegionWidth = 640,
            OcrCaptureRegionHeight = 480
        };

        var status = PermissionsReadinessPresenter.Build(settings, hasAudioInputChoices: true);

        Assert.Equal("Ready", status.SummarySeverity);
        Assert.Collection(
            status.Items,
            item =>
            {
                Assert.Equal("Keyboard Shortcut", item.Title);
                Assert.Equal("Configured", item.Status);
                Assert.True(item.IsReady);
                Assert.Equal("Settings", item.ActionTarget);
            },
            item =>
            {
                Assert.Equal("Microphone Access", item.Title);
                Assert.Equal("Audio input available", item.Status);
                Assert.True(item.IsReady);
                Assert.Equal("ms-settings:privacy-microphone", item.ActionTarget);
                Assert.Equal("Manual path: Settings > Privacy & security > Microphone.", item.FallbackGuidance);
            },
            item =>
            {
                Assert.Equal("App Microphone Capability", item.Title);
                Assert.Equal("Declared", item.Status);
                Assert.True(item.IsReady);
                Assert.Equal("Package Manifest", item.ActionTarget);
                Assert.Equal("Source check: Package.appxmanifest declares DeviceCapability Name=\"microphone\".", item.FallbackGuidance);
            },
            item =>
            {
                Assert.Equal("Text Insertion", item.Title);
                Assert.Equal("Direct text", item.Status);
                Assert.True(item.IsReady);
                Assert.Equal("Settings", item.ActionTarget);
            },
            item =>
            {
                Assert.Equal("Screen Context", item.Title);
                Assert.Equal("OCR region configured", item.Status);
                Assert.True(item.IsReady);
                Assert.Equal("Enhancement", item.ActionTarget);
            });
    }

    [Fact]
    public void Build_MarksMissingSetupAsAttentionNeeded()
    {
        var status = PermissionsReadinessPresenter.Build(
            new AppSettings
            {
                Hotkey = " ",
                UseOcrContext = true,
                UseOcrCaptureRegion = true
            },
            hasAudioInputChoices: false);

        Assert.Equal("Needs attention", status.SummarySeverity);
        Assert.Contains(status.Items, item => item.Title == "Keyboard Shortcut" && !item.IsReady);
        Assert.Contains(status.Items, item => item.Title == "Microphone Access" && !item.IsReady);
        Assert.Contains(status.Items, item => item.Title == "Screen Context" && !item.IsReady);
        Assert.Contains(status.Items, item => item.Title == "App Microphone Capability" && item.IsReady);
    }

    [Fact]
    public void Build_ItemsExposeAccessibleNames()
    {
        var status = PermissionsReadinessPresenter.Build(new AppSettings(), hasAudioInputChoices: false);

        Assert.Equal(
            "App Microphone Capability, Declared, VoiceInk's packaged manifest declares microphone capture capability for signed MSIX builds., Source check: Package.appxmanifest declares DeviceCapability Name=\"microphone\".",
            status.Items[2].AccessibleName);
    }
}
