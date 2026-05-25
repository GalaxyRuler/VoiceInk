namespace VoiceInk.Windows.Core.Enhancement;

public sealed class EmptyEnhancementContextProvider : IEnhancementContextProvider
{
    public Task<EnhancementContext> GetContextAsync(CancellationToken cancellationToken) =>
        Task.FromResult(EnhancementContext.Empty);
}
