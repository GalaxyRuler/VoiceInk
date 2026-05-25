using VoiceInk.Windows.Core.Enhancement;

namespace VoiceInk.Windows.Native.Text;

public sealed class WindowsEnhancementContextProvider : IEnhancementContextProvider
{
    private readonly ClipboardEnhancementContextProvider clipboardProvider;
    private readonly SelectedTextEnhancementContextProvider selectedTextProvider;

    public WindowsEnhancementContextProvider()
        : this(new ClipboardEnhancementContextProvider(), new SelectedTextEnhancementContextProvider())
    {
    }

    public WindowsEnhancementContextProvider(
        ClipboardEnhancementContextProvider clipboardProvider,
        SelectedTextEnhancementContextProvider selectedTextProvider)
    {
        this.clipboardProvider = clipboardProvider;
        this.selectedTextProvider = selectedTextProvider;
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
            return await selectedTextProvider.GetSelectedTextAsync(cancellationToken);
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
