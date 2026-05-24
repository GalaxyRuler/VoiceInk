using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Onboarding;

public static class OnboardingSetupStatusService
{
    public static OnboardingSetupStatus Build(
        AppSettings settings,
        bool hasAudioInputChoices)
    {
        var hasModelPath = !string.IsNullOrWhiteSpace(settings.ModelPath);
        var hasPrimaryShortcut = !string.IsNullOrWhiteSpace(settings.Hotkey);

        return new OnboardingSetupStatus(
            hasModelPath,
            hasPrimaryShortcut,
            hasAudioInputChoices,
            CanCompleteSetup: hasModelPath && hasPrimaryShortcut);
    }
}
