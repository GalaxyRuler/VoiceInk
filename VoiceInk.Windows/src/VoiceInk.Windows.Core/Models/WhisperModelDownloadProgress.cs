namespace VoiceInk.Windows.Core.Models;

public sealed record WhisperModelDownloadProgress(
    string ModelName,
    long BytesReceived,
    long? TotalBytes)
{
    public double FractionComplete =>
        TotalBytes is > 0
            ? Math.Clamp((double)BytesReceived / TotalBytes.Value, 0, 1)
            : 0;
}
