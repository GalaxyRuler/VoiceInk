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
        var contextProvider = new FakeEnhancementContextProvider(new EnhancementContext("ignored clipboard"));
        var pipeline = new TextEnhancementPipeline(provider, contextProvider: contextProvider);
        var settings = ConfiguredSettings() with
        {
            IsEnhancementEnabled = false,
            UseClipboardContext = true
        };

        var result = await pipeline.EnhanceAsync("hello", settings, vocabulary: [], CancellationToken.None);

        Assert.False(result.AttemptedEnhancement);
        Assert.Equal("hello", result.FinalText);
        Assert.Null(result.EnhancedText);
        Assert.Equal(0, provider.CallCount);
        Assert.Equal(0, contextProvider.CallCount);
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
    public async Task EnhanceAsync_PassesSelectedEnhancementProviderIdAndMetadata()
    {
        var provider = new FakeTextEnhancementService("Enhanced note.")
        {
            UseRequestProviderName = true
        };
        var pipeline = new TextEnhancementPipeline(provider);
        var settings = ConfiguredSettings() with
        {
            IsEnhancementEnabled = true,
            EnhancementProviderId = "groq"
        };

        var result = await pipeline.EnhanceAsync("clean this", settings, [], CancellationToken.None);

        Assert.Equal("groq", provider.LastRequest!.ProviderId);
        Assert.Equal("groq", result.EnhancementProviderName);
    }

    [Fact]
    public async Task EnhanceAsync_UsesCurrentPromptSourceAfterPipelineConstruction()
    {
        var promptId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var prompts = EnhancementPromptCatalog.CreateDefaultPrompts();
        var provider = new FakeTextEnhancementService("Enhanced note.");
        var pipeline = new TextEnhancementPipeline(
            provider,
            promptsProvider: () => prompts);
        prompts =
        [
            new EnhancementPrompt(
                promptId,
                "Standup",
                "Format as a terse standup update.",
                "list.bullet",
                null,
                IsPredefined: false,
                TriggerWords: [],
                UseSystemInstructions: true)
        ];
        var settings = ConfiguredSettings() with
        {
            IsEnhancementEnabled = true,
            SelectedEnhancementPromptId = promptId
        };

        var result = await pipeline.EnhanceAsync("yesterday shipped prompts", settings, [], CancellationToken.None);

        Assert.Equal("Standup", result.PromptName);
        Assert.Contains("terse standup update", provider.LastRequest!.SystemMessage);
    }

    [Fact]
    public async Task EnhanceAsync_IncludesClipboardContextWhenEnabled()
    {
        var provider = new FakeTextEnhancementService("Enhanced note.");
        var contextProvider = new FakeEnhancementContextProvider(new EnhancementContext("Clipboard note"));
        var pipeline = new TextEnhancementPipeline(provider, contextProvider: contextProvider);
        var settings = ConfiguredSettings() with
        {
            IsEnhancementEnabled = true,
            UseClipboardContext = true
        };

        await pipeline.EnhanceAsync("clean this", settings, [], CancellationToken.None);

        Assert.Equal(1, contextProvider.CallCount);
        Assert.Contains("<CLIPBOARD_CONTEXT>", provider.LastRequest!.SystemMessage);
        Assert.Contains("Clipboard note", provider.LastRequest.SystemMessage);
    }

    [Fact]
    public async Task EnhanceAsync_IncludesSelectedTextContextWhenEnhancementRuns()
    {
        var provider = new FakeTextEnhancementService("Enhanced note.");
        var contextProvider = new FakeEnhancementContextProvider(new EnhancementContext(SelectedText: "Selected note"));
        var pipeline = new TextEnhancementPipeline(provider, contextProvider: contextProvider);
        var settings = ConfiguredSettings() with { IsEnhancementEnabled = true };

        await pipeline.EnhanceAsync("clean this", settings, [], CancellationToken.None);

        Assert.Equal(new EnhancementContextRequest(IncludeClipboard: false, IncludeSelectedText: true), contextProvider.LastRequest);
        Assert.Contains("<CURRENTLY_SELECTED_TEXT>", provider.LastRequest!.SystemMessage);
        Assert.Contains("Selected note", provider.LastRequest.SystemMessage);
    }

    [Fact]
    public async Task EnhanceAsync_RequestsClipboardAndSelectedTextWhenClipboardContextEnabled()
    {
        var provider = new FakeTextEnhancementService("Enhanced note.");
        var contextProvider = new FakeEnhancementContextProvider(new EnhancementContext("Clipboard note", "Selected note"));
        var pipeline = new TextEnhancementPipeline(provider, contextProvider: contextProvider);
        var settings = ConfiguredSettings() with
        {
            IsEnhancementEnabled = true,
            UseClipboardContext = true
        };

        await pipeline.EnhanceAsync("clean this", settings, [], CancellationToken.None);

        Assert.Equal(new EnhancementContextRequest(IncludeClipboard: true, IncludeSelectedText: true), contextProvider.LastRequest);
        Assert.Contains("<CURRENTLY_SELECTED_TEXT>", provider.LastRequest!.SystemMessage);
        Assert.Contains("<CLIPBOARD_CONTEXT>", provider.LastRequest.SystemMessage);
    }

    [Fact]
    public async Task EnhanceAsync_WhenClipboardContextProviderFails_EnhancesWithoutContext()
    {
        var provider = new FakeTextEnhancementService("Enhanced note.");
        var contextProvider = new FakeEnhancementContextProvider(EnhancementContext.Empty)
        {
            Exception = new InvalidOperationException("clipboard unavailable")
        };
        var pipeline = new TextEnhancementPipeline(provider, contextProvider: contextProvider);
        var settings = ConfiguredSettings() with
        {
            IsEnhancementEnabled = true,
            UseClipboardContext = true
        };

        var result = await pipeline.EnhanceAsync("clean this", settings, [], CancellationToken.None);

        Assert.True(result.AttemptedEnhancement);
        Assert.Equal("Enhanced note.", result.FinalText);
        Assert.DoesNotContain("<CLIPBOARD_CONTEXT>", provider.LastRequest!.SystemMessage);
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
        var contextProvider = new FakeEnhancementContextProvider(new EnhancementContext("ignored clipboard"));
        var pipeline = new TextEnhancementPipeline(provider, contextProvider: contextProvider);
        var settings = ConfiguredSettings() with
        {
            IsEnhancementEnabled = true,
            UseClipboardContext = true,
            SkipShortEnhancement = true,
            ShortEnhancementWordThreshold = 3
        };

        var result = await pipeline.EnhanceAsync("yes please", settings, [], CancellationToken.None);

        Assert.False(result.AttemptedEnhancement);
        Assert.Equal("yes please", result.FinalText);
        Assert.Equal(0, provider.CallCount);
        Assert.Equal(0, contextProvider.CallCount);
    }

    [Fact]
    public async Task EnhanceAsync_MissingProviderConfigurationDoesNotReadClipboardContext()
    {
        var provider = new FakeTextEnhancementService("ignored");
        var contextProvider = new FakeEnhancementContextProvider(new EnhancementContext("ignored clipboard"));
        var pipeline = new TextEnhancementPipeline(provider, contextProvider: contextProvider);
        var settings = ConfiguredSettings() with
        {
            IsEnhancementEnabled = true,
            UseClipboardContext = true,
            EnhancementEndpoint = string.Empty
        };

        var result = await pipeline.EnhanceAsync("hello", settings, [], CancellationToken.None);

        Assert.False(result.AttemptedEnhancement);
        Assert.Equal("hello", result.FinalText);
        Assert.Equal("AI enhancement provider is not configured.", result.WarningMessage);
        Assert.Equal(0, provider.CallCount);
        Assert.Equal(0, contextProvider.CallCount);
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
        public bool UseRequestProviderName { get; init; }

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
                UseRequestProviderName
                    ? EnhancementConfiguration.ProviderNameFor(request.ProviderId)
                    : "openai-compatible",
                request.Model,
                TimeSpan.FromMilliseconds(42)));
        }
    }

    private sealed class FakeEnhancementContextProvider(EnhancementContext context) : IEnhancementContextProvider
    {
        public int CallCount { get; private set; }
        public EnhancementContextRequest? LastRequest { get; private set; }
        public Exception? Exception { get; init; }

        public Task<EnhancementContext> GetContextAsync(
            EnhancementContextRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequest = request;
            if (Exception is not null)
            {
                throw Exception;
            }

            return Task.FromResult(context);
        }
    }
}
