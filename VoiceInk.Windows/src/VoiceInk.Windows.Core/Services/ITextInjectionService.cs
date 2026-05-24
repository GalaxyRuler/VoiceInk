namespace VoiceInk.Windows.Core.Services;

public interface ITextInjectionService
{
    Task InsertAsync(string text, CancellationToken cancellationToken);
}
