namespace VoiceInk.Windows.Core.Onboarding;

public enum OnboardingChecklistItemState
{
    Ready,
    NeedsAttention,
    Advisory
}

public sealed record OnboardingChecklistItemPresentation(
    string Title,
    string Description,
    OnboardingChecklistItemState State);

public sealed record OnboardingSetupStagePresentation(
    string StepNumber,
    string Title,
    string Description,
    OnboardingChecklistItemState State)
{
    public string DisplayText => $"{PrefixFor(State)} {StepNumber}. {Title} - {Description}";

    public string StatusBadge => StateLabelFor(State);

    public string AccessibleName => OnboardingRowAccessibleName.From(
        StepNumber,
        Title,
        StatusBadge,
        Description);

    private static string PrefixFor(OnboardingChecklistItemState state) =>
        state switch
        {
            OnboardingChecklistItemState.Ready => "[Ready]",
            OnboardingChecklistItemState.NeedsAttention => "[Needs attention]",
            _ => "[Review]"
        };

    private static string StateLabelFor(OnboardingChecklistItemState state) =>
        state switch
        {
            OnboardingChecklistItemState.Ready => "Ready",
            OnboardingChecklistItemState.NeedsAttention => "Needs attention",
            _ => "Review"
        };
}

public sealed record OnboardingSummaryRowPresentation(
    string Title,
    string Description,
    string StatusBadge)
{
    public string DisplayText => $"{StatusBadge}: {Title} - {Description}";

    public string AccessibleName => OnboardingRowAccessibleName.From(Title, StatusBadge, Description);
}

public sealed record OnboardingSetupActionPresentation(
    string Title,
    string Description,
    string CommandText,
    string StatusBadge)
{
    public string DisplayText => $"{StatusBadge}: {Title} - {CommandText}. {Description}";

    public string AccessibleName => OnboardingRowAccessibleName.From(Title, StatusBadge, CommandText, Description);
}

public sealed record OnboardingTutorialStepPresentation(
    string StepNumber,
    string Title,
    string Description,
    string StatusBadge)
{
    public string DisplayText => $"{StatusBadge}: {StepNumber}. {Title} - {Description}";

    public string AccessibleName => OnboardingRowAccessibleName.From(StepNumber, Title, StatusBadge, Description);
}

internal static class OnboardingRowAccessibleName
{
    public static string From(params string[] parts) =>
        string.Join(", ", parts.Where(part => !string.IsNullOrWhiteSpace(part)).Select(part => part.Trim()));
}

public sealed record OnboardingChecklistPresentation(
    string Title,
    IReadOnlyList<string> HeroTaglines,
    string Description,
    string ProgressLabel,
    string NextAction,
    string CurrentStageLabel,
    string CurrentStageTitle,
    string CurrentStageDescription,
    string CurrentStageStatusBadge,
    string CurrentStageAccessibleName,
    bool CanSaveSetup,
    IReadOnlyList<OnboardingTutorialStepPresentation> TutorialSteps,
    IReadOnlyList<OnboardingSetupActionPresentation> SetupActions,
    IReadOnlyList<OnboardingSummaryRowPresentation> SummaryRows,
    IReadOnlyList<OnboardingSetupStagePresentation> Stages,
    IReadOnlyList<OnboardingChecklistItemPresentation> Items)
{
    public string ChecklistSummary => string.Join(
        Environment.NewLine,
        Items.Select(item => $"{PrefixFor(item.State)} {item.Title}: {item.Description}"));

    private static string PrefixFor(OnboardingChecklistItemState state) =>
        state switch
        {
            OnboardingChecklistItemState.Ready => "[Ready]",
            OnboardingChecklistItemState.NeedsAttention => "[Needs attention]",
            _ => "[Review]"
        };
}

