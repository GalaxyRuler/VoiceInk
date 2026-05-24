using VoiceInk.Windows.Core.Transcription;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Transcription;

public sealed class TranscriptionProviderPresetCatalogTests
{
    [Fact]
    public void All_IncludesCustomAndGroqPresets()
    {
        var presets = TranscriptionProviderPresetCatalog.All;

        Assert.Contains(presets, item =>
            item.Id == "custom" && item.DisplayName == "Custom OpenAI-compatible");
        var groq = Assert.Single(presets, item => item.Id == "groq");
        Assert.Equal("Groq", groq.DisplayName);
        Assert.Equal("https://api.groq.com/openai/v1/audio/transcriptions", groq.Endpoint);
        Assert.Equal("whisper-large-v3-turbo", groq.DefaultModel);
        Assert.Contains("whisper-large-v3", groq.ModelIds);
    }

    [Theory]
    [InlineData("", "custom")]
    [InlineData("missing", "custom")]
    [InlineData("groq", "groq")]
    public void Resolve_ReturnsRequestedPresetOrCustomFallback(string id, string expectedId)
    {
        Assert.Equal(expectedId, TranscriptionProviderPresetCatalog.Resolve(id).Id);
    }

    [Theory]
    [InlineData("custom", "VoiceInk.Windows.Transcription.OpenAICompatible.Custom.ApiKey")]
    [InlineData("groq", "VoiceInk.Windows.Transcription.OpenAICompatible.Groq.ApiKey")]
    public void SecretNameFor_ReturnsProviderSpecificCredentialName(string providerId, string expected)
    {
        Assert.Equal(expected, TranscriptionConfiguration.SecretNameForCloudProvider(providerId));
    }

    [Fact]
    public void SecretNamesForCloudProvider_IncludesLegacyFallbackOnlyForCustomProvider()
    {
        Assert.Equal(
            [
                "VoiceInk.Windows.Transcription.OpenAICompatible.Custom.ApiKey",
                "VoiceInk.Windows.Transcription.OpenAICompatible.ApiKey"
            ],
            TranscriptionConfiguration.SecretNamesForCloudProvider("custom"));
        Assert.Equal(
            ["VoiceInk.Windows.Transcription.OpenAICompatible.Groq.ApiKey"],
            TranscriptionConfiguration.SecretNamesForCloudProvider("groq"));
    }
}
