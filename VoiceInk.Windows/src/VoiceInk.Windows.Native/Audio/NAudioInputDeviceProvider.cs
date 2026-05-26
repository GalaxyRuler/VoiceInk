using NAudio.CoreAudioApi;
using NAudio.Wave;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Native.Audio;

public sealed class NAudioInputDeviceProvider : IAudioInputDeviceProvider
{
    public Task<IReadOnlyList<AudioInputDevice>> ListInputDevicesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var endpointIds = CaptureEndpointIdsByFriendlyName();
        var usedEndpointIds = new HashSet<string>(StringComparer.Ordinal);
        var devices = new List<AudioInputDevice>();
        for (var deviceNumber = 0; deviceNumber < WaveIn.DeviceCount; deviceNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var capabilities = WaveIn.GetCapabilities(deviceNumber);
            var endpointId = EndpointIdForWaveInDevice(capabilities.ProductName, endpointIds, usedEndpointIds);
            devices.Add(new AudioInputDevice(
                deviceNumber,
                capabilities.ProductName,
                capabilities.Channels,
                endpointId));
        }

        return Task.FromResult<IReadOnlyList<AudioInputDevice>>(devices);
    }

    private static Dictionary<string, Queue<string>> CaptureEndpointIdsByFriendlyName()
    {
        var endpoints = new Dictionary<string, Queue<string>>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            foreach (var endpoint in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                var names = new[]
                {
                    endpoint.FriendlyName,
                    endpoint.DeviceFriendlyName
                };
                foreach (var name in names.Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var key = name.Trim();
                    if (!endpoints.TryGetValue(key, out var ids))
                    {
                        ids = new Queue<string>();
                        endpoints[key] = ids;
                    }

                    ids.Enqueue(endpoint.ID);
                }
            }
        }
        catch
        {
            // WaveIn device numbers remain usable when Core Audio endpoint enumeration is unavailable.
        }

        return endpoints;
    }

    private static string EndpointIdForWaveInDevice(
        string productName,
        Dictionary<string, Queue<string>> endpointIds,
        HashSet<string> usedEndpointIds)
    {
        var product = productName.Trim();
        var exactMatch = DequeueUnused(product, endpointIds, usedEndpointIds);
        if (!string.IsNullOrWhiteSpace(exactMatch))
        {
            return exactMatch;
        }

        foreach (var key in endpointIds.Keys)
        {
            if (key.Contains(product, StringComparison.OrdinalIgnoreCase)
                || product.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                var fuzzyMatch = DequeueUnused(key, endpointIds, usedEndpointIds);
                if (!string.IsNullOrWhiteSpace(fuzzyMatch))
                {
                    return fuzzyMatch;
                }
            }
        }

        return string.Empty;
    }

    private static string DequeueUnused(
        string key,
        Dictionary<string, Queue<string>> endpointIds,
        HashSet<string> usedEndpointIds)
    {
        if (!endpointIds.TryGetValue(key, out var ids))
        {
            return string.Empty;
        }

        while (ids.Count > 0)
        {
            var id = ids.Dequeue();
            if (usedEndpointIds.Add(id))
            {
                return id;
            }
        }

        return string.Empty;
    }
}
