using System.Buffers.Binary;

namespace VoiceInk.Windows.Core.Audio;

public static class AudioLevelMeter
{
    private const double Pcm16MaxMagnitude = 32768.0;

    public static AudioInputLevel CalculatePcm16Peak(byte[] buffer, int bytesRecorded)
    {
        if (buffer.Length == 0 || bytesRecorded <= 1)
        {
            return AudioInputLevel.Silent;
        }

        var bytesToRead = Math.Min(buffer.Length, bytesRecorded);
        bytesToRead -= bytesToRead % 2;
        var peak = 0;

        for (var i = 0; i < bytesToRead; i += 2)
        {
            var sample = BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(i, 2));
            var magnitude = sample == short.MinValue ? 32768 : Math.Abs(sample);
            if (magnitude > peak)
            {
                peak = magnitude;
            }
        }

        return new AudioInputLevel(peak / Pcm16MaxMagnitude);
    }
}
