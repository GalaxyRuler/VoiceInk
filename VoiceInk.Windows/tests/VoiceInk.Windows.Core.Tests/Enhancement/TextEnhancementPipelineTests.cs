using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.Settings;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Enhancement;

public sealed class TextEnhancementPipelineTests
{
    [Fact]
    public async Task EnhanceAsync_DisabledEnhancementReturnsOriginalTextWithoutProviderCall()
    {
        var provider = new FakeTextEnhancementService("ignored");
        var pipeline = new TextEnhancementPipeline(provider);
        var settings = ConfiguredSettings() with { IsEnhancementEnabled = false };

        var result = await pipeline.EnhanceAsync("hello", settings, vocabulary: [], CancellationToken.None);

        Assert.False(result.AttemptedEnhancement);
        Assert.Equal("hello", result.FinalText);
        Assert.Null(result.EnhancedText);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task EnhanceAsync_EnabledEnhancementReturnsFilteredEnhancedTextAndMetadata()
    {
        var provider = new FakeTextEnhancementService("<think>private</think> Hello there.");
        var pipeline = new TextEnhancementPipeline(provider);
        var settings = ConfiguredSettings() with
        {
            IsEnhancementEnabled = true,
            SelectedEnhancementPromptId = EnhancementPromptCatalog.DefaultPromptId
        };

        var result = await pipeline.EnhanceAsync(
            "hello there",
            settings,
            vocabulary: [new VocabularyWord(Guid.NewGuid(), "VoiceInk", DateTimeOffset.UtcNow)],
            CancellationToken.None);

        Assert.True(result.AttemptedEnhancement);
        Assert.Equal("Hello there.", result.FinalText);
        Assert.Equal("Hello there.", result.EnhancedText);
        Assert.Equal("Default", result.PromptName);
        Assert.Equal("openai-compatible", result.EnhancementProviderName);
        Assert.Equal("test-model", result.EnhancementModelName);
        Assert.NotNull(result.EnhancementDuration);
        Assert.Contains("<CUSTOM_VOCABULARY>", provider.LastRequest!.SystemMessage);
        Assert.Contains("<TRANSCRIPT>", provider.LastRequest.UserMessage);
    }

    [Fact]
    public async Task EnhanceAsync_TriggerWordsEnableEnhancementAndStripTriggerText()
    {
        var promptId = Guid.NewGuid();
        var provider = new FakeTextEnhancementService("A shorter version.");
        var pipeline = new TextEnhancementPipeline(
            provider,
            prompts:
            [
                new EnhancementPrompt(
                    EnhancementPromptCatalog.DefaultPromptId,
                    "Default",
                    "default prompt",
                    "check",
                    null,
                    true,
                    [],
                    true),
                new EnhancementPrompt(
                    promptId,
                    "Chat",
                    "make it chatty",
                    "bubble",
                    null,
                    false,
                    ["chat mode"],
                    true)
            ]);
        var settings = ConfiguredSettings() with
        {
            IsEnhancementEnabled = false,
            SelectedEnhancementPromptId = EnhancementPromptCatalog.DefaultPromptId
        };

        var result = await pipeline.EnhanceAsync("chat mode, make this shorter", settings, [], CancellationToken.None);

        Assert.True(result.AttemptedEnhancement);
        Assert.Equal("Chat", result.PromptName);
        Assert.Contains("Make this shorter", provider.LastRequest!.UserMessage);
        Assert.DoesNotContain("chat mode", provider.LastRequest.UserMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnhanceAsync_SkipsShortTextUnlessTriggerWordWasDetected()
    {
        var provider = new FakeTextEnhancementService("Enhanced");
        var pipeline = new TextEnhancementPipeline(provider);
        var settings = ConfiguredSettings() with
        {
            IsEnhancementEnabled = true,
            SkipShortEnhancement = true,
            ShortEnhancementWordThreshold = 3
        };

        var result = await pipeline.EnhanceAsync("yes please", settings, [], CancellationToken.None);

        Assert.False(result.AttemptedEnhancement);
        Assert.Equal("yes please", result.FinalText);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task EnhanceAsync_ProviderFailureReturnsOriginalTextAndWarning()
    {
        var provider = new FakeTextEnhancementService("ignored")
        {
            Exception = new InvalidOperationException("provider unavailable")
        };
        var pipeline = new TextEnhancementPipeline(provider);
        var settings = ConfiguredSettings() with { IsEnhancementEnabled = true };

        var result = await pipeline.EnhanceAsync("hello", settings, [], CancellationToken.None);

        Assert.True(result.AttemptedEnhancement);
        Assert.Equal("hello", result.FinalText);
        Assert.Null(result.EnhancedText);
        Assert.Contains("provider unavailable", result.WarningMessage);
    }

    private static AppSettings ConfiguredSettings() =>
        new()
        {
            EnhancementEndpoint = "https://example.test/v1/chat/completions",
            EnhancementModel = "test-model",
            EnhancementTimeoutSeconds = 7,
            EnhancementRetryOnTimeout = true,
            SkipShortEnhancement = false
        };

    private sealed class FakeTextEnhancementService(string text) : ITextEnhancementService
    {
        public int CallCount { get; private set; }
        public TextEnhancementRequest? LastRequest { get; private set; }
        public Exception? Exception { get; init; }

        public Task<TextEnhancementResult> EnhanceAsync(
            TextEnhancementRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequest = request;

            if (Exception is not null)
            {
                throw Exception;
            }

            return Task.FromResult(new TextEnhancementResult(
                text,
                "openai-compatible",
                request.Model,
                TimeSpan.FromMilliseconds(42)));
        }
    }
}
