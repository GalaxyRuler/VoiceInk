using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Core.Settings;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Transcription;

public sealed class TranscriptionProviderPresetCatalogTests
{
    [Fact]
    public void All_IncludesMacCloudProviderPresets()
    {
        var presets = TranscriptionProviderPresetCatalog.All;

        Assert.Contains(presets, item =>
            item.Id == "custom" && item.DisplayName == "Custom OpenAI-compatible");
        var groq = Assert.Single(presets, item => item.Id == "groq");
        Assert.Equal("Groq", groq.DisplayName);
        Assert.Equal("https://api.groq.com/openai/v1/audio/transcriptions", groq.Endpoint);
        Assert.Equal("whisper-large-v3-turbo", groq.DefaultModel);
        Assert.Contains("whisper-large-v3", groq.ModelIds);
        var deepgram = Assert.Single(presets, item => item.Id == "deepgram");
        Assert.Equal("Deepgram", deepgram.DisplayName);
        Assert.Equal("https://api.deepgram.com/v1/listen", deepgram.Endpoint);
        Assert.Equal("nova-3", deepgram.DefaultModel);
        Assert.Contains("nova-3-medical", deepgram.ModelIds);
        var assemblyAI = Assert.Single(presets, item => item.Id == "assemblyai");
        Assert.Equal("AssemblyAI", assemblyAI.DisplayName);
        Assert.Equal("https://streaming.assemblyai.com/v3/ws", assemblyAI.Endpoint);
        Assert.Equal("universal-3-pro", assemblyAI.DefaultModel);
        Assert.Contains("universal-streaming", assemblyAI.ModelIds);
        var mistral = Assert.Single(presets, item => item.Id == "mistral");
        Assert.Equal("Mistral", mistral.DisplayName);
        Assert.Equal("https://api.mistral.ai/v1/audio/transcriptions", mistral.Endpoint);
        Assert.Equal("voxtral-mini-latest", mistral.DefaultModel);
        Assert.Contains("voxtral-mini-latest", mistral.ModelIds);
        var elevenLabs = Assert.Single(presets, item => item.Id == "elevenlabs");
        Assert.Equal("ElevenLabs", elevenLabs.DisplayName);
        Assert.Equal("https://api.elevenlabs.io/v1/speech-to-text", elevenLabs.Endpoint);
        Assert.Equal("scribe_v2", elevenLabs.DefaultModel);
        Assert.Contains("scribe_v1", elevenLabs.ModelIds);
        var soniox = Assert.Single(presets, item => item.Id == "soniox");
        Assert.Equal("Soniox", soniox.DisplayName);
        Assert.Equal("https://api.soniox.com/v1/transcriptions", soniox.Endpoint);
        Assert.Equal("stt-async-v4", soniox.DefaultModel);
        Assert.Contains("stt-async-v4", soniox.ModelIds);
        var speechmatics = Assert.Single(presets, item => item.Id == "speechmatics");
        Assert.Equal("Speechmatics", speechmatics.DisplayName);
        Assert.Equal("https://eu1.asr.api.speechmatics.com/v2/jobs", speechmatics.Endpoint);
        Assert.Equal("speechmatics-enhanced", speechmatics.DefaultModel);
        Assert.Contains("speechmatics-enhanced", speechmatics.ModelIds);
        var gemini = Assert.Single(presets, item => item.Id == "gemini");
        Assert.Equal("Gemini", gemini.DisplayName);
        Assert.Equal("https://generativelanguage.googleapis.com/v1beta/models", gemini.Endpoint);
        Assert.Equal("gemini-2.5-flash", gemini.DefaultModel);
        Assert.Contains("gemini-2.5-pro", gemini.ModelIds);
        Assert.Contains("gemini-3-flash-preview", gemini.ModelIds);
        var xai = Assert.Single(presets, item => item.Id == "xai");
        Assert.Equal("xAI", xai.DisplayName);
        Assert.Equal("https://api.x.ai/v1/stt", xai.Endpoint);
        Assert.Equal("grok-stt", xai.DefaultModel);
        Assert.Contains("grok-stt", xai.ModelIds);
        var cartesia = Assert.Single(presets, item => item.Id == "cartesia");
        Assert.Equal("Cartesia", cartesia.DisplayName);
        Assert.Equal("https://api.cartesia.ai/stt", cartesia.Endpoint);
        Assert.Equal("ink-whisper", cartesia.DefaultModel);
        Assert.Contains("ink-whisper", cartesia.ModelIds);
    }

