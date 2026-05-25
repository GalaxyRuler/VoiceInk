using VoiceInk.Windows.Core.Models;

namespace VoiceInk.Windows.Core.Onboarding;

public static class OnboardingModelCatalogService
{
    public const string DefaultRecommendedModelName = "ggml-base.en";

    public static IReadOnlyList<WhisperModelCatalogEntry> BuildRecommendedChoices() =>
        WhisperModelCatalog.Recommended;
}
