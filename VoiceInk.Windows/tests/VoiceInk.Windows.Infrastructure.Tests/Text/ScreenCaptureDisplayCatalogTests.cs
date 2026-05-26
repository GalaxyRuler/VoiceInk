using VoiceInk.Windows.Native.Text;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Text;

public sealed class ScreenCaptureDisplayCatalogTests
{
    [Fact]
    public void FromSources_SortsPrimaryFirstAndLabelsVirtualScreenCoordinates()
    {
        var displays = ScreenCaptureDisplayCatalog.FromSources(
            [
                new ScreenCaptureDisplaySource(@"\\.\DISPLAY2", -1920, 0, 1920, 1080, false),
                new ScreenCaptureDisplaySource(@"\\.\DISPLAY1", 0, 0, 2560, 1440, true)
            ]);

        Assert.Equal(
            [
                "Primary display - 2560 x 1440 at (0, 0)",
                "DISPLAY2 - 1920 x 1080 at (-1920, 0)"
            ],
            displays.Select(display => display.DisplayName).ToArray());
        Assert.Equal(new ScreenCaptureRegion(0, 0, 2560, 1440), displays[0].BoundsRegion);
        Assert.Equal(new ScreenCaptureRegion(-1920, 0, 1920, 1080), displays[1].BoundsRegion);
    }
}
