using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace VoiceInk.Windows.Native.Audio;

public sealed class NAudioEndpointNotificationRegistrar : IAudioEndpointNotificationRegistrar
{
    private readonly MMDeviceEnumerator enumerator = new();

    public void Register(IMMNotificationClient client) =>
        enumerator.RegisterEndpointNotificationCallback(client);

    public void Unregister(IMMNotificationClient client) =>
        enumerator.UnregisterEndpointNotificationCallback(client);

    public void Dispose() => enumerator.Dispose();
}
