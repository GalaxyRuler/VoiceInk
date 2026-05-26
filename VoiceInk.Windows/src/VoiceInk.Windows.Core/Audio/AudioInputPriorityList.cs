namespace VoiceInk.Windows.Core.Audio;

public static class AudioInputPriorityList
{
    public static PrioritizedAudioInputDevice[] Add(
        IReadOnlyList<PrioritizedAudioInputDevice> devices,
        string name)
    {
        if (string.IsNullOrWhiteSpace(name)
            || devices.Any(device => NamesMatch(device.Name, name)))
        {
            return Normalize(devices);
        }

        return Normalize(devices.Append(new PrioritizedAudioInputDevice(name.Trim(), devices.Count)));
    }

    public static PrioritizedAudioInputDevice[] Remove(
        IReadOnlyList<PrioritizedAudioInputDevice> devices,
        string name) =>
        Normalize(Ordered(devices).Where(device => !NamesMatch(device.Name, name)));

    public static PrioritizedAudioInputDevice[] MoveUp(
        IReadOnlyList<PrioritizedAudioInputDevice> devices,
        string name)
    {
        var ordered = Ordered(devices);
        var index = ordered.FindIndex(device => NamesMatch(device.Name, name));
        if (index <= 0)
        {
            return Normalize(ordered);
        }

        (ordered[index - 1], ordered[index]) = (ordered[index], ordered[index - 1]);
        return Normalize(ordered);
    }

    public static PrioritizedAudioInputDevice[] MoveDown(
        IReadOnlyList<PrioritizedAudioInputDevice> devices,
        string name)
    {
        var ordered = Ordered(devices);
        var index = ordered.FindIndex(device => NamesMatch(device.Name, name));
        if (index < 0 || index >= ordered.Count - 1)
        {
            return Normalize(ordered);
        }

        (ordered[index], ordered[index + 1]) = (ordered[index + 1], ordered[index]);
        return Normalize(ordered);
    }

    public static PrioritizedAudioInputDevice[] Normalize(
        IEnumerable<PrioritizedAudioInputDevice> devices) =>
        devices
            .Select((device, index) => new PrioritizedAudioInputDevice(device.Name.Trim(), index))
            .Where(device => !string.IsNullOrWhiteSpace(device.Name))
            .ToArray();

    private static List<PrioritizedAudioInputDevice> Ordered(
        IEnumerable<PrioritizedAudioInputDevice> devices) =>
        devices
            .Where(device => !string.IsNullOrWhiteSpace(device.Name))
            .OrderBy(device => device.Priority)
            .ThenBy(device => device.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static bool NamesMatch(string first, string second) =>
        string.Equals(first.Trim(), second.Trim(), StringComparison.Ordinal);
}
