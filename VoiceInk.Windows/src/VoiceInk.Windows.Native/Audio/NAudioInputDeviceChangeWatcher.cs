using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace VoiceInk.Windows.Native.Audio;

public sealed class NAudioInputDeviceChangeWatcher : IMMNotificationClient, IDisposable
{
    private static readonly TimeSpan DefaultDebounceInterval = TimeSpan.FromMilliseconds(500);

    private readonly IAudioEndpointNotificationRegistrar registrar;
    private readonly TimeSpan debounceInterval;
    private readonly object gate = new();
    private System.Threading.Timer? debounceTimer;
    private bool disposed;

    public event EventHandler? DevicesChanged;

    public NAudioInputDeviceChangeWatcher()
        : this(new NAudioEndpointNotificationRegistrar(), DefaultDebounceInterval)
    {
    }

    public NAudioInputDeviceChangeWatcher(
        IAudioEndpointNotificationRegistrar registrar,
        TimeSpan debounceInterval)
    {
        this.registrar = registrar;
        this.debounceInterval = debounceInterval <= TimeSpan.Zero
            ? TimeSpan.Zero
            : debounceInterval;
        registrar.Register(this);
    }

    public void OnDeviceStateChanged(string deviceId, DeviceState newState) => ScheduleNotification();

    public void OnDeviceAdded(string pwstrDeviceId) => ScheduleNotification();

    public void OnDeviceRemoved(string deviceId) => ScheduleNotification();

    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
    {
        if (flow is DataFlow.Capture or DataFlow.All)
        {
            ScheduleNotification();
        }
    }

    public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) => ScheduleNotification();

    public void Dispose()
    {
        lock (gate)
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            debounceTimer?.Dispose();
            debounceTimer = null;
            registrar.Unregister(this);
            registrar.Dispose();
        }
    }

    private void ScheduleNotification()
    {
        lock (gate)
        {
            if (disposed)
            {
                return;
            }

            debounceTimer ??= new System.Threading.Timer(
                _ => RaiseDevicesChanged(),
                null,
                Timeout.InfiniteTimeSpan,
                Timeout.InfiniteTimeSpan);
            debounceTimer.Change(debounceInterval, Timeout.InfiniteTimeSpan);
        }
    }

    private void RaiseDevicesChanged()
    {
        lock (gate)
        {
            if (disposed)
            {
                return;
            }
        }

        DevicesChanged?.Invoke(this, EventArgs.Empty);
    }
}
