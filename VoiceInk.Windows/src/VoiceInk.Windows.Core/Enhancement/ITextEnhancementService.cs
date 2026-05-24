namespace VoiceInk.Windows.Core.Enhancement;

public interface ITextEnhancementService
{
    Task<TextEnhancementResult> EnhanceAsync(
        TextEnhancementRequest request,
        CancellationToken cancellationToken);
}
