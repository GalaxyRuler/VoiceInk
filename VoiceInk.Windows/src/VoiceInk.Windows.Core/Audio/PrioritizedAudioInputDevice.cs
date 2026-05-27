namespace VoiceInk.Windows.Core.Audio;

public sealed record PrioritizedAudioInputDevice(
    string Name,
    int Priority,
    string EndpointId = "")
{
    public string PriorityDisplay => $"#{Priority + 1}";

    public string AccessibleName => string.Join(
        ", ",
        new[] { PriorityDisplay, Name, EndpointAccessibleText(EndpointId) }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part.Trim()));

    private static string EndpointAccessibleText(string endpointId)
    {
        if (string.IsNullOrWhiteSpace(endpointId))
        {
            return string.Empty;
        }

        return $"Endpoint {ShortEndpointId(endpointId)}";
    }

    private static string ShortEndpointId(string endpointId)
    {
        var trimmedEndpointId = endpointId.Trim();
        return trimmedEndpointId.Length <= 18
            ? trimmedEndpointId
            : $"{trimmedEndpointId[..8]}...{trimmedEndpointId[^7..]}";
    }
}
