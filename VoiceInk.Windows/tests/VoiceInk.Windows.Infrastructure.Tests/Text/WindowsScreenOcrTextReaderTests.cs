using VoiceInk.Windows.Native.Text;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Text;

public sealed class WindowsScreenOcrTextReaderTests
{
    [Fact]
    public async Task GetOcrTextAsync_WhenCaptureReturnsNoBytes_SkipsRecognition()
    {
        var capture = new FakeScreenImageCapture([]);
        var recognizer = new FakeOcrTextRecognizer("ignored");
        var reader = new WindowsScreenOcrTextReader(capture, recognizer);

        var text = await reader.GetOcrTextAsync(CancellationToken.None);

        Assert.Equal(string.Empty, text);
        Assert.Equal(1, capture.CallCount);
        Assert.Equal(0, recognizer.CallCount);
    }

    [Fact]
    public async Task GetOcrTextAsync_CapturesThenRecognizesText()
    {
        var calls = new List<string>();
        var capture = new FakeScreenImageCapture([1, 2, 3], calls);
        var recognizer = new FakeOcrTextRecognizer(" Dashboard total ", calls);
        var reader = new WindowsScreenOcrTextReader(capture, recognizer);

        var text = await reader.GetOcrTextAsync(CancellationToken.None);

        Assert.Equal("Dashboard total", text);
        Assert.Equal([1, 2, 3], recognizer.LastImagePngBytes);
        Assert.Equal(["capture", "recognize"], calls);
    }

    [Fact]
    public async Task GetOcrTextAsync_WhenRecognitionFindsNoText_ReturnsMacStyleEmptyOcrMessage()
    {
        var capture = new FakeScreenImageCapture([1, 2, 3]);
        var recognizer = new FakeOcrTextRecognizer("   ");
        var reader = new WindowsScreenOcrTextReader(capture, recognizer);

        var text = await reader.GetOcrTextAsync(CancellationToken.None);

        Assert.Equal("No text detected via OCR", text);
        Assert.Equal(1, recognizer.CallCount);
    }

    [Fact]
    public async Task GetOcrTextAsync_CapsRecognizedText()
    {
        var capture = new FakeScreenImageCapture([1]);
        var recognizer = new FakeOcrTextRecognizer("abcdefghij");
        var reader = new WindowsScreenOcrTextReader(capture, recognizer, maxCharacters: 4);

        var text = await reader.GetOcrTextAsync(CancellationToken.None);

        Assert.Equal("abcd", text);
    }

    [Fact]
    public async Task GetOcrTextAsync_PassesConfiguredRegionToCapture()
    {
        var capture = new FakeScreenImageCapture([1]);
        var recognizer = new FakeOcrTextRecognizer("Region text");
        var region = new ScreenCaptureRegion(10, 20, 300, 180);
        var reader = new WindowsScreenOcrTextReader(capture, recognizer, region: region);

        var text = await reader.GetOcrTextAsync(CancellationToken.None);

        Assert.Equal("Region text", text);
        Assert.Equal(region, capture.LastRegion);
    }

    [Fact]
    public async Task GetOcrTextAsync_WhenConfiguredRegionIsEmpty_SkipsRecognition()
    {
        var capture = new FakeScreenImageCapture([1]);
        var recognizer = new FakeOcrTextRecognizer("ignored");
        var reader = new WindowsScreenOcrTextReader(
            capture,
            recognizer,
            region: new ScreenCaptureRegion(10, 20, 0, 180));

        var text = await reader.GetOcrTextAsync(CancellationToken.None);

        Assert.Equal(string.Empty, text);
        Assert.Equal(0, capture.CallCount);
        Assert.Equal(0, recognizer.CallCount);
    }

    private sealed class FakeScreenImageCapture(byte[] imagePngBytes, List<string>? calls = null) : IScreenImageCapture
    {
        public int CallCount { get; private set; }
        public ScreenCaptureRegion? LastRegion { get; private set; }

        public Task<byte[]> CapturePngAsync(ScreenCaptureRegion? region, CancellationToken cancellationToken)
        {
            CallCount++;
            calls?.Add("capture");
            LastRegion = region;
            return Task.FromResult(imagePngBytes);
        }
    }

    private sealed class FakeOcrTextRecognizer(string text, List<string>? calls = null) : IOcrTextRecognizer
    {
        public int CallCount { get; private set; }
        public byte[] LastImagePngBytes { get; private set; } = [];

        public Task<string> RecognizeTextAsync(byte[] imagePngBytes, CancellationToken cancellationToken)
        {
            CallCount++;
            calls?.Add("recognize");
            LastImagePngBytes = imagePngBytes;
            return Task.FromResult(text);
        }
    }
}