public static class OnboardingChecklistPresenter
{
    public static OnboardingChecklistPresentation Present(OnboardingSetupStatus status)
    {
        var items = new[]
        {
            new OnboardingChecklistItemPresentation(
                "Local Whisper model",
                status.HasModelPath
                    ? "A local GGML model path is selected for private offline transcription."
                    : "Choose an existing .bin model or download the recommended starter model.",
                StateFor(status.HasModelPath)),
            new OnboardingChecklistItemPresentation(
                "Primary recording shortcut",
                status.HasPrimaryShortcut
                    ? "The global shortcut is configured for starting and stopping dictation."
                    : "Set the shortcut you will use from any focused text field.",
                StateFor(status.HasPrimaryShortcut)),
            new OnboardingChecklistItemPresentation(
                "Microphone input",
                status.HasAudioInputChoices
                    ? "VoiceInk can see at least one recording input."
                    : "No input is visible yet. Refresh devices or open Windows microphone privacy settings before your first recording.",
                StateFor(status.HasAudioInputChoices)),
            new OnboardingChecklistItemPresentation(
                "Windows microphone privacy",
                "VoiceInk may not appear as a separate app entry while source-built; keep Microphone access and 'Let desktop apps access your microphone' enabled.",
                OnboardingChecklistItemState.Advisory),
            new OnboardingChecklistItemPresentation(
                "Text insertion",
                "VoiceInk inserts through the focused field by using clipboard paste; History keeps the transcript available if the target app blocks paste or loses focus.",
                OnboardingChecklistItemState.Advisory),
            new OnboardingChecklistItemPresentation(
                "Context awareness",
                "Optional local context stays default-off during setup; Windows will ask before screen OCR captures a window or display.",
                OnboardingChecklistItemState.Advisory),
            new OnboardingChecklistItemPresentation(
                "First dictation test",
                "After saving, click a text field, press your shortcut, speak, then press it again.",
                status.CanCompleteSetup && status.HasAudioInputChoices
                    ? OnboardingChecklistItemState.Ready
                    : OnboardingChecklistItemState.Advisory)
        };

        var readyCount = items.Count(item => item.State == OnboardingChecklistItemState.Ready);
        var summaryRows = new[]
        {
            new OnboardingSummaryRowPresentation(
                "Model",
                status.HasModelPath
                    ? "Local Whisper model selected."
                    : "Choose a local Whisper model.",
                status.HasModelPath ? "Ready" : "Required"),
            new OnboardingSummaryRowPresentation(
                "Microphone",
                status.HasAudioInputChoices
                    ? "Recording input detected."
                    : "Refresh devices or check Windows privacy settings.",
                status.HasAudioInputChoices ? "Ready" : "Check"),
            new OnboardingSummaryRowPresentation(
                "Windows Permission",
                status.HasAudioInputChoices
                    ? "Keep Microphone access and 'Let desktop apps access your microphone' enabled; source-built VoiceInk may not be listed by name."
                    : "Open Microphone privacy settings; VoiceInk may rely on 'Let desktop apps access your microphone' even when it is not listed by name.",
                status.HasAudioInputChoices ? "Review" : "Check"),
            new OnboardingSummaryRowPresentation(
                "Text Insertion",
                "Uses clipboard paste into the focused field; recover from History if the target app rejects insertion.",
                "Review"),
            new OnboardingSummaryRowPresentation(
                "Context Awareness",
                "Windows asks for capture consent when screen OCR context is enabled and used.",
                "Optional"),
            new OnboardingSummaryRowPresentation(
                "Shortcut",
                status.HasPrimaryShortcut
                    ? "Ctrl+Alt+Space is ready."
                    : "Set the shortcut you will use from any app.",
                status.HasPrimaryShortcut ? "Ready" : "Required"),
            new OnboardingSummaryRowPresentation(
                "First Dictation",
                status.CanCompleteSetup && status.HasAudioInputChoices
                    ? "Click a text field, press your shortcut, speak, then press it again."
                    : "Save setup after required items are ready, then try insertion in any text field.",
                "Try next")
        };
        var setupActions = new[]
        {
            new OnboardingSetupActionPresentation(
                "Choose Model",
                status.HasModelPath
                    ? "Local Whisper model is selected for private offline transcription."
                    : "Select an existing GGML .bin model or download the recommended starter model.",
                "Browse or Download",
                status.HasModelPath ? "Ready" : "Required"),
            new OnboardingSetupActionPresentation(
                "Check Microphone",
                status.HasAudioInputChoices
                    ? "Recording input is visible."
                    : "Refresh devices or open Windows microphone privacy settings.",
                status.HasAudioInputChoices ? "Refresh Devices" : "Open Privacy",
                status.HasAudioInputChoices ? "Ready" : "Check"),
            new OnboardingSetupActionPresentation(
                "Manual Privacy Path",
                "Use Settings > Privacy & security > Microphone, then check both Microphone access and 'Let desktop apps access your microphone' if VoiceInk is not listed; Windows may show recent desktop app microphone activity there.",
                "Open Manually",
                "Fallback"),
            new OnboardingSetupActionPresentation(
                "Text Insertion",
                "Click a target field before recording. VoiceInk pastes the transcript there and keeps a History copy for recovery.",
                "Review Flow",
                "Review"),
            new OnboardingSetupActionPresentation(
                "Context Awareness",
                "Optional local screen OCR and clipboard context are default-off. Enable them later when you want extra context for enhancement.",
                "Review Later",
                "Optional"),
            new OnboardingSetupActionPresentation(
                "Set Shortcut",
                status.HasPrimaryShortcut
                    ? "Primary shortcut is configured for system-wide recording."
                    : "Enter the shortcut you will press from any app.",
                "Edit Shortcut",
                status.HasPrimaryShortcut ? "Ready" : "Required"),
            new OnboardingSetupActionPresentation(
                "Try Dictation",
                status.CanCompleteSetup && status.HasAudioInputChoices
                    ? "Click a text field, press your shortcut, speak, then press it again."
                    : "Complete required setup, then test insertion in any text field.",
                status.CanCompleteSetup && status.HasAudioInputChoices
                    ? "Click Field and Speak"
                    : "Test after Save",
                status.CanCompleteSetup && status.HasAudioInputChoices ? "Ready" : "Next")
        };
        var stages = new[]
        {
            new OnboardingSetupStagePresentation(
                "1",
                "Choose Model",
                status.HasModelPath
                    ? "Local transcription model selected."
                    : "Pick or download a local Whisper model.",
                StateFor(status.HasModelPath)),
            new OnboardingSetupStagePresentation(
                "2",
                "Check Microphone",
                status.HasAudioInputChoices
                    ? "At least one recording input is visible."
                    : "Refresh devices or open Windows microphone settings.",
                StateFor(status.HasAudioInputChoices)),
            new OnboardingSetupStagePresentation(
                "3",
                "Set Shortcut",
                status.HasPrimaryShortcut
                    ? "Global recording shortcut configured."
                    : "Choose the shortcut you will press from any app.",
                StateFor(status.HasPrimaryShortcut)),
            new OnboardingSetupStagePresentation(
                "4",
                "Try Dictation",
                status.CanCompleteSetup && status.HasAudioInputChoices
                    ? "Ready for a first focused-text-field smoke test."
                    : "Save setup when required items are ready, then test insertion.",
                status.CanCompleteSetup && status.HasAudioInputChoices
                    ? OnboardingChecklistItemState.Ready
                    : OnboardingChecklistItemState.Advisory)
        };
        var currentStage = CurrentStageFrom(stages);
        var currentStageIndex = Array.IndexOf(stages, currentStage) + 1;
        var currentStageLabel = $"Step {currentStageIndex} of {stages.Length}";

        return new OnboardingChecklistPresentation(
            "Welcome to VoiceInk",
            [
                "Welcome to the Future of Typing",
                "A New Way to Type",
                "Your Writing Assistant",
                "Works everywhere on Windows with your shortcut",
                "100% offline and private with local models"
            ],
            "Set up your local model, microphone, and shortcut once, then dictate anywhere from the tray or keyboard.",
            $"{readyCount} of {items.Length} setup essentials ready",
            NextActionFor(status),
            currentStageLabel,
            currentStage.Title,
            currentStage.Description,
            currentStage.StatusBadge,
            OnboardingRowAccessibleName.From(currentStageLabel, currentStage.Title, currentStage.StatusBadge, currentStage.Description),
            status.CanCompleteSetup,
            TutorialSteps(status),
            setupActions,
            summaryRows,
            stages,
            items);
    }

