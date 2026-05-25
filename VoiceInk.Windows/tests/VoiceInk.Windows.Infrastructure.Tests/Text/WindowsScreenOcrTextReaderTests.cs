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
    public async Task GetOcrTextAsync_CapsRecognizedText()
    {
        var capture = new FakeScreenImageCapture([1]);
        var recognizer = new FakeOcrTextRecognizer("abcdefghij");
        var reader = new WindowsScreenOcrTextReader(capture, recognizer, maxCharacters: 4);

        var text = await reader.GetOcrTextAsync(CancellationToken.None);

        Assert.Equal("abcd", text);
    }

    private sealed class FakeScreenImageCapture(byte[] imagePngBytes, List<string>? calls = null) : IScreenImageCapture
    {
        public int CallCount { get; private set; }

        public Task<byte[]> CapturePngAsync(CancellationToken cancellationToken)
        {
            CallCount++;
            calls?.Add("capture");
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
