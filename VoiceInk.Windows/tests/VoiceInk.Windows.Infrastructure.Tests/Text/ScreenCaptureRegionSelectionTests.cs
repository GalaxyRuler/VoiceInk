using VoiceInk.Windows.Native.Text;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Text;

public sealed class ScreenCaptureRegionSelectionTests
{
    [Fact]
    public void FromDrag_NormalizesReverseDragAndAddsDisplayOrigin()
    {
        var region = ScreenCaptureRegionSelection.FromDrag(
            startX: 320,
            startY: 240,
            endX: 120,
            endY: 80,
            originLeft: -1920,
            originTop: 40);

        Assert.Equal(new ScreenCaptureRegion(-1800, 120, 200, 160), region);
    }

    [Fact]
    public void FromDrag_AppliesRasterizationScale()
    {
        var region = ScreenCaptureRegionSelection.FromDrag(
            startX: 10,
            startY: 20,
            endX: 110,
            endY: 70,
            originLeft: 200,
            originTop: 300,
            rasterizationScale: 1.5);

        Assert.Equal(new ScreenCaptureRegion(215, 330, 150, 75), region);
    }

    [Fact]
    public void FromDrag_ReturnsNullForTinySelection()
    {
        var region = ScreenCaptureRegionSelection.FromDrag(
            startX: 10,
            startY: 20,
            endX: 16,
            endY: 27,
            originLeft: 0,
            originTop: 0,
            minimumSize: 8);

        Assert.Null(region);
    }

    [Fact]
    public void FromDrag_RejectsInvalidRasterizationScale()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ScreenCaptureRegionSelection.FromDrag(
                startX: 0,
                startY: 0,
                endX: 100,
                endY: 100,
                originLeft: 0,
                originTop: 0,
                rasterizationScale: 0));
    }
}
