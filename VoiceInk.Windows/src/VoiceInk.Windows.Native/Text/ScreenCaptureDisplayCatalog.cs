using System.Windows.Forms;

namespace VoiceInk.Windows.Native.Text;

public sealed record ScreenCaptureDisplaySource(
    string DeviceName,
    int Left,
    int Top,
    int Width,
    int Height,
    bool IsPrimary);

public sealed record ScreenCaptureDisplay(
    string Id,
    string Name,
    int Left,
    int Top,
    int Width,
    int Height,
    bool IsPrimary)
{
    public string DisplayName =>
        $"{(IsPrimary ? "Primary display" : Name)} - {Width} x {Height} at ({Left}, {Top})";

    public ScreenCaptureRegion BoundsRegion => new(Left, Top, Width, Height);
}

public static class ScreenCaptureDisplayCatalog
{
    public static IReadOnlyList<ScreenCaptureDisplay> FromWindowsScreens() =>
        FromSources(Screen.AllScreens.Select(screen => new ScreenCaptureDisplaySource(
            screen.DeviceName,
            screen.Bounds.Left,
            screen.Bounds.Top,
            screen.Bounds.Width,
            screen.Bounds.Height,
            screen.Primary)));

    public static IReadOnlyList<ScreenCaptureDisplay> FromSources(
        IEnumerable<ScreenCaptureDisplaySource> sources) =>
        sources
            .Where(source => source.Width > 0 && source.Height > 0)
            .Select(source => new ScreenCaptureDisplay(
                source.DeviceName,
                DisplayNameFromDeviceName(source.DeviceName),
                source.Left,
                source.Top,
                source.Width,
                source.Height,
                source.IsPrimary))
            .OrderBy(display => display.IsPrimary ? 0 : 1)
            .ThenBy(display => display.Left)
            .ThenBy(display => display.Top)
            .ThenBy(display => display.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string DisplayNameFromDeviceName(string deviceName)
    {
        var trimmed = deviceName.Trim();
        const string devicePrefix = @"\\.\";
        return trimmed.StartsWith(devicePrefix, StringComparison.Ordinal)
            ? trimmed[devicePrefix.Length..]
            : trimmed;
    }
}
