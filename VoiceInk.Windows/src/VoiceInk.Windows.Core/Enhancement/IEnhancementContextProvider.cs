namespace VoiceInk.Windows.Core.Enhancement;

public interface IEnhancementContextProvider
{
    Task<EnhancementContext> GetContextAsync(CancellationToken cancellationToken);
}
