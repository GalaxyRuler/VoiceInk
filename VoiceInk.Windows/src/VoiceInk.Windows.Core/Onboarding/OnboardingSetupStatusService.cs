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
            string.IsNullOrWhiteSpace(settings.Hotkey) ? "your shortcut" : settings.Hotkey.Trim(),
            CanCompleteSetup: hasModelPath && hasPrimaryShortcut,
            HealthChecks:
            [
                $"Model path: {ReadyText(hasModelPath)}",
                $"Primary shortcut: {ReadyText(hasPrimaryShortcut)}",
                $"Microphone device: {ReadyText(hasAudioInputChoices)}",
                "Windows microphone privacy: review if recording fails",
                "App microphone capability: declared",
                "Text insertion: focused field and clipboard paste path explained",
                "Context awareness: optional and off by default during setup"
            ]);
    }

    private static string ReadyText(bool isReady) =>
        isReady ? "ready" : "needs attention";
}
