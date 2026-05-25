namespace VoiceInk.Windows.Core.Onboarding;

public sealed record OnboardingSetupStatus(
    bool HasModelPath,
    bool HasPrimaryShortcut,
    bool HasAudioInputChoices,
    bool CanCompleteSetup)
{
    public string MicrophoneStatusTitle =>
        HasAudioInputChoices ? "Microphone detected" : "No microphone detected";

    public string MicrophoneStatusMessage =>
        HasAudioInputChoices
            ? "VoiceInk found at least one recording input."
            : "Connect or enable a microphone, refresh the device list, or check Windows microphone privacy settings.";

    public string MicrophoneActionText =>
        HasAudioInputChoices ? "Open Windows Microphone Settings" : "Check Windows Microphone Settings";
}