    private static OnboardingChecklistItemState StateFor(bool isReady) =>
        isReady ? OnboardingChecklistItemState.Ready : OnboardingChecklistItemState.NeedsAttention;

    private static OnboardingSetupStagePresentation CurrentStageFrom(IReadOnlyList<OnboardingSetupStagePresentation> stages) =>
        stages.FirstOrDefault(stage => stage.State == OnboardingChecklistItemState.NeedsAttention)
            ?? stages.FirstOrDefault(stage => stage.State == OnboardingChecklistItemState.Advisory)
            ?? stages[^1];

    private static IReadOnlyList<OnboardingTutorialStepPresentation> TutorialSteps(OnboardingSetupStatus status)
    {
        var stepStatus = status.CanCompleteSetup && status.HasAudioInputChoices ? "Ready" : "Waiting";
        var shortcut = string.IsNullOrWhiteSpace(status.PrimaryShortcut) ? "your shortcut" : status.PrimaryShortcut.Trim();

        return
        [
            new OnboardingTutorialStepPresentation(
                "1",
                "Click a text field",
                "Place the cursor where VoiceInk should insert your first dictation.",
                stepStatus),
            new OnboardingTutorialStepPresentation(
                "2",
                $"Press {shortcut}",
                "Start recording with your primary shortcut.",
                stepStatus),
            new OnboardingTutorialStepPresentation(
                "3",
                "Speak a short phrase",
                "Say a sentence you can easily recognize in the target field.",
                stepStatus),
            new OnboardingTutorialStepPresentation(
                "4",
                $"Press {shortcut} again",
                "Stop recording and let VoiceInk insert the transcription.",
                stepStatus),
            new OnboardingTutorialStepPresentation(
                "5",
                "Check insertion and History",
                "Confirm text appears in the target field, then use History to review, paste, retry, or recover the dictation.",
                stepStatus)
        ];
    }

    private static string NextActionFor(OnboardingSetupStatus status)
    {
        if (!status.HasModelPath)
        {
            return "Choose or download a local Whisper model to continue.";
        }

        if (!status.HasPrimaryShortcut)
        {
            return "Set your primary recording shortcut to continue.";
        }

        if (!status.HasAudioInputChoices)
        {
            return "Save setup after checking your microphone, then run the first dictation test.";
        }

        return "Save setup, click a text field, press your shortcut, speak, then press it again to insert text.";
    }
}
