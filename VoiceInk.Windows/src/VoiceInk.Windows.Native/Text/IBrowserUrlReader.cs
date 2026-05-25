namespace VoiceInk.Windows.Native.Text;

public interface IBrowserUrlReader
{
    Task<string> GetBrowserUrlAsync(CancellationToken cancellationToken);
}
