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

public sealed record OnboardingChecklistPresentation(
    string Title,
    string Description,
    string ProgressLabel,
    string NextAction,
    bool CanSaveSetup,
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

        return new OnboardingChecklistPresentation(
            "Welcome to VoiceInk",
            "Set up your local model, microphone, and shortcut once, then dictate anywhere from the tray or keyboard.",
            $"{readyCount} of {items.Length} setup essentials ready",
            NextActionFor(status),
            status.CanCompleteSetup,
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
