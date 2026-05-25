namespace VoiceInk.Windows.Core.Enhancement;

public sealed class EmptyEnhancementContextProvider : IEnhancementContextProvider
{
    public Task<EnhancementContext> GetContextAsync(
        EnhancementContextRequest request,
        CancellationToken cancellationToken) =>
        Task.FromResult(EnhancementContext.Empty);
}
