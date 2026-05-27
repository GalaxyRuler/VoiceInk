namespace VoiceInk.Windows.Native.Text;

public sealed class WindowsScreenOcrTextReader : IOcrTextReader
{
    private const int DefaultMaxCharacters = 4_000;

    private readonly IScreenImageCapture capture;
    private readonly IOcrTextRecognizer recognizer;
    private readonly int maxCharacters;
    private readonly ScreenCaptureRegion? region;

    public WindowsScreenOcrTextReader()
        : this(new WindowsDesktopScreenImageCapture(), new WindowsMediaOcrTextRecognizer())
    {
    }

    public WindowsScreenOcrTextReader(
        IScreenImageCapture capture,
        IOcrTextRecognizer recognizer,
        int maxCharacters = DefaultMaxCharacters,
        ScreenCaptureRegion? region = null)
    {
        this.capture = capture;
        this.recognizer = recognizer;
        this.maxCharacters = Math.Max(0, maxCharacters);
        this.region = region;
    }

    public async Task<string> GetOcrTextAsync(CancellationToken cancellationToken)
    {
        if (region?.IsEmpty == true)
        {
            return string.Empty;
        }

        var imagePngBytes = await capture.CapturePngAsync(region, cancellationToken);
        if (imagePngBytes.Length == 0)
        {
            return string.Empty;
        }

        var text = (await recognizer.RecognizeTextAsync(imagePngBytes, cancellationToken)).Trim();
        if (text.Length == 0)
        {
            return "No text detected via OCR";
        }

        return text.Length <= maxCharacters
            ? text
            : text[..maxCharacters];
    }
}
