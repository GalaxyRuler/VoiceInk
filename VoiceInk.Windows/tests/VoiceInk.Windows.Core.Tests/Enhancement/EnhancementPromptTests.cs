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
    public void PromptLibrary_BuildPromptsReturnsDefaultsWhenNoPromptsArePersisted()
    {
        var prompts = EnhancementPromptLibrary.BuildPrompts([]);

        Assert.Equal(
            EnhancementPromptCatalog.CreateDefaultPrompts().Select(prompt => prompt.Id),
            prompts.Select(prompt => prompt.Id));
    }

    [Fact]
    public void PromptLibrary_BuildPromptsAppendsPersistedCustomPrompts()
    {
        var customPrompt = new EnhancementPrompt(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Standup",
            "Format as a terse standup update.",
            "list.bullet",
            "Daily update",
            IsPredefined: false,
            TriggerWords: ["standup mode"],
            UseSystemInstructions: true);

        var prompts = EnhancementPromptLibrary.BuildPrompts([customPrompt]);

        Assert.Contains(prompts, prompt =>
            prompt.Id == customPrompt.Id
            && prompt.Title == "Standup"
            && !prompt.IsPredefined
            && prompt.TriggerWords.SequenceEqual(["standup mode"]));
    }

    [Fact]
    public void PromptLibrary_BuildPromptsPreservesOnlyPredefinedTriggerOverrides()
    {
        var persistedDefault = new EnhancementPrompt(
            EnhancementPromptCatalog.DefaultPromptId,
            "User edited title",
            "User edited prompt text",
            "terminal.fill",
            "User edited description",
            IsPredefined: true,
            TriggerWords: ["clean mode", "default mode"],
            UseSystemInstructions: false);

        var prompts = EnhancementPromptLibrary.BuildPrompts([persistedDefault]);
        var actualDefault = prompts.Single(prompt => prompt.Id == EnhancementPromptCatalog.DefaultPromptId);

        Assert.Equal("Default", actualDefault.Title);
        Assert.Contains("Clean up the <TRANSCRIPT>", actualDefault.PromptText);
        Assert.Equal("checkmark.seal.fill", actualDefault.Icon);
        Assert.True(actualDefault.UseSystemInstructions);
        Assert.Equal(["clean mode", "default mode"], actualDefault.TriggerWords);
    }

    [Fact]
    public void PromptLibrary_CreateCustomPromptTrimsAndNormalizesTriggerWords()
    {
        var prompt = EnhancementPromptLibrary.CreateCustomPrompt(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "  Standup  ",
            "  Format as a terse standup update.  ",
            "",
            "  Daily update  ",
            " standup mode, Standup Mode, , sync mode ",
            useSystemInstructions: true);

        Assert.Equal("Standup", prompt.Title);
        Assert.Equal("Format as a terse standup update.", prompt.PromptText);
        Assert.Equal("doc.text.fill", prompt.Icon);
        Assert.Equal("Daily update", prompt.Description);
        Assert.False(prompt.IsPredefined);
        Assert.Equal(["standup mode", "sync mode"], prompt.TriggerWords);
    }

    [Fact]
    public void PromptLibrary_DeletePromptRemovesOnlyCustomPrompts()
    {
        var customPrompt = EnhancementPromptLibrary.CreateCustomPrompt(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Custom",
            "Custom instructions",
            "note",
            null,
            "",
            useSystemInstructions: true);
        var prompts = EnhancementPromptLibrary.BuildPrompts([customPrompt]);

        var afterDeletingDefault = EnhancementPromptLibrary.DeletePrompt(
            prompts,
            EnhancementPromptCatalog.DefaultPromptId);
        var afterDeletingCustom = EnhancementPromptLibrary.DeletePrompt(
            prompts,
            customPrompt.Id);

        Assert.Contains(afterDeletingDefault, prompt => prompt.Id == EnhancementPromptCatalog.DefaultPromptId);
        Assert.DoesNotContain(afterDeletingCustom, prompt => prompt.Id == customPrompt.Id);
    }

    [Fact]
    public void PromptLibrary_PersistentPromptsKeepsCustomPromptsAndPredefinedTriggerOverrides()
    {
        var prompts = EnhancementPromptLibrary.BuildPrompts(
            [
                new EnhancementPrompt(
                    EnhancementPromptCatalog.DefaultPromptId,
                    "Ignored",
                    "Ignored",
                    "ignored",
                    null,
                    IsPredefined: true,
                    TriggerWords: ["clean mode"],
                    UseSystemInstructions: false),
                new EnhancementPrompt(
                    Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    "Custom",
                    "Custom instructions",
                    "note",
                    null,
                    IsPredefined: false,
                    TriggerWords: [],
                    UseSystemInstructions: true)
            ]);

        var persistent = EnhancementPromptLibrary.PersistentPrompts(prompts);

        Assert.Contains(persistent, prompt =>
            prompt.Id == EnhancementPromptCatalog.DefaultPromptId
            && prompt.IsPredefined
            && prompt.TriggerWords.SequenceEqual(["clean mode"]));
        Assert.Contains(persistent, prompt => prompt.Title == "Custom" && !prompt.IsPredefined);
        Assert.DoesNotContain(persistent, prompt =>
            prompt.IsPredefined
            && prompt.Id != EnhancementPromptCatalog.DefaultPromptId);
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
    public void Render_AppendsSelectedTextBeforeClipboardAndVocabulary()
    {
        var prompt = EnhancementPromptCatalog.CreateDefaultPrompts()
            .Single(item => item.Id == EnhancementPromptCatalog.DefaultPromptId);
        var vocabulary = new[]
        {
            new VocabularyWord(Guid.NewGuid(), "VoiceInk", DateTimeOffset.UtcNow)
        };

        var rendered = EnhancementPromptRenderer.Render(
            prompt,
            "fix this",
            vocabulary,
            new EnhancementContext(
                ClipboardText: "clipboard note",
                SelectedText: "selected note"));

        Assert.Contains("<CURRENTLY_SELECTED_TEXT>", rendered.SystemMessage);
        Assert.Contains("selected note", rendered.SystemMessage);
        Assert.Contains("</CURRENTLY_SELECTED_TEXT>", rendered.SystemMessage);
        var selectedTextIndex = rendered.SystemMessage.LastIndexOf("<CURRENTLY_SELECTED_TEXT>", StringComparison.Ordinal);
        var clipboardIndex = rendered.SystemMessage.LastIndexOf("<CLIPBOARD_CONTEXT>", StringComparison.Ordinal);
        var vocabularyIndex = rendered.SystemMessage.LastIndexOf("<CUSTOM_VOCABULARY>", StringComparison.Ordinal);

        Assert.True(
            selectedTextIndex < clipboardIndex,
            "Selected text context should render before clipboard context.");
        Assert.True(
            clipboardIndex < vocabularyIndex,
            "Clipboard context should render before vocabulary context.");
    }

    [Fact]
    public void Render_AppendsActiveWindowContextBeforeSelectedTextClipboardAndVocabulary()
    {
        var prompt = EnhancementPromptCatalog.CreateDefaultPrompts()
            .Single(item => item.Id == EnhancementPromptCatalog.DefaultPromptId);
        var vocabulary = new[]
        {
            new VocabularyWord(Guid.NewGuid(), "VoiceInk", DateTimeOffset.UtcNow)
        };

        var rendered = EnhancementPromptRenderer.Render(
            prompt,
            "fix this",
            vocabulary,
            new EnhancementContext(
                ClipboardText: "clipboard note",
                SelectedText: "selected note",
                ActiveWindowProcessName: "WINWORD",
                ActiveWindowTitle: "Quarterly Planning"));

        Assert.Contains("<ACTIVE_WINDOW_CONTEXT>", rendered.SystemMessage);
        Assert.Contains("Process: WINWORD", rendered.SystemMessage);
        Assert.Contains("Title: Quarterly Planning", rendered.SystemMessage);
        Assert.Contains("</ACTIVE_WINDOW_CONTEXT>", rendered.SystemMessage);
        var activeWindowIndex = rendered.SystemMessage.LastIndexOf("<ACTIVE_WINDOW_CONTEXT>", StringComparison.Ordinal);
        var selectedTextIndex = rendered.SystemMessage.LastIndexOf("<CURRENTLY_SELECTED_TEXT>", StringComparison.Ordinal);
        var clipboardIndex = rendered.SystemMessage.LastIndexOf("<CLIPBOARD_CONTEXT>", StringComparison.Ordinal);
        var vocabularyIndex = rendered.SystemMessage.LastIndexOf("<CUSTOM_VOCABULARY>", StringComparison.Ordinal);

        Assert.True(activeWindowIndex < selectedTextIndex, "Active window context should render before selected text.");
        Assert.True(selectedTextIndex < clipboardIndex, "Selected text context should render before clipboard context.");
        Assert.True(clipboardIndex < vocabularyIndex, "Clipboard context should render before vocabulary context.");
    }

    [Fact]
    public void Render_AppendsSanitizedBrowserUrlContextAfterActiveWindow()
    {
        var prompt = EnhancementPromptCatalog.CreateDefaultPrompts()
            .Single(item => item.Id == EnhancementPromptCatalog.DefaultPromptId);

        var rendered = EnhancementPromptRenderer.Render(
            prompt,
            "fix this",
            vocabulary: [],
            new EnhancementContext(
                SelectedText: "selected note",
                ActiveWindowProcessName: "msedge",
                ActiveWindowTitle: "Docs",
                BrowserUrl: "https://example.com/docs?token=secret#part"));

        Assert.Contains("<BROWSER_URL_CONTEXT>", rendered.SystemMessage);
        Assert.Contains("URL: https://example.com/docs", rendered.SystemMessage);
        Assert.DoesNotContain("token=secret", rendered.SystemMessage);
        Assert.DoesNotContain("#part", rendered.SystemMessage);
        Assert.Contains("</BROWSER_URL_CONTEXT>", rendered.SystemMessage);
        var activeWindowIndex = rendered.SystemMessage.LastIndexOf("<ACTIVE_WINDOW_CONTEXT>", StringComparison.Ordinal);
        var browserUrlIndex = rendered.SystemMessage.LastIndexOf("<BROWSER_URL_CONTEXT>", StringComparison.Ordinal);
        var selectedTextIndex = rendered.SystemMessage.LastIndexOf("<CURRENTLY_SELECTED_TEXT>", StringComparison.Ordinal);

        Assert.True(activeWindowIndex < browserUrlIndex, "Browser URL context should render after active window.");
        Assert.True(browserUrlIndex < selectedTextIndex, "Browser URL context should render before selected text.");
    }

    [Fact]
    public void Render_AppendsScreenOcrContextBeforeSelectedTextClipboardAndVocabulary()
    {
        var prompt = EnhancementPromptCatalog.CreateDefaultPrompts()
            .Single(item => item.Id == EnhancementPromptCatalog.DefaultPromptId);
        var vocabulary = new[]
        {
            new VocabularyWord(Guid.NewGuid(), "VoiceInk", DateTimeOffset.UtcNow)
        };

        var rendered = EnhancementPromptRenderer.Render(
            prompt,
            "fix this",
            vocabulary,
            new EnhancementContext(
                ClipboardText: "clipboard note",
                SelectedText: "selected note",
                ActiveWindowProcessName: "msedge",
                ActiveWindowTitle: "Dashboard",
                BrowserUrl: "https://example.com/dashboard",
                OcrText: "Invoice total forty two dollars"));

        Assert.Contains("<SCREEN_OCR_CONTEXT>", rendered.SystemMessage);
        Assert.Contains("Invoice total forty two dollars", rendered.SystemMessage);
        Assert.Contains("</SCREEN_OCR_CONTEXT>", rendered.SystemMessage);
        var browserUrlIndex = rendered.SystemMessage.LastIndexOf("<BROWSER_URL_CONTEXT>", StringComparison.Ordinal);
        var ocrIndex = rendered.SystemMessage.LastIndexOf("<SCREEN_OCR_CONTEXT>", StringComparison.Ordinal);
        var selectedTextIndex = rendered.SystemMessage.LastIndexOf("<CURRENTLY_SELECTED_TEXT>", StringComparison.Ordinal);
        var clipboardIndex = rendered.SystemMessage.LastIndexOf("<CLIPBOARD_CONTEXT>", StringComparison.Ordinal);
        var vocabularyIndex = rendered.SystemMessage.LastIndexOf("<CUSTOM_VOCABULARY>", StringComparison.Ordinal);

        Assert.True(browserUrlIndex < ocrIndex, "OCR context should render after browser URL context.");
        Assert.True(ocrIndex < selectedTextIndex, "OCR context should render before selected text.");
        Assert.True(selectedTextIndex < clipboardIndex, "Selected text context should render before clipboard context.");
        Assert.True(clipboardIndex < vocabularyIndex, "Clipboard context should render before vocabulary context.");
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
