using VoiceInk.Windows.Core.Enhancement;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Enhancement;

public sealed class EnhancementProviderPresetCatalogTests
{
    [Fact]
    public void All_IncludesOpenAICompatibleEnhancementPresets()
    {
        var presets = EnhancementProviderPresetCatalog.All;

        Assert.Contains(presets, item =>
            item.Id == "custom"
            && item.DisplayName == "Custom OpenAI-compatible"
            && item.Endpoint.Length == 0
            && item.DefaultModel.Length == 0
            && item.RequiresApiKey);
        Assert.Contains(presets, item =>
            item.Id == "cerebras"
            && item.Endpoint == "https://api.cerebras.ai/v1/chat/completions"
            && item.DefaultModel == "gpt-oss-120b"
            && item.ModelIds.Contains("zai-glm-4.7"));
        Assert.Contains(presets, item =>
            item.Id == "groq"
            && item.Endpoint == "https://api.groq.com/openai/v1/chat/completions"
            && item.DefaultModel == "openai/gpt-oss-120b"
            && item.ModelIds.Contains("llama-3.3-70b-versatile"));
        Assert.Contains(presets, item =>
            item.Id == "gemini"
            && item.Endpoint == "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions"
            && item.DefaultModel == "gemini-2.5-flash-lite"
            && item.ModelIds.Contains("gemini-2.5-pro"));
        Assert.Contains(presets, item =>
            item.Id == "anthropic"
            && item.DisplayName == "Anthropic"
            && item.Endpoint == "https://api.anthropic.com/v1/messages"
            && item.DefaultModel == "claude-sonnet-4-6"
            && item.ModelIds.Contains("claude-haiku-4-5")
            && item.RequiresApiKey);
        Assert.Contains(presets, item =>
            item.Id == "openai"
            && item.Endpoint == "https://api.openai.com/v1/chat/completions"
            && item.DefaultModel == "gpt-5.4"
            && item.ModelIds.Contains("gpt-4.1-mini"));
        Assert.Contains(presets, item =>
            item.Id == "openrouter"
            && item.Endpoint == "https://openrouter.ai/api/v1/chat/completions"
            && item.DefaultModel == "openai/gpt-oss-120b");
        Assert.Contains(presets, item =>
            item.Id == "mistral"
            && item.Endpoint == "https://api.mistral.ai/v1/chat/completions"
            && item.DefaultModel == "mistral-large-latest"
            && item.ModelIds.Contains("mistral-small-latest"));
        Assert.Contains(presets, item =>
            item.Id == "ollama"
            && item.Endpoint == "http://localhost:11434/api/chat"
            && item.DefaultModel == "mistral"
            && !item.RequiresApiKey);
        Assert.Contains(presets, item =>
            item.Id == "local-cli"
            && item.DisplayName == "Local CLI"
            && item.Endpoint.Length == 0
            && item.DefaultModel == "local-cli"
            && !item.RequiresApiKey);
    }

    [Theory]
    [InlineData("", "custom")]
    [InlineData("missing", "custom")]
    [InlineData("GROQ", "groq")]
    [InlineData("ollama", "ollama")]
    [InlineData("local-cli", "local-cli")]
    public void Resolve_ReturnsRequestedPresetOrCustomFallback(string id, string expectedId)
    {
        Assert.Equal(expectedId, EnhancementProviderPresetCatalog.Resolve(id).Id);
    }

    [Theory]
    [InlineData("custom", "VoiceInk.Windows.Enhancement.OpenAICompatible.Custom.ApiKey")]
    [InlineData("groq", "VoiceInk.Windows.Enhancement.OpenAICompatible.Groq.ApiKey")]
    [InlineData("gemini", "VoiceInk.Windows.Enhancement.OpenAICompatible.Gemini.ApiKey")]
    [InlineData("anthropic", "VoiceInk.Windows.Enhancement.OpenAICompatible.Anthropic.ApiKey")]
    [InlineData("openai", "VoiceInk.Windows.Enhancement.OpenAICompatible.OpenAI.ApiKey")]
    public void SecretNameForProvider_ReturnsProviderSpecificCredentialName(
        string providerId,
        string expected)
    {
        Assert.Equal(expected, EnhancementConfiguration.SecretNameForProvider(providerId));
    }

