using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using VoiceInk.Windows.Native.Audio;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Audio;

public sealed class NAudioInputDeviceChangeWatcherTests
{
    [Fact]
    public void Constructor_RegistersNotificationCallbackAndDisposeUnregisters()
    {
        var registrar = new FakeEndpointNotificationRegistrar();
        using var watcher = new NAudioInputDeviceChangeWatcher(
            registrar,
            TimeSpan.FromMilliseconds(10));

        Assert.NotNull(registrar.RegisteredClient);

        watcher.Dispose();

        Assert.Same(registrar.RegisteredClient, registrar.UnregisteredClient);
    }

    [Fact]
    public async Task OnDefaultDeviceChanged_ForCaptureDevice_RaisesDevicesChanged()
    {
        var registrar = new FakeEndpointNotificationRegistrar();
        using var watcher = new NAudioInputDeviceChangeWatcher(
            registrar,
            TimeSpan.FromMilliseconds(5));
        var raised = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        watcher.DevicesChanged += (_, _) => raised.TrySetResult();

        registrar.RegisteredClient!.OnDefaultDeviceChanged(DataFlow.Capture, Role.Console, "capture-id");

        await raised.Task.WaitAsync(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task OnDefaultDeviceChanged_ForRenderDevice_DoesNotRaiseDevicesChanged()
    {
        var registrar = new FakeEndpointNotificationRegistrar();
        using var watcher = new NAudioInputDeviceChangeWatcher(
            registrar,
            TimeSpan.FromMilliseconds(5));
        var callCount = 0;
        watcher.DevicesChanged += (_, _) => callCount++;

        registrar.RegisteredClient!.OnDefaultDeviceChanged(DataFlow.Render, Role.Console, "render-id");
        await Task.Delay(50);

        Assert.Equal(0, callCount);
    }

    [Fact]
    public async Task DeviceNotifications_AreDebounced()
    {
        var registrar = new FakeEndpointNotificationRegistrar();
        using var watcher = new NAudioInputDeviceChangeWatcher(
            registrar,
            TimeSpan.FromMilliseconds(20));
        var callCount = 0;
        var raised = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        watcher.DevicesChanged += (_, _) =>
        {
            callCount++;
            raised.TrySetResult();
        };

        registrar.RegisteredClient!.OnDeviceAdded("device-1");
        registrar.RegisteredClient.OnDeviceRemoved("device-1");
        registrar.RegisteredClient.OnDeviceStateChanged("device-2", DeviceState.Active);

        await raised.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await Task.Delay(80);

        Assert.Equal(1, callCount);
    }

    private sealed class FakeEndpointNotificationRegistrar : IAudioEndpointNotificationRegistrar
    {
        public IMMNotificationClient? RegisteredClient { get; private set; }
        public IMMNotificationClient? UnregisteredClient { get; private set; }

        public void Register(IMMNotificationClient client) => RegisteredClient = client;

        public void Unregister(IMMNotificationClient client) => UnregisteredClient = client;

        public void Dispose()
        {
        }
    }
}
