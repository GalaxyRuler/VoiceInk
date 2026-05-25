using Windows.ApplicationModel.DataTransfer;
using VoiceInk.Windows.Core.Enhancement;

namespace VoiceInk.Windows.Native.Text;

public sealed class ClipboardEnhancementContextProvider : IEnhancementContextProvider, IClipboardTextReader
{
    private readonly int maxCharacters;

    public ClipboardEnhancementContextProvider(int maxCharacters = 4_000)
    {
        this.maxCharacters = Math.Max(1, maxCharacters);
    }

    public async Task<EnhancementContext> GetContextAsync(
        EnhancementContextRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.IncludeClipboard)
        {
            return EnhancementContext.Empty;
        }

        return new EnhancementContext(await GetClipboardTextAsync(cancellationToken));
    }

    public async Task<string> GetClipboardTextAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var data = global::Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            if (!data.Contains(StandardDataFormats.Text))
            {
                return string.Empty;
            }

            var text = (await data.GetTextAsync().AsTask(cancellationToken)).Trim();
            cancellationToken.ThrowIfCancellationRequested();
            if (text.Length == 0)
            {
                return string.Empty;
            }

            return text.Length <= maxCharacters ? text : text[..maxCharacters];
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
