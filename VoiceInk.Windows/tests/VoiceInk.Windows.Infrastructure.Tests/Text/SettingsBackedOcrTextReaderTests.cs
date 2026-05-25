using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Native.Text;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Text;

public sealed class SettingsBackedOcrTextReaderTests
{
    [Fact]
    public async Task GetOcrTextAsync_UsesFullScreenWhenRegionIsDisabled()
    {
        var capture = new FakeScreenImageCapture([1]);
        var reader = new SettingsBackedOcrTextReader(
            new FakeSettingsStore(new AppSettings
            {
                UseOcrCaptureRegion = false,
                OcrCaptureRegionWidth = 400,
                OcrCaptureRegionHeight = 200
            }),
            capture,
            new FakeOcrTextRecognizer("Full screen text"));

        var text = await reader.GetOcrTextAsync(CancellationToken.None);

        Assert.Equal("Full screen text", text);
        Assert.Null(capture.LastRegion);
    }

    [Fact]
    public async Task GetOcrTextAsync_UsesSavedRegionWhenRegionIsEnabled()
    {
        var capture = new FakeScreenImageCapture([1]);
        var reader = new SettingsBackedOcrTextReader(
            new FakeSettingsStore(new AppSettings
            {
                UseOcrCaptureRegion = true,
                OcrCaptureRegionLeft = 10,
                OcrCaptureRegionTop = 20,
                OcrCaptureRegionWidth = 640,
                OcrCaptureRegionHeight = 360
            }),
            capture,
            new FakeOcrTextRecognizer("Region text"));

        var text = await reader.GetOcrTextAsync(CancellationToken.None);

        Assert.Equal("Region text", text);
        Assert.Equal(new ScreenCaptureRegion(10, 20, 640, 360), capture.LastRegion);
    }

    [Fact]
    public async Task GetOcrTextAsync_EmptySavedRegionSkipsCaptureAndRecognition()
    {
        var capture = new FakeScreenImageCapture([1]);
        var recognizer = new FakeOcrTextRecognizer("ignored");
        var reader = new SettingsBackedOcrTextReader(
            new FakeSettingsStore(new AppSettings
            {
                UseOcrCaptureRegion = true,
                OcrCaptureRegionLeft = 10,
                OcrCaptureRegionTop = 20,
                OcrCaptureRegionWidth = 0,
                OcrCaptureRegionHeight = 360
            }),
            capture,
            recognizer);

        var text = await reader.GetOcrTextAsync(CancellationToken.None);

        Assert.Equal(string.Empty, text);
        Assert.Equal(0, capture.CallCount);
        Assert.Equal(0, recognizer.CallCount);
    }

    private sealed class FakeSettingsStore(AppSettings settings) : ISettingsStore
    {
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(settings);

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class FakeScreenImageCapture(byte[] imagePngBytes) : IScreenImageCapture
    {
        public int CallCount { get; private set; }
        public ScreenCaptureRegion? LastRegion { get; private set; }

        public Task<byte[]> CapturePngAsync(ScreenCaptureRegion? region, CancellationToken cancellationToken)
        {
            CallCount++;
            LastRegion = region;
            return Task.FromResult(imagePngBytes);
        }
    }

    private sealed class FakeOcrTextRecognizer(string text) : IOcrTextRecognizer
    {
        public int CallCount { get; private set; }

        public Task<string> RecognizeTextAsync(byte[] imagePngBytes, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(text);
        }
    }
}
