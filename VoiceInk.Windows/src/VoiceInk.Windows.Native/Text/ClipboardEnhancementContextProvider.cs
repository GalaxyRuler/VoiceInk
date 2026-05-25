using Windows.ApplicationModel.DataTransfer;
using VoiceInk.Windows.Core.Enhancement;

namespace VoiceInk.Windows.Native.Text;

public sealed class ClipboardEnhancementContextProvider : IEnhancementContextProvider
{
    private readonly int maxCharacters;

    public ClipboardEnhancementContextProvider(int maxCharacters = 4_000)
    {
        this.maxCharacters = Math.Max(1, maxCharacters);
    }

    public async Task<EnhancementContext> GetContextAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var data = global::Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            if (!data.Contains(StandardDataFormats.Text))
            {
                return EnhancementContext.Empty;
            }

            var text = (await data.GetTextAsync().AsTask(cancellationToken)).Trim();
            cancellationToken.ThrowIfCancellationRequested();
            if (text.Length == 0)
            {
                return EnhancementContext.Empty;
            }

            return new EnhancementContext(text.Length <= maxCharacters ? text : text[..maxCharacters]);
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
}
