namespace VoiceInk.Windows.Native.Text;

public interface IClipboardTextReader
{
    Task<string> GetClipboardTextAsync(CancellationToken cancellationToken);
}
