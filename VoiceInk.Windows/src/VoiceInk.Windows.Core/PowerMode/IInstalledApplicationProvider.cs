namespace VoiceInk.Windows.Core.PowerMode;

public interface IInstalledApplicationProvider
{
    Task<IReadOnlyList<PowerModeInstalledApplicationChoice>> GetInstalledApplicationsAsync(
        CancellationToken cancellationToken);
}
