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
        Assert.Equal(
            [
                "Welcome to the Future of Typing",
                "A New Way to Type",
                "Your Writing Assistant",
                "Works everywhere on Windows with your shortcut",
                "100% offline and private with local models"
            ],
            presentation.HeroTaglines);
        Assert.Equal("Set up your local model, microphone, and shortcut once, then dictate anywhere from the tray or keyboard.", presentation.Description);
        Assert.Equal("2 of 7 setup essentials ready", presentation.ProgressLabel);
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
                Assert.Equal("Manual Privacy Path", action.Title);
                Assert.Equal("Use Settings > Privacy & security > Microphone, then check both Microphone access and 'Let desktop apps access your microphone' if VoiceInk is not listed; Windows may show recent desktop app microphone activity there.", action.Description);
                Assert.Equal("Open Manually", action.CommandText);
                Assert.Equal("Fallback", action.StatusBadge);
            },
            action =>
            {
                Assert.Equal("Text Insertion", action.Title);
                Assert.Equal("Click a target field before recording. VoiceInk pastes the transcript there and keeps a History copy for recovery.", action.Description);
                Assert.Equal("Review Flow", action.CommandText);
                Assert.Equal("Review", action.StatusBadge);
            },
            action =>
            {
                Assert.Equal("Context Awareness", action.Title);
                Assert.Equal("Optional local screen OCR and clipboard context are default-off. Enable them later when you want extra context for enhancement.", action.Description);
                Assert.Equal("Review Later", action.CommandText);
                Assert.Equal("Optional", action.StatusBadge);
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
                Assert.Equal("Windows Permission", row.Title);
                Assert.Equal("Keep Microphone access and 'Let desktop apps access your microphone' enabled; source-built VoiceInk may not be listed by name.", row.Description);
                Assert.Equal("Review", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Text Insertion", row.Title);
                Assert.Equal("Uses clipboard paste into the focused field; recover from History if the target app rejects insertion.", row.Description);
                Assert.Equal("Review", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Context Awareness", row.Title);
                Assert.Equal("Windows asks for capture consent when screen OCR context is enabled and used.", row.Description);
                Assert.Equal("Optional", row.StatusBadge);
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
                Assert.Equal("Text insertion", item.Title);
                Assert.Equal(OnboardingChecklistItemState.Advisory, item.State);
            },
            item =>
            {
                Assert.Equal("Context awareness", item.Title);
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
        Assert.Equal("4 of 7 setup essentials ready", presentation.ProgressLabel);
        Assert.Equal("Save setup, click a text field, press your shortcut, speak, then press it again to insert text.", presentation.NextAction);
        Assert.Equal(["Ready", "Ready", "Review", "Review", "Optional", "Ready", "Try next"], presentation.SummaryRows.Select(row => row.StatusBadge).ToArray());
        Assert.Equal(["Ready", "Ready", "Fallback", "Review", "Optional", "Ready", "Ready"], presentation.SetupActions.Select(row => row.StatusBadge).ToArray());
        Assert.Equal("Click Field and Speak", presentation.SetupActions[6].CommandText);
        Assert.Equal(["Ready", "Ready", "Ready", "Ready", "Ready"], presentation.TutorialSteps.Select(step => step.StatusBadge).ToArray());
        Assert.Equal("Press Ctrl+Alt+Space", presentation.TutorialSteps[1].Title);
        Assert.Equal("Press Ctrl+Alt+Space again", presentation.TutorialSteps[3].Title);
        Assert.Equal("Check insertion and History", presentation.TutorialSteps[4].Title);
        Assert.All(presentation.Items.Take(3), item => Assert.Equal(OnboardingChecklistItemState.Ready, item.State));
        Assert.All(presentation.Stages.Take(3), stage => Assert.Equal(OnboardingChecklistItemState.Ready, stage.State));
        Assert.Equal(OnboardingChecklistItemState.Ready, presentation.Stages[3].State);
    }

    [Fact]
    public void Present_ExposesCurrentSetupFocusPanel()
    {
        var missingModelStatus = OnboardingSetupStatusService.Build(
            new AppSettings
            {
                Hotkey = "Ctrl+Alt+Space"
            },
            hasAudioInputChoices: true);

        var missingModelPresentation = OnboardingChecklistPresenter.Present(missingModelStatus);

        Assert.Equal("Step 1 of 4", missingModelPresentation.CurrentStageLabel);
        Assert.Equal("Choose Model", missingModelPresentation.CurrentStageTitle);
        Assert.Equal("Pick or download a local Whisper model.", missingModelPresentation.CurrentStageDescription);
        Assert.Equal("Needs attention", missingModelPresentation.CurrentStageStatusBadge);
        Assert.Equal(
            "Step 1 of 4, Choose Model, Needs attention, Pick or download a local Whisper model.",
            missingModelPresentation.CurrentStageAccessibleName);

        var readyStatus = OnboardingSetupStatusService.Build(
            new AppSettings
            {
                ModelPath = "C:\\Models\\ggml-base.en.bin",
                Hotkey = "Ctrl+Alt+Space"
            },
            hasAudioInputChoices: true);

        var readyPresentation = OnboardingChecklistPresenter.Present(readyStatus);

        Assert.Equal("Step 4 of 4", readyPresentation.CurrentStageLabel);
        Assert.Equal("Try Dictation", readyPresentation.CurrentStageTitle);
        Assert.Equal("Ready for a first focused-text-field smoke test.", readyPresentation.CurrentStageDescription);
        Assert.Equal("Ready", readyPresentation.CurrentStageStatusBadge);
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
        Assert.Equal("2 of 7 setup essentials ready", presentation.ProgressLabel);
        Assert.Equal(OnboardingChecklistItemState.NeedsAttention, microphoneItem.State);
        Assert.Equal("No input is visible yet. Refresh devices or open Windows microphone privacy settings before your first recording.", microphoneItem.Description);
        Assert.Contains(
            presentation.SummaryRows,
            row => row.Title == "Windows Permission"
                && row.Description == "Open Microphone privacy settings; VoiceInk may rely on 'Let desktop apps access your microphone' even when it is not listed by name."
                && row.StatusBadge == "Check");
        Assert.Equal("Save setup after checking your microphone, then run the first dictation test.", presentation.NextAction);
    }

    [Fact]
    public void Present_AlwaysShowsOptionalContextAwarenessGuidance()
    {
        var status = OnboardingSetupStatusService.Build(
            new AppSettings
            {
                ModelPath = "C:\\Models\\ggml-base.en.bin",
                Hotkey = "Ctrl+Alt+Space"
            },
            hasAudioInputChoices: true);

        var presentation = OnboardingChecklistPresenter.Present(status);

        Assert.Contains(
            presentation.SetupActions,
            action => action.Title == "Context Awareness"
                && action.Description == "Optional local screen OCR and clipboard context are default-off. Enable them later when you want extra context for enhancement."
                && action.CommandText == "Review Later"
                && action.StatusBadge == "Optional");
        Assert.Contains(
            presentation.SummaryRows,
            row => row.Title == "Context Awareness"
                && row.Description == "Windows asks for capture consent when screen OCR context is enabled and used."
                && row.StatusBadge == "Optional");
        Assert.Contains(
            presentation.Items,
            item => item.Title == "Context awareness"
                && item.Description == "Optional local context stays default-off during setup; Windows will ask before screen OCR captures a window or display."
                && item.State == OnboardingChecklistItemState.Advisory);
        Assert.Equal("4 of 7 setup essentials ready", presentation.ProgressLabel);
        Assert.True(presentation.CanSaveSetup);
    }

    [Fact]
    public void Present_ShowsDesktopAppMicrophonePrivacyBoundary()
    {
        var status = OnboardingSetupStatusService.Build(
            new AppSettings
            {
                ModelPath = "C:\\Models\\ggml-base.en.bin",
                Hotkey = "Ctrl+Alt+Space"
            },
            hasAudioInputChoices: false);

        var presentation = OnboardingChecklistPresenter.Present(status);

        Assert.Contains(
            presentation.Items,
            item => item.Title == "Windows microphone privacy"
                && item.Description == "VoiceInk may not appear as a separate app entry while source-built; keep Microphone access and 'Let desktop apps access your microphone' enabled.");
        Assert.Contains(
            presentation.SummaryRows,
            row => row.Title == "Windows Permission"
                && row.Description == "Open Microphone privacy settings; VoiceInk may rely on 'Let desktop apps access your microphone' even when it is not listed by name.");
        Assert.Contains(
            presentation.SetupActions,
            action => action.Title == "Manual Privacy Path"
                && action.Description == "Use Settings > Privacy & security > Microphone, then check both Microphone access and 'Let desktop apps access your microphone' if VoiceInk is not listed; Windows may show recent desktop app microphone activity there.");
    }

    [Fact]
    public void Present_ShowsWindowsTextInsertionReadinessGuidance()
    {
        var status = OnboardingSetupStatusService.Build(
            new AppSettings
            {
                ModelPath = "C:\\Models\\ggml-base.en.bin",
                Hotkey = "Ctrl+Alt+Space"
            },
            hasAudioInputChoices: true);

        var presentation = OnboardingChecklistPresenter.Present(status);

        Assert.Contains(
            presentation.Items,
            item => item.Title == "Text insertion"
                && item.Description == "VoiceInk inserts through the focused field by using clipboard paste; History keeps the transcript available if the target app blocks paste or loses focus."
                && item.State == OnboardingChecklistItemState.Advisory);
        Assert.Contains(
            presentation.SummaryRows,
            row => row.Title == "Text Insertion"
                && row.Description == "Uses clipboard paste into the focused field; recover from History if the target app rejects insertion."
                && row.StatusBadge == "Review");
        Assert.Contains(
            presentation.SetupActions,
            action => action.Title == "Text Insertion"
                && action.Description == "Click a target field before recording. VoiceInk pastes the transcript there and keeps a History copy for recovery."
                && action.CommandText == "Review Flow"
                && action.StatusBadge == "Review");
    }

    [Fact]
    public void Present_RowsExposeDisplayTextAndAccessibleNames()
    {
        var status = OnboardingSetupStatusService.Build(
            new AppSettings
            {
                ModelPath = "C:\\Models\\ggml-base.en.bin",
                Hotkey = "Ctrl+Shift+V"
            },
            hasAudioInputChoices: true);

        var presentation = OnboardingChecklistPresenter.Present(status);

        Assert.Equal(
            "Ready: Choose Model - Browse or Download. Local Whisper model is selected for private offline transcription.",
            presentation.SetupActions[0].DisplayText);
        Assert.Equal(
            "Choose Model, Ready, Browse or Download, Local Whisper model is selected for private offline transcription.",
            presentation.SetupActions[0].AccessibleName);
        Assert.Equal(
            "Ready: Model - Local Whisper model selected.",
            presentation.SummaryRows[0].DisplayText);
        Assert.Equal(
            "Model, Ready, Local Whisper model selected.",
            presentation.SummaryRows[0].AccessibleName);
        Assert.Equal(
            "[Ready] 1. Choose Model - Local transcription model selected.",
            presentation.Stages[0].DisplayText);
        Assert.Equal(
            "1, Choose Model, Ready, Local transcription model selected.",
            presentation.Stages[0].AccessibleName);
        Assert.Equal(
            "Ready: 2. Press Ctrl+Shift+V - Start recording with your primary shortcut.",
            presentation.TutorialSteps[1].DisplayText);
        Assert.Equal(
            "2, Press Ctrl+Shift+V, Ready, Start recording with your primary shortcut.",
            presentation.TutorialSteps[1].AccessibleName);
    }
}
