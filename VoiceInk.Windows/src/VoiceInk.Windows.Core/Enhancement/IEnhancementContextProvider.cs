namespace VoiceInk.Windows.Core.Enhancement;

public interface IEnhancementContextProvider
{
    Task<EnhancementContext> GetContextAsync(
        EnhancementContextRequest request,
        CancellationToken cancellationToken);
}
