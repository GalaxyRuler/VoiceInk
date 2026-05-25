using VoiceInk.Windows.Core.Enhancement;

namespace VoiceInk.Windows.Native.Text;

public sealed class WindowsEnhancementContextProvider : IEnhancementContextProvider
{
    private readonly IClipboardTextReader clipboardProvider;
    private readonly ISelectedTextReader selectedTextProvider;
    private readonly ISelectedTextClipboardFallbackReader selectedTextFallbackProvider;

    public WindowsEnhancementContextProvider()
        : this(
            new ClipboardEnhancementContextProvider(),
            new SelectedTextEnhancementContextProvider(),
            new SelectedTextClipboardFallbackReader())
    {
    }

    public WindowsEnhancementContextProvider(
        IClipboardTextReader clipboardProvider,
        ISelectedTextReader selectedTextProvider,
        ISelectedTextClipboardFallbackReader selectedTextFallbackProvider)
    {
        this.clipboardProvider = clipboardProvider;
        this.selectedTextProvider = selectedTextProvider;
        this.selectedTextFallbackProvider = selectedTextFallbackProvider;
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

        return new EnhancementContext(clipboardText, selectedText);
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
}
