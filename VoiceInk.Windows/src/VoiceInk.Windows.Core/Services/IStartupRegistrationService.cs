using VoiceInk.Windows.Core.Startup;

namespace VoiceInk.Windows.Core.Services;

public interface IStartupRegistrationService
{
    Task<StartupRegistrationState> GetStateAsync(CancellationToken cancellationToken);

    Task SetEnabledAsync(bool isEnabled, CancellationToken cancellationToken);

    Task RepairRegistrationAsync(bool isEnabled, CancellationToken cancellationToken);

    Task RestoreStateAsync(StartupRegistrationState state, CancellationToken cancellationToken);
}
