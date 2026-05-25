using NAudio.CoreAudioApi;
using VoiceInk.Windows.Core.Recording;

namespace VoiceInk.Windows.Native.Recording;

public sealed class WindowsSystemAudioFeedback : ISystemAudioFeedback, IDisposable
{
    private readonly object gate = new();
    private MMDevice? activeDevice;
    private bool didMuteAudio;
    private bool wasMutedBeforeRecording;

    public Task MuteAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            lock (gate)
            {
                ResetSession(disposeDevice: true);
                using var enumerator = new MMDeviceEnumerator();
                activeDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                wasMutedBeforeRecording = activeDevice.AudioEndpointVolume.Mute;
                if (!wasMutedBeforeRecording)
                {
                    activeDevice.AudioEndpointVolume.Mute = true;
                    didMuteAudio = true;
                }
            }
        }
        catch
        {
            lock (gate)
            {
                ResetSession(disposeDevice: true);
            }
        }

        return Task.CompletedTask;
    }

    public async Task RestoreAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        if (delay > TimeSpan.Zero)
        {
            try
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
        }

        try
        {
            lock (gate)
            {
                if (activeDevice is not null && didMuteAudio && !wasMutedBeforeRecording)
                {
                    activeDevice.AudioEndpointVolume.Mute = false;
                }

                ResetSession(disposeDevice: true);
            }
        }
        catch
        {
            lock (gate)
            {
                ResetSession(disposeDevice: true);
            }
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            ResetSession(disposeDevice: true);
        }
    }

    private void ResetSession(bool disposeDevice)
    {
        if (disposeDevice)
        {
            activeDevice?.Dispose();
        }

        activeDevice = null;
        didMuteAudio = false;
        wasMutedBeforeRecording = false;
    }
}
