using NAudio.CoreAudioApi.Interfaces;

namespace VoiceInk.Windows.Native.Audio;

public interface IAudioEndpointNotificationRegistrar : IDisposable
{
    void Register(IMMNotificationClient client);

    void Unregister(IMMNotificationClient client);
}
