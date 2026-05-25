using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Enhancement;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Enhancement;

public sealed class EnhancementPromptTests
{
    [Fact]
    public void DefaultCatalog_ContainsStableDefaultAndAssistantPrompts()
    {
        var prompts = EnhancementPromptCatalog.CreateDefaultPrompts();

        Assert.Contains(prompts, prompt =>
            prompt.Id == EnhancementPromptCatalog.DefaultPromptId &&
            prompt.Title == "Default" &&
            prompt.IsPredefined &&
            prompt.UseSystemInstructions);
        Assert.Contains(prompts, prompt =>
            prompt.Id == EnhancementPromptCatalog.AssistantPromptId &&
            prompt.Title == "Assistant" &&
            prompt.IsPredefined &&
            !prompt.UseSystemInstructions);
        Assert.Contains(prompts, prompt => prompt.Title == "Chat");
        Assert.Contains(prompts, prompt => prompt.Title == "Email");
        Assert.Contains(prompts, prompt => prompt.Title == "Rewrite");
    }

    [Fact]
    public void Render_WrapsNormalPromptWithSystemInstructionsAndTranscript()
    {
        var prompt = EnhancementPromptCatalog.CreateDefaultPrompts()
            .Single(item => item.Id == EnhancementPromptCatalog.DefaultPromptId);

        var rendered = EnhancementPromptRenderer.Render(
            prompt,
            "um hello world",
            vocabulary: []);

        Assert.Equal("Default", rendered.PromptName);
        Assert.Contains("TRANSCRIPTION ENHANCER", rendered.SystemMessage);
        Assert.Contains("Output only the cleaned text", rendered.SystemMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<TRANSCRIPT>", rendered.UserMessage);
        Assert.Contains("um hello world", rendered.UserMessage);
        Assert.Contains("</TRANSCRIPT>", rendered.UserMessage);
    }

    [Fact]
    public void Render_AppendsVocabularyContext()
    {
        var prompt = EnhancementPromptCatalog.CreateDefaultPrompts()
            .Single(item => item.Id == EnhancementPromptCatalog.DefaultPromptId);
        var vocabulary = new[]
        {
            new VocabularyWord(Guid.NewGuid(), "VoiceInk", DateTimeOffset.UtcNow),
            new VocabularyWord(Guid.NewGuid(), "Whisper", DateTimeOffset.UtcNow)
        };

        var rendered = EnhancementPromptRenderer.Render(prompt, "voice ink", vocabulary);

        Assert.Contains("<CUSTOM_VOCABULARY>", rendered.SystemMessage);
        Assert.Contains("VoiceInk", rendered.SystemMessage);
        Assert.Contains("Whisper", rendered.SystemMessage);
        Assert.Contains("</CUSTOM_VOCABULARY>", rendered.SystemMessage);
    }

    [Fact]
    public void Render_AppendsClipboardContext()
    {
        var prompt = EnhancementPromptCatalog.CreateDefaultPrompts()
            .Single(item => item.Id == EnhancementPromptCatalog.DefaultPromptId);

        var rendered = EnhancementPromptRenderer.Render(
            prompt,
            "send the note",
            vocabulary: [],
            context: new EnhancementContext(ClipboardText: "Project Zephyr release notes"));

        Assert.Contains("<CLIPBOARD_CONTEXT>", rendered.SystemMessage);
        Assert.Contains("Project Zephyr release notes", rendered.SystemMessage);
        Assert.Contains("</CLIPBOARD_CONTEXT>", rendered.SystemMessage);
    }

    [Fact]
    public void Render_AssistantPromptUsesRawAssistantInstructions()
    {
        var prompt = EnhancementPromptCatalog.CreateDefaultPrompts()
            .Single(item => item.Id == EnhancementPromptCatalog.AssistantPromptId);

        var rendered = EnhancementPromptRenderer.Render(prompt, "summarize this", vocabulary: []);

        Assert.Contains("powerful AI assistant", rendered.SystemMessage);
        Assert.DoesNotContain("TRANSCRIPTION ENHANCER", rendered.SystemMessage);
        Assert.Contains("<TRANSCRIPT>", rendered.UserMessage);
    }

    [Fact]
    public void OutputFilter_StripsThinkingReasoningBlocksAndTrims()
    {
        var text = """
            <thinking>hidden</thinking>
            Clean text
            <think>also hidden</think>
            <reasoning>private</reasoning>
            """;

        var filtered = EnhancementOutputFilter.Filter(text);

        Assert.Equal("Clean text", filtered);
    }

    [Fact]
    public void PromptDetection_StripsLeadingOrTrailingTriggersLongestFirst()
    {
        var defaultId = EnhancementPromptCatalog.DefaultPromptId;
        var chatId = Guid.NewGuid();
        var prompts = new[]
        {
            new EnhancementPrompt(defaultId, "Default", "default", "check", null, true, [], true),
            new EnhancementPrompt(chatId, "Chat", "chat", "bubble", null, false, ["ai", "ai chat"], true)
        };

        var leading = PromptDetectionService.Analyze(
            "ai chat, make this shorter",
            prompts,
            isEnhancementEnabled: false,
            selectedPromptId: defaultId);
        var trailing = PromptDetectionService.Analyze(
            "make this shorter, ai chat.",
            prompts,
            isEnhancementEnabled: false,
            selectedPromptId: defaultId);

        Assert.True(leading.ShouldEnableEnhancement);
        Assert.Equal(chatId, leading.SelectedPromptId);
        Assert.Equal("ai chat", leading.DetectedTriggerWord);
        Assert.Equal("Make this shorter", leading.ProcessedText);
        Assert.Equal("Make this shorter", trailing.ProcessedText);
    }

    [Fact]
    public void PromptDetection_DoesNotMatchInsideWords()
    {
        var promptId = Guid.NewGuid();
        var prompts = new[]
        {
            new EnhancementPrompt(promptId, "Chat", "chat", "bubble", null, false, ["ai"], true)
        };

        var result = PromptDetectionService.Analyze(
            "aide memo",
            prompts,
            isEnhancementEnabled: false,
            selectedPromptId: null);

        Assert.False(result.ShouldEnableEnhancement);
        Assert.Equal("aide memo", result.ProcessedText);
    }
}
