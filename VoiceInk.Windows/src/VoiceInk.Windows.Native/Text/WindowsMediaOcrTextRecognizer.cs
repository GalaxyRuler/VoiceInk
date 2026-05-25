using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace VoiceInk.Windows.Native.Text;

public sealed class WindowsMediaOcrTextRecognizer : IOcrTextRecognizer
{
    public async Task<string> RecognizeTextAsync(byte[] imagePngBytes, CancellationToken cancellationToken)
    {
        if (imagePngBytes.Length == 0)
        {
            return string.Empty;
        }

        var engine = OcrEngine.TryCreateFromUserProfileLanguages();
        if (engine is null)
        {
            return string.Empty;
        }

        using var stream = new InMemoryRandomAccessStream();
        using (var writer = new DataWriter(stream))
        {
            writer.WriteBytes(imagePngBytes);
            await writer.StoreAsync().AsTask(cancellationToken);
            await writer.FlushAsync().AsTask(cancellationToken);
            writer.DetachStream();
        }

        stream.Seek(0);
        var decoder = await BitmapDecoder.CreateAsync(stream).AsTask(cancellationToken);
        using var bitmap = await decoder
            .GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied)
            .AsTask(cancellationToken);
        var result = await engine.RecognizeAsync(bitmap).AsTask(cancellationToken);

        return string.Join(
            Environment.NewLine,
            result.Lines.Select(line => line.Text).Where(line => !string.IsNullOrWhiteSpace(line)));
    }
}
