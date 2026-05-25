namespace VoiceInk.Windows.Native.Text;

public static class ScreenCaptureRegionSelection
{
    public static ScreenCaptureRegion? FromDrag(
        double startX,
        double startY,
        double endX,
        double endY,
        int originLeft,
        int originTop,
        double rasterizationScale = 1,
        int minimumSize = 8)
    {
        if (rasterizationScale <= 0 || double.IsNaN(rasterizationScale) || double.IsInfinity(rasterizationScale))
        {
            throw new ArgumentOutOfRangeException(nameof(rasterizationScale), "Rasterization scale must be positive.");
        }

        if (minimumSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumSize), "Minimum selection size cannot be negative.");
        }

        var left = Math.Min(startX, endX);
        var top = Math.Min(startY, endY);
        var width = Math.Abs(endX - startX);
        var height = Math.Abs(endY - startY);

        if (width < minimumSize || height < minimumSize)
        {
            return null;
        }

        return new ScreenCaptureRegion(
            originLeft + ToPixel(left, rasterizationScale),
            originTop + ToPixel(top, rasterizationScale),
            ToPixel(width, rasterizationScale),
            ToPixel(height, rasterizationScale));
    }

    private static int ToPixel(double value, double scale) =>
        (int)Math.Round(value * scale, MidpointRounding.AwayFromZero);
}
