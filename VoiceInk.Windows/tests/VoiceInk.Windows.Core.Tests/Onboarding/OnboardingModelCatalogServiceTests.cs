using VoiceInk.Windows.Core.Onboarding;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Onboarding;

public sealed class OnboardingModelCatalogServiceTests
{
    [Fact]
    public void BuildRecommendedChoices_ReturnsSmallAndFastDefaults()
    {
        var choices = OnboardingModelCatalogService.BuildRecommendedChoices();

        Assert.Contains(choices, choice => choice.Name == "ggml-base.en");
        Assert.Contains(choices, choice => choice.Name == "ggml-large-v3-turbo-q5_0");
    }

    [Fact]
    public void DefaultRecommendedModelName_UsesEnglishBaseModel()
    {
        Assert.Equal("ggml-base.en", OnboardingModelCatalogService.DefaultRecommendedModelName);
    }
}