    [Theory]
    [InlineData("", "custom")]
    [InlineData("missing", "custom")]
    [InlineData("groq", "groq")]
    [InlineData("deepgram", "deepgram")]
    [InlineData("assemblyai", "assemblyai")]
    [InlineData("mistral", "mistral")]
    [InlineData("elevenlabs", "elevenlabs")]
    [InlineData("soniox", "soniox")]
    [InlineData("speechmatics", "speechmatics")]
    [InlineData("gemini", "gemini")]
    [InlineData("xai", "xai")]
    [InlineData("cartesia", "cartesia")]
    public void Resolve_ReturnsRequestedPresetOrCustomFallback(string id, string expectedId)
    {
        Assert.Equal(expectedId, TranscriptionProviderPresetCatalog.Resolve(id).Id);
    }

    [Theory]
    [InlineData("custom", "VoiceInk.Windows.Transcription.OpenAICompatible.Custom.ApiKey")]
    [InlineData("groq", "VoiceInk.Windows.Transcription.OpenAICompatible.Groq.ApiKey")]
    [InlineData("deepgram", "VoiceInk.Windows.Transcription.OpenAICompatible.Deepgram.ApiKey")]
    [InlineData("assemblyai", "VoiceInk.Windows.Transcription.OpenAICompatible.AssemblyAI.ApiKey")]
    [InlineData("mistral", "VoiceInk.Windows.Transcription.OpenAICompatible.Mistral.ApiKey")]
    [InlineData("elevenlabs", "VoiceInk.Windows.Transcription.OpenAICompatible.ElevenLabs.ApiKey")]
    [InlineData("soniox", "VoiceInk.Windows.Transcription.OpenAICompatible.Soniox.ApiKey")]
    [InlineData("speechmatics", "VoiceInk.Windows.Transcription.OpenAICompatible.Speechmatics.ApiKey")]
    [InlineData("gemini", "VoiceInk.Windows.Transcription.OpenAICompatible.Gemini.ApiKey")]
    [InlineData("xai", "VoiceInk.Windows.Transcription.OpenAICompatible.xAI.ApiKey")]
    [InlineData("cartesia", "VoiceInk.Windows.Transcription.OpenAICompatible.Cartesia.ApiKey")]
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
        Assert.Equal(
            ["VoiceInk.Windows.Transcription.OpenAICompatible.Deepgram.ApiKey"],
            TranscriptionConfiguration.SecretNamesForCloudProvider("deepgram"));
        Assert.Equal(
            ["VoiceInk.Windows.Transcription.OpenAICompatible.AssemblyAI.ApiKey"],
            TranscriptionConfiguration.SecretNamesForCloudProvider("assemblyai"));
        Assert.Equal(
            ["VoiceInk.Windows.Transcription.OpenAICompatible.Mistral.ApiKey"],
            TranscriptionConfiguration.SecretNamesForCloudProvider("mistral"));
        Assert.Equal(
            ["VoiceInk.Windows.Transcription.OpenAICompatible.ElevenLabs.ApiKey"],
            TranscriptionConfiguration.SecretNamesForCloudProvider("elevenlabs"));
        Assert.Equal(
            ["VoiceInk.Windows.Transcription.OpenAICompatible.Soniox.ApiKey"],
            TranscriptionConfiguration.SecretNamesForCloudProvider("soniox"));
        Assert.Equal(
            ["VoiceInk.Windows.Transcription.OpenAICompatible.Speechmatics.ApiKey"],
            TranscriptionConfiguration.SecretNamesForCloudProvider("speechmatics"));
        Assert.Equal(
            ["VoiceInk.Windows.Transcription.OpenAICompatible.Gemini.ApiKey"],
            TranscriptionConfiguration.SecretNamesForCloudProvider("gemini"));
        Assert.Equal(
            ["VoiceInk.Windows.Transcription.OpenAICompatible.xAI.ApiKey"],
            TranscriptionConfiguration.SecretNamesForCloudProvider("xai"));
        Assert.Equal(
            ["VoiceInk.Windows.Transcription.OpenAICompatible.Cartesia.ApiKey"],
            TranscriptionConfiguration.SecretNamesForCloudProvider("cartesia"));
    }

    [Fact]
    public void BuildOptions_NormalizesLocalWhisperLanguageForEnglishOnlyModel()
    {
        var options = TranscriptionConfiguration.BuildOptions(
            new AppSettings
            {
                ModelPath = "C:\\Models\\ggml-base.en.bin",
                Language = "fr"
            },
            prompt: "");

        Assert.Equal("en", options.Language);
    }

    [Fact]
    public void BuildOptions_KeepsCloudLanguageIndependentFromLocalWhisperModel()
    {
        var options = TranscriptionConfiguration.BuildOptions(
            new AppSettings
            {
                TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
                ModelPath = "C:\\Models\\ggml-base.en.bin",
                Language = "fr",
                CloudTranscriptionEndpoint = "https://api.example.test/v1/audio/transcriptions",
                CloudTranscriptionModel = "whisper-large-v3"
            },
            prompt: "");

        Assert.Equal("fr", options.Language);
        Assert.Equal(TranscriptionProviderKind.OpenAICompatible, options.Provider);
    }
}
