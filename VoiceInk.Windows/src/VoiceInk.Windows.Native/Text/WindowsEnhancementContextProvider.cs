using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.PowerMode;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Native.PowerMode;

namespace VoiceInk.Windows.Native.Text;

public sealed class WindowsEnhancementContextProvider : IEnhancementContextProvider
{
    private readonly IClipboardTextReader clipboardProvider;
    private readonly ISelectedTextReader selectedTextProvider;
    private readonly ISelectedTextClipboardFallbackReader selectedTextFallbackProvider;
    private readonly IPowerModeTargetProvider activeWindowProvider;
    private readonly IBrowserUrlReader browserUrlProvider;

    public WindowsEnhancementContextProvider()
        : this(
            new ClipboardEnhancementContextProvider(),
            new SelectedTextEnhancementContextProvider(),
            new SelectedTextClipboardFallbackReader(),
            new ActiveWindowPowerModeTargetProvider(),
            new BrowserUrlEnhancementContextProvider())
    {
    }

    public WindowsEnhancementContextProvider(
        IClipboardTextReader clipboardProvider,
        ISelectedTextReader selectedTextProvider,
        ISelectedTextClipboardFallbackReader selectedTextFallbackProvider,
        IPowerModeTargetProvider? activeWindowProvider = null,
        IBrowserUrlReader? browserUrlProvider = null)
    {
        this.clipboardProvider = clipboardProvider;
        this.selectedTextProvider = selectedTextProvider;
        this.selectedTextFallbackProvider = selectedTextFallbackProvider;
        this.activeWindowProvider = activeWindowProvider ?? new EmptyPowerModeTargetProvider();
        this.browserUrlProvider = browserUrlProvider ?? new EmptyBrowserUrlReader();
    }

    public async Task<EnhancementContext> GetContextAsync(
        EnhancementContextRequest request,
        CancellationToken cancellationToken)
    {
        var selectedText = request.IncludeSelectedText
            ? await ReadSelectedTextAsync(cancellationToken)
            : string.Empty;
        var clipboardText = request.IncludeClipboard
            ? await ReadClipboardTextAsync(cancellationToken)
            : string.Empty;
        var activeWindow = request.IncludeActiveWindow
            ? await ReadActiveWindowAsync(cancellationToken)
            : null;
        var browserUrl = request.IncludeBrowserUrl
            ? await ReadBrowserUrlAsync(cancellationToken)
            : string.Empty;

        return new EnhancementContext(
            clipboardText,
            selectedText,
            activeWindow?.ProcessName ?? string.Empty,
            activeWindow?.WindowTitle ?? string.Empty,
            browserUrl);
    }

    private async Task<string> ReadSelectedTextAsync(CancellationToken cancellationToken)
    {
        try
        {
            var selectedText = await selectedTextProvider.GetSelectedTextAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(selectedText))
            {
                return selectedText;
            }

            return await selectedTextFallbackProvider.GetSelectedTextAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return string.Empty;
        }
    }

    private async Task<string> ReadClipboardTextAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await clipboardProvider.GetClipboardTextAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return string.Empty;
        }
    }

    private async Task<PowerModeTarget?> ReadActiveWindowAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await activeWindowProvider.GetCurrentTargetAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private async Task<string> ReadBrowserUrlAsync(CancellationToken cancellationToken)
    {
        try
        {
            var browserUrl = await browserUrlProvider.GetBrowserUrlAsync(cancellationToken);
            return BrowserUrlContextSanitizer.Sanitize(browserUrl);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return string.Empty;
        }
    }

    private sealed class EmptyPowerModeTargetProvider : IPowerModeTargetProvider
    {
        public Task<PowerModeTarget?> GetCurrentTargetAsync(CancellationToken cancellationToken) =>
            Task.FromResult<PowerModeTarget?>(null);
    }

    private sealed class EmptyBrowserUrlReader : IBrowserUrlReader
    {
        public Task<string> GetBrowserUrlAsync(CancellationToken cancellationToken) =>
            Task.FromResult(string.Empty);
    }
}
