using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Services;

public interface ISettingsStore
{
    Task<AppSettings> LoadAsync(CancellationToken cancellationToken);
    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken);
}
