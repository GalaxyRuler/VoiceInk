namespace VoiceInk.Windows.Core.Onboarding;

public sealed record OnboardingSetupStatus(
    bool HasModelPath,
    bool HasPrimaryShortcut,
    bool HasAudioInputChoices,
    bool CanCompleteSetup);
