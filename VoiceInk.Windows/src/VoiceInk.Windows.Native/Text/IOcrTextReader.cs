namespace VoiceInk.Windows.Native.Text;

public interface IOcrTextReader
{
    Task<string> GetOcrTextAsync(CancellationToken cancellationToken);
}
