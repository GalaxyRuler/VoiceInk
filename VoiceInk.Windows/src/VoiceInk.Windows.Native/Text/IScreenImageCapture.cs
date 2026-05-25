namespace VoiceInk.Windows.Native.Text;

public interface IScreenImageCapture
{
    Task<byte[]> CapturePngAsync(ScreenCaptureRegion? region, CancellationToken cancellationToken);
}
