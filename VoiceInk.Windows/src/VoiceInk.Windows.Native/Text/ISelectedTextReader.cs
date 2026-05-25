namespace VoiceInk.Windows.Native.Text;

public interface ISelectedTextReader
{
    Task<string> GetSelectedTextAsync(CancellationToken cancellationToken);
}
