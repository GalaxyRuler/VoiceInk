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
            presentation.TutorialSteps,
            step =>
            {
                Assert.Equal("1", step.StepNumber);
                Assert.Equal("Click a text field", step.Title);
                Assert.Equal("Place the cursor where VoiceInk should insert your first dictation.", step.Description);
                Assert.Equal("Waiting", step.StatusBadge);
            },
            step =>
            {
                Assert.Equal("2", step.StepNumber);
                Assert.Equal("Press Ctrl+Alt+Space", step.Title);
                Assert.Equal("Start recording with your primary shortcut.", step.Description);
                Assert.Equal("Waiting", step.StatusBadge);
            },
            step =>
            {
                Assert.Equal("3", step.StepNumber);
                Assert.Equal("Speak a short phrase", step.Title);
                Assert.Equal("Say a sentence you can easily recognize in the target field.", step.Description);
                Assert.Equal("Waiting", step.StatusBadge);
            },
            step =>
            {
                Assert.Equal("4", step.StepNumber);
                Assert.Equal("Press Ctrl+Alt+Space again", step.Title);
                Assert.Equal("Stop recording and let VoiceInk insert the transcription.", step.Description);
                Assert.Equal("Waiting", step.StatusBadge);
            },
            step =>
            {
                Assert.Equal("5", step.StepNumber);
                Assert.Equal("Check insertion and History", step.Title);
                Assert.Equal("Confirm text appears in the target field, then use History to review, paste, retry, or recover the dictation.", step.Description);
                Assert.Equal("Waiting", step.StatusBadge);
            });
        Assert.Collection(
            presentation.SetupActions,
            action =>
            {
                Assert.Equal("Choose Model", action.Title);
                Assert.Equal("Select an existing GGML .bin model or download the recommended starter model.", action.Description);
                Assert.Equal("Browse or Download", action.CommandText);
                Assert.Equal("Required", action.StatusBadge);
            },
            action =>
            {
                Assert.Equal("Check Microphone", action.Title);
                Assert.Equal("Recording input is visible.", action.Description);
                Assert.Equal("Refresh Devices", action.CommandText);
                Assert.Equal("Ready", action.StatusBadge);
            },
            action =>
            {
                Assert.Equal("Set Shortcut", action.Title);
                Assert.Equal("Primary shortcut is configured for system-wide recording.", action.Description);
                Assert.Equal("Edit Shortcut", action.CommandText);
                Assert.Equal("Ready", action.StatusBadge);
            },
            action =>
            {
                Assert.Equal("Try Dictation", action.Title);
                Assert.Equal("Complete required setup, then test insertion in any text field.", action.Description);
                Assert.Equal("Test after Save", action.CommandText);
                Assert.Equal("Next", action.StatusBadge);
            });
        Assert.Collection(
            presentation.SummaryRows,
            row =>
            {
                Assert.Equal("Model", row.Title);
                Assert.Equal("Choose a local Whisper model.", row.Description);
                Assert.Equal("Required", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Microphone", row.Title);
                Assert.Equal("Recording input detected.", row.Description);
                Assert.Equal("Ready", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Shortcut", row.Title);
                Assert.Equal("Ctrl+Alt+Space is ready.", row.Description);
                Assert.Equal("Ready", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("First Dictation", row.Title);
                Assert.Equal("Save setup after required items are ready, then try insertion in any text field.", row.Description);
                Assert.Equal("Try next", row.StatusBadge);
            });
        Assert.Collection(
            presentation.Stages,
            stage =>
            {
                Assert.Equal("1", stage.StepNumber);
                Assert.Equal("Choose Model", stage.Title);
                Assert.Equal(OnboardingChecklistItemState.NeedsAttention, stage.State);
                Assert.Equal("[Needs attention] 1. Choose Model - Pick or download a local Whisper model.", stage.DisplayText);
            },
            stage =>
            {
                Assert.Equal("2", stage.StepNumber);
                Assert.Equal("Check Microphone", stage.Title);
                Assert.Equal(OnboardingChecklistItemState.Ready, stage.State);
            },
            stage =>
            {
                Assert.Equal("3", stage.StepNumber);
                Assert.Equal("Set Shortcut", stage.Title);
                Assert.Equal(OnboardingChecklistItemState.Ready, stage.State);
            },
            stage =>
            {
                Assert.Equal("4", stage.StepNumber);
                Assert.Equal("Try Dictation", stage.Title);
                Assert.Equal(OnboardingChecklistItemState.Advisory, stage.State);
            });

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
        Assert.Equal(["Ready", "Ready", "Ready", "Try next"], presentation.SummaryRows.Select(row => row.StatusBadge).ToArray());
        Assert.Equal(["Ready", "Ready", "Ready", "Ready"], presentation.SetupActions.Select(row => row.StatusBadge).ToArray());
        Assert.Equal("Click Field and Speak", presentation.SetupActions[3].CommandText);
        Assert.Equal(["Ready", "Ready", "Ready", "Ready", "Ready"], presentation.TutorialSteps.Select(step => step.StatusBadge).ToArray());
        Assert.Equal("Press Ctrl+Alt+Space", presentation.TutorialSteps[1].Title);
        Assert.Equal("Press Ctrl+Alt+Space again", presentation.TutorialSteps[3].Title);
        Assert.Equal("Check insertion and History", presentation.TutorialSteps[4].Title);
        Assert.All(presentation.Items.Take(3), item => Assert.Equal(OnboardingChecklistItemState.Ready, item.State));
        Assert.All(presentation.Stages.Take(3), stage => Assert.Equal(OnboardingChecklistItemState.Ready, stage.State));
        Assert.Equal(OnboardingChecklistItemState.Ready, presentation.Stages[3].State);
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
