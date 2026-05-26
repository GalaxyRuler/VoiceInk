using VoiceInk.Windows.Core.Onboarding;
using VoiceInk.Windows.Core.Settings;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Onboarding;

public sealed class OnboardingChecklistPresenterTests
{
    [Fact]
    public void Present_IncompleteSetup_ShowsRequiredProgressAndNextAction()
    {
        var status = OnboardingSetupStatusService.Build(new AppSettings(), hasAudioInputChoices: true);

        var presentation = OnboardingChecklistPresenter.Present(status);

        Assert.Equal("Welcome to VoiceInk", presentation.Title);
        Assert.Equal("Set up your local model, microphone, and shortcut once, then dictate anywhere from the tray or keyboard.", presentation.Description);
        Assert.Equal("2 of 5 setup essentials ready", presentation.ProgressLabel);
        Assert.Equal("Choose or download a local Whisper model to continue.", presentation.NextAction);
        Assert.False(presentation.CanSaveSetup);

        Assert.Collection(
            presentation.Items,
            item =>
            {
                Assert.Equal("Local Whisper model", item.Title);
                Assert.Equal(OnboardingChecklistItemState.NeedsAttention, item.State);
            },
            item =>
            {
                Assert.Equal("Primary recording shortcut", item.Title);
                Assert.Equal(OnboardingChecklistItemState.Ready, item.State);
            },
            item =>
            {
                Assert.Equal("Microphone input", item.Title);
                Assert.Equal(OnboardingChecklistItemState.Ready, item.State);
            },
            item =>
            {
                Assert.Equal("Windows microphone privacy", item.Title);
                Assert.Equal(OnboardingChecklistItemState.Advisory, item.State);
            },
            item =>
            {
                Assert.Equal("First dictation test", item.Title);
                Assert.Equal(OnboardingChecklistItemState.Advisory, item.State);
            });
    }

    [Fact]
    public void Present_CompleteSetup_ShowsReadyProgressAndSmokePath()
    {
        var status = OnboardingSetupStatusService.Build(
            new AppSettings
            {
                ModelPath = "C:\\Models\\ggml-base.en.bin",
                Hotkey = "Ctrl+Alt+Space"
            },
            hasAudioInputChoices: true);

        var presentation = OnboardingChecklistPresenter.Present(status);

        Assert.True(presentation.CanSaveSetup);
        Assert.Equal("4 of 5 setup essentials ready", presentation.ProgressLabel);
        Assert.Equal("Save setup, click a text field, press your shortcut, speak, then press it again to insert text.", presentation.NextAction);
        Assert.All(presentation.Items.Take(3), item => Assert.Equal(OnboardingChecklistItemState.Ready, item.State));
    }

    [Fact]
    public void Present_MissingMicrophone_KeepsSetupCompletableWithAdvisory()
    {
        var status = OnboardingSetupStatusService.Build(
            new AppSettings
            {
                ModelPath = "C:\\Models\\ggml-base.en.bin",
                Hotkey = "Ctrl+Alt+Space"
            },
            hasAudioInputChoices: false);

        var presentation = OnboardingChecklistPresenter.Present(status);
        var microphoneItem = presentation.Items.Single(item => item.Title == "Microphone input");

        Assert.True(presentation.CanSaveSetup);
        Assert.Equal("2 of 5 setup essentials ready", presentation.ProgressLabel);
        Assert.Equal(OnboardingChecklistItemState.NeedsAttention, microphoneItem.State);
        Assert.Equal("No input is visible yet. Refresh devices or open Windows microphone privacy settings before your first recording.", microphoneItem.Description);
        Assert.Equal("Save setup after checking your microphone, then run the first dictation test.", presentation.NextAction);
    }
}
