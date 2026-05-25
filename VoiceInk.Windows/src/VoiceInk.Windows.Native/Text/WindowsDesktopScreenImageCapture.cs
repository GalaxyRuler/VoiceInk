using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace VoiceInk.Windows.Native.Text;

public sealed class WindowsDesktopScreenImageCapture : IScreenImageCapture
{
    public Task<byte[]> CapturePngAsync(ScreenCaptureRegion? region, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var bounds = region is null
            ? VirtualScreenBounds()
            : new Rectangle(region.Left, region.Top, region.Width, region.Height);
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return Task.FromResult(Array.Empty<byte>());
        }

        using var bitmap = new Bitmap(bounds.Width, bounds.Height);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(
                bounds.Left,
                bounds.Top,
                0,
                0,
                bounds.Size,
                CopyPixelOperation.SourceCopy);
        }

        cancellationToken.ThrowIfCancellationRequested();

        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return Task.FromResult(stream.ToArray());
    }

    private static Rectangle VirtualScreenBounds()
    {
        var left = Screen.AllScreens.Min(screen => screen.Bounds.Left);
        var top = Screen.AllScreens.Min(screen => screen.Bounds.Top);
        var right = Screen.AllScreens.Max(screen => screen.Bounds.Right);
        var bottom = Screen.AllScreens.Max(screen => screen.Bounds.Bottom);
        return Rectangle.FromLTRB(left, top, right, bottom);
    }
}
