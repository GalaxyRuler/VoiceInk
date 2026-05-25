namespace VoiceInk.Windows.Native.Text;

public sealed record ScreenCaptureRegion(int Left, int Top, int Width, int Height)
{
    public bool IsEmpty => Width <= 0 || Height <= 0;
}
