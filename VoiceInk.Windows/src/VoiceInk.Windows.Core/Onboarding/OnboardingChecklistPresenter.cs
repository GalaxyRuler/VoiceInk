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

    private static string PrefixFor(OnboardingChecklistItemState state) =>
        state switch
        {
            OnboardingChecklistItemState.Ready => "[Ready]",
            OnboardingChecklistItemState.NeedsAttention => "[Needs attention]",
            _ => "[Review]"
        };
}

public sealed record OnboardingSummaryRowPresentation(
    string Title,
    string Description,
    string StatusBadge);

public sealed record OnboardingSetupActionPresentation(
    string Title,
    string Description,
    string CommandText,
    string StatusBadge);

public sealed record OnboardingChecklistPresentation(
    string Title,
    string Description,
    string ProgressLabel,
    string NextAction,
    bool CanSaveSetup,
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
                "If recording does not start, allow microphone access for desktop apps in Windows Settings.",
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

        return new OnboardingChecklistPresentation(
            "Welcome to VoiceInk",
            "Set up your local model, microphone, and shortcut once, then dictate anywhere from the tray or keyboard.",
            $"{readyCount} of {items.Length} setup essentials ready",
            NextActionFor(status),
            status.CanCompleteSetup,
            setupActions,
            summaryRows,
            stages,
            items);
    }

    private static OnboardingChecklistItemState StateFor(bool isReady) =>
        isReady ? OnboardingChecklistItemState.Ready : OnboardingChecklistItemState.NeedsAttention;

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
