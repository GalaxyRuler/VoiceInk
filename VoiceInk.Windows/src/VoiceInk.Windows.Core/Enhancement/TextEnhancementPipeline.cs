using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Enhancement;

public sealed class TextEnhancementPipeline
{
    private const int DefaultTimeoutSeconds = 7;
    private const int DefaultShortWordThreshold = 3;
    private const int DefaultMaxRetries = 3;
    private const double DefaultTemperature = 0.3;

    private readonly ITextEnhancementService enhancementService;
    private readonly Func<IReadOnlyList<EnhancementPrompt>> promptsProvider;
    private readonly IEnhancementContextProvider contextProvider;

    public TextEnhancementPipeline(
        ITextEnhancementService enhancementService,
        IReadOnlyList<EnhancementPrompt>? prompts = null,
        IEnhancementContextProvider? contextProvider = null)
    {
        this.enhancementService = enhancementService;
        var staticPrompts = prompts is { Count: > 0 }
            ? prompts
            : EnhancementPromptCatalog.CreateDefaultPrompts();
        promptsProvider = () => staticPrompts;
        this.contextProvider = contextProvider ?? new EmptyEnhancementContextProvider();
    }

    public TextEnhancementPipeline(
        ITextEnhancementService enhancementService,
        Func<IReadOnlyList<EnhancementPrompt>> promptsProvider,
        IEnhancementContextProvider? contextProvider = null)
    {
        this.enhancementService = enhancementService;
        this.promptsProvider = () =>
        {
            var prompts = promptsProvider();
            return prompts.Count > 0 ? prompts : EnhancementPromptCatalog.CreateDefaultPrompts();
        };
        this.contextProvider = contextProvider ?? new EmptyEnhancementContextProvider();
    }

    public async Task<TextEnhancementPipelineResult> EnhanceAsync(
        string text,
        AppSettings settings,
        IReadOnlyList<VocabularyWord> vocabulary,
        CancellationToken cancellationToken)
    {
        if (text.Length == 0)
        {
            return OriginalOnly(text);
        }

        var selectedPromptId = settings.SelectedEnhancementPromptId ?? EnhancementPromptCatalog.DefaultPromptId;
        var prompts = promptsProvider();
        var detection = PromptDetectionService.Analyze(
            text,
            prompts,
            settings.IsEnhancementEnabled,
            selectedPromptId);

        if (!settings.IsEnhancementEnabled && !detection.ShouldEnableEnhancement)
        {
            return OriginalOnly(text);
        }

        if (!IsProviderConfigured(settings))
        {
            return OriginalOnly(text, "AI enhancement provider is not configured.");
        }

        if (ShouldSkipShortEnhancement(settings, detection, text))
        {
            return OriginalOnly(text);
        }

        var prompt = PromptFor(detection.SelectedPromptId ?? selectedPromptId);
        var context = await GetContextAsync(
            new EnhancementContextRequest(
                IncludeClipboard: settings.UseClipboardContext,
                IncludeSelectedText: true,
                IncludeOcr: settings.UseOcrContext),
            cancellationToken);
        var rendered = EnhancementPromptRenderer.Render(prompt, detection.ProcessedText, vocabulary, context);
        var request = new TextEnhancementRequest(
            settings.EnhancementEndpoint.Trim(),
            settings.EnhancementModel.Trim(),
            rendered.SystemMessage,
            rendered.UserMessage,
            TimeSpan.FromSeconds(TimeoutSeconds(settings)),
            DefaultTemperature,
            settings.EnhancementRetryOnTimeout,
            DefaultMaxRetries,
            EnhancementProviderPresetCatalog.Resolve(settings.EnhancementProviderId).Id);

        try
        {
            var result = await enhancementService.EnhanceAsync(request, cancellationToken);
            var filtered = EnhancementOutputFilter.Filter(result.Text);
            if (filtered.Length == 0)
            {
                return OriginalOnly(text, "AI enhancement returned no text.", attemptedEnhancement: true);
            }

            return new TextEnhancementPipelineResult(
                OriginalText: text,
                FinalText: filtered,
                AttemptedEnhancement: true,
                EnhancedText: filtered,
                PromptName: rendered.PromptName,
                EnhancementProviderName: result.ProviderName,
                EnhancementModelName: result.ModelName,
                EnhancementDuration: result.Duration,
                SystemMessage: rendered.SystemMessage,
                UserMessage: rendered.UserMessage);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return OriginalOnly(text, $"Enhancement failed: {ex.Message}", attemptedEnhancement: true);
        }
    }

    private EnhancementPrompt PromptFor(Guid promptId)
    {
        var prompts = promptsProvider();
        return
        prompts.FirstOrDefault(prompt => prompt.Id == promptId)
        ?? prompts.FirstOrDefault(prompt => prompt.Id == EnhancementPromptCatalog.DefaultPromptId)
        ?? prompts[0];
    }

    private async Task<EnhancementContext> GetContextAsync(
        EnhancementContextRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await contextProvider.GetContextAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return EnhancementContext.Empty;
        }
    }

    private static bool IsProviderConfigured(AppSettings settings)
    {
        var provider = EnhancementProviderPresetCatalog.Resolve(settings.EnhancementProviderId);
        if (provider.Id == EnhancementProviderPresetCatalog.LocalCli.Id)
        {
            return !string.IsNullOrWhiteSpace(settings.EnhancementEndpoint);
        }

        return !string.IsNullOrWhiteSpace(settings.EnhancementEndpoint)
            && !string.IsNullOrWhiteSpace(settings.EnhancementModel);
    }

    private static bool ShouldSkipShortEnhancement(
        AppSettings settings,
        PromptDetectionResult detection,
        string text) =>
        settings.SkipShortEnhancement
        && !detection.ShouldEnableEnhancement
        && WordCount(text) <= ShortThreshold(settings);

    private static int WordCount(string text) =>
        text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

    private static int TimeoutSeconds(AppSettings settings) =>
        settings.EnhancementTimeoutSeconds > 0
            ? settings.EnhancementTimeoutSeconds
            : DefaultTimeoutSeconds;

    private static int ShortThreshold(AppSettings settings) =>
        settings.ShortEnhancementWordThreshold > 0
            ? settings.ShortEnhancementWordThreshold
            : DefaultShortWordThreshold;

    private static TextEnhancementPipelineResult OriginalOnly(
        string text,
        string? warningMessage = null,
        bool attemptedEnhancement = false) =>
        new(
            OriginalText: text,
            FinalText: text,
            AttemptedEnhancement: attemptedEnhancement,
            WarningMessage: warningMessage);
}
