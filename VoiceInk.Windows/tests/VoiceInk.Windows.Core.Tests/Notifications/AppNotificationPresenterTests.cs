using VoiceInk.Windows.Core.Notifications;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Notifications;

public sealed class AppNotificationPresenterTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Idle")]
    [InlineData("Recording")]
    [InlineData("Transcribing")]
    [InlineData("Inserting")]
    [InlineData("Loading settings")]
    public void FromStatus_IgnoresPassiveStatusText(string? status)
    {
        var presentation = AppNotificationPresenter.FromStatus(status);

        Assert.Null(presentation);
    }

    [Theory]
    [InlineData("Model selection failed: file missing")]
    [InlineData("Tray audio input is no longer available")]
    [InlineData("Audio file not found")]
    [InlineData("OCR region width and height must be positive.")]
    public void FromStatus_ClassifiesErrorMessages(string status)
    {
        var presentation = AppNotificationPresenter.FromStatus(status);

        Assert.NotNull(presentation);
        Assert.Equal(AppNotificationKind.Error, presentation.Kind);
        Assert.Equal(status, presentation.Message);
        Assert.Equal(TimeSpan.FromSeconds(5), presentation.Duration);
    }

    [Theory]
    [InlineData("History export canceled")]
    [InlineData("Select a transcription to copy")]
    [InlineData("Choose a microphone to add to priority")]
    [InlineData("Power Mode rule needs a match")]
    public void FromStatus_ClassifiesWarningMessages(string status)
    {
        var presentation = AppNotificationPresenter.FromStatus(status);

        Assert.NotNull(presentation);
        Assert.Equal(AppNotificationKind.Warning, presentation.Kind);
        Assert.Equal(TimeSpan.FromSeconds(4), presentation.Duration);
    }

    [Theory]
    [InlineData("Settings saved")]
    [InlineData("Audio inputs refreshed")]
    [InlineData("Dictionary exported: words.csv")]
    [InlineData("Transcription deleted")]
    public void FromStatus_ClassifiesSuccessMessages(string status)
    {
        var presentation = AppNotificationPresenter.FromStatus(status);

        Assert.NotNull(presentation);
        Assert.Equal(AppNotificationKind.Success, presentation.Kind);
        Assert.Equal(TimeSpan.FromSeconds(3), presentation.Duration);
    }

    [Fact]
    public void FromStatus_DefaultsToInfoForActionableStatus()
    {
        var presentation = AppNotificationPresenter.FromStatus("Press shortcut keys, or Escape to clear");

        Assert.NotNull(presentation);
        Assert.Equal(AppNotificationKind.Info, presentation.Kind);
        Assert.Equal(TimeSpan.FromSeconds(3), presentation.Duration);
    }
}
