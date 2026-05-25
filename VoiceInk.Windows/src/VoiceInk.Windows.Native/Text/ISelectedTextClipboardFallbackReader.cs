namespace VoiceInk.Windows.Native.Text;

public interface ISelectedTextClipboardFallbackReader
{
    Task<string> GetSelectedTextAsync(CancellationToken cancellationToken);
}
