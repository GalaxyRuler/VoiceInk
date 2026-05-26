using VoiceInk.Windows.Core.PowerMode;

namespace VoiceInk.Windows.Core.Services;

public interface IPowerModeAutoSendService
{
    Task SendAsync(PowerModeAutoSendKey key, CancellationToken cancellationToken);
}
