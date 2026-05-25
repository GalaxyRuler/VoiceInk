namespace VoiceInk.Windows.Native.Text;

public interface IScreenImageCapture
{
    Task<byte[]> CapturePngAsync(CancellationToken cancellationToken);
}
