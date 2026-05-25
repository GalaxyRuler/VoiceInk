using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Native.Text;

public sealed class SettingsBackedOcrTextReader(
    ISettingsStore settingsStore,
    IScreenImageCapture? capture = null,
    IOcrTextRecognizer? recognizer = null,
    int maxCharacters = 4_000) : IOcrTextReader
{
    private readonly IScreenImageCapture capture = capture ?? new WindowsDesktopScreenImageCapture();
    private readonly IOcrTextRecognizer recognizer = recognizer ?? new WindowsMediaOcrTextRecognizer();

    public async Task<string> GetOcrTextAsync(CancellationToken cancellationToken)
    {
        var settings = await settingsStore.LoadAsync(cancellationToken);
        var reader = new WindowsScreenOcrTextReader(
            capture,
            recognizer,
            maxCharacters,
            BuildRegion(settings));
        return await reader.GetOcrTextAsync(cancellationToken);
    }

    private static ScreenCaptureRegion? BuildRegion(AppSettings settings) =>
        settings.UseOcrCaptureRegion
            ? new ScreenCaptureRegion(
                settings.OcrCaptureRegionLeft,
                settings.OcrCaptureRegionTop,
                settings.OcrCaptureRegionWidth,
                settings.OcrCaptureRegionHeight)
            : null;
}
