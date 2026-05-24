using VoiceInk.Windows.Core.Audio;

namespace VoiceInk.Windows.Core.Services;

public interface IAudioInputDeviceProvider
{
    Task<IReadOnlyList<AudioInputDevice>> ListInputDevicesAsync(CancellationToken cancellationToken);
}
