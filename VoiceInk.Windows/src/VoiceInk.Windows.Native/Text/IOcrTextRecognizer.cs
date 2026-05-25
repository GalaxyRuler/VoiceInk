namespace VoiceInk.Windows.Native.Text;

public interface IOcrTextRecognizer
{
    Task<string> RecognizeTextAsync(byte[] imagePngBytes, CancellationToken cancellationToken);
}
