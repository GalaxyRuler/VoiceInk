namespace VoiceInk.Windows.Native.Text;

public sealed class EmptyOcrTextReader : IOcrTextReader
{
    public Task<string> GetOcrTextAsync(CancellationToken cancellationToken) =>
        Task.FromResult(string.Empty);
}
