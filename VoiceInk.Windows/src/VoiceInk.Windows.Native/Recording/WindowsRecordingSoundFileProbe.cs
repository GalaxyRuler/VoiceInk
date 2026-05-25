using NAudio.Wave;
using VoiceInk.Windows.Core.Recording;

namespace VoiceInk.Windows.Native.Recording;

public sealed class WindowsRecordingSoundFileProbe : IRecordingSoundFileProbe
{
    public RecordingSoundFileProbeResult Probe(string filePath)
    {
        try
        {
            using var reader = new AudioFileReader(filePath);
            return RecordingSoundFileProbeResult.Success(reader.TotalTime);
        }
        catch (Exception ex)
        {
            return RecordingSoundFileProbeResult.Failure($"Audio file cannot be read: {ex.Message}");
        }
    }
}
