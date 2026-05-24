using VoiceInk.Windows.Core.PowerMode;

namespace VoiceInk.Windows.Core.Services;

public interface IPowerModeTargetProvider
{
    Task<PowerModeTarget?> GetCurrentTargetAsync(CancellationToken cancellationToken);
}