    [Fact]
    public void SecretNamesForProvider_IncludesLegacyFallbackOnlyForCustomProvider()
    {
        Assert.Equal(
            [
                "VoiceInk.Windows.Enhancement.OpenAICompatible.Custom.ApiKey",
                "VoiceInk.Windows.Enhancement.OpenAICompatible.ApiKey"
            ],
            EnhancementConfiguration.SecretNamesForProvider("custom"));
        Assert.Equal(
            ["VoiceInk.Windows.Enhancement.OpenAICompatible.Groq.ApiKey"],
            EnhancementConfiguration.SecretNamesForProvider("groq"));
        Assert.Empty(EnhancementConfiguration.SecretNamesForProvider("ollama"));
        Assert.Empty(EnhancementConfiguration.SecretNamesForProvider("local-cli"));
    }

    [Theory]
    [InlineData("custom", "openai-compatible")]
    [InlineData("groq", "groq")]
    [InlineData("gemini", "gemini")]
    [InlineData("anthropic", "anthropic")]
    [InlineData("ollama", "ollama")]
    [InlineData("local-cli", "local-cli")]
    [InlineData("missing", "openai-compatible")]
    public void ProviderNameFor_ReturnsStableHistoryMetadata(string providerId, string expected)
    {
        Assert.Equal(expected, EnhancementConfiguration.ProviderNameFor(providerId));
    }

    [Theory]
    [InlineData("", "AI enhancement endpoint is invalid.")]
    [InlineData("http://api.example.test/v1/chat/completions", "AI enhancement endpoint must use HTTPS unless it targets localhost.")]
    [InlineData("https://sk-test-secret@api.example.test/v1/chat/completions", "AI enhancement endpoint must not contain credentials.")]
    [InlineData("https://api.example.test/v1/chat/completions?api_key=sk-test-secret", "AI enhancement endpoint must not contain API keys or tokens in the query string.")]
    public void ValidateEndpoint_RejectsInvalidOrSecretBearingEndpoints(
        string endpoint,
        string expectedMessage)
    {
        Assert.Equal(expectedMessage, EnhancementConfiguration.ValidateEndpoint(endpoint));
    }

    [Theory]
    [InlineData("https://api.example.test/v1/chat/completions")]
    [InlineData("http://localhost:11434/v1/chat/completions")]
    [InlineData("http://localhost:11434/api/chat")]
    [InlineData("http://127.0.0.1:11434/v1/chat/completions")]
    public void ValidateEndpoint_AllowsHttpsAndLoopbackHttpEndpoints(string endpoint)
    {
        Assert.Null(EnhancementConfiguration.ValidateEndpoint(endpoint));
    }

    [Theory]
    [InlineData(false, "", "", null)]
    [InlineData(false, "https://api.example.test/v1/chat/completions?api_key=sk-test-secret", "", "AI enhancement endpoint must not contain API keys or tokens in the query string.")]
    [InlineData(true, "", "test-model", "AI enhancement endpoint and model are required.")]
    [InlineData(true, "https://api.example.test/v1/chat/completions", "", "AI enhancement endpoint and model are required.")]
    [InlineData(true, "https://api.example.test/v1/chat/completions?token=sk-test-secret", "test-model", "AI enhancement endpoint must not contain API keys or tokens in the query string.")]
    [InlineData(true, "https://api.example.test/v1/chat/completions", "test-model", null)]
    public void ValidateRequiredSettings_ChecksEnabledEnhancementConfigurationBeforePersisting(
        bool isEnabled,
        string endpoint,
        string model,
        string? expectedMessage)
    {
        Assert.Equal(
            expectedMessage,
            EnhancementConfiguration.ValidateRequiredSettings(isEnabled, endpoint, model));
    }

    [Theory]
    [InlineData(false, "", "", null)]
    [InlineData(true, "", "local-cli", "AI enhancement endpoint and model are required.")]
    [InlineData(true, "claude -p \"%VOICEINK_FULL_PROMPT%\"", "", null)]
    [InlineData(true, "claude -p \"%VOICEINK_FULL_PROMPT%\"", "local-cli", null)]
    public void ValidateRequiredSettings_AllowsLocalCliCommandTemplates(
        bool isEnabled,
        string endpoint,
        string model,
        string? expectedMessage)
    {
        Assert.Equal(
            expectedMessage,
            EnhancementConfiguration.ValidateRequiredSettings(isEnabled, endpoint, model, "local-cli"));
    }
}
