using System.Buffers.Binary;
using System.Text;

namespace VoiceInk.Windows.Core.History;

public static class HistoryWaveformPeakExtractor
{
    private const ushort PcmFormat = 1;
    private const ushort Pcm16BitsPerSample = 16;

    public static IReadOnlyList<double> ExtractPeaks(byte[] wavBytes, int targetPeakCount)
    {
        if (targetPeakCount <= 0 || wavBytes.Length < 12)
        {
            return [];
        }

        var data = wavBytes.AsSpan();
        if (!FourCcEquals(data[..4], "RIFF")
            || !FourCcEquals(data.Slice(8, 4), "WAVE"))
        {
            return [];
        }

        ushort audioFormat = 0;
        ushort channelCount = 0;
        ushort bitsPerSample = 0;
        var dataOffset = -1;
        var dataSize = 0;

        var offset = 12;
        while (offset + 8 <= data.Length)
        {
            var chunkId = data.Slice(offset, 4);
            var chunkSize = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(offset + 4, 4));
            if (chunkSize < 0 || offset + 8 + chunkSize > data.Length)
            {
                return [];
            }

            var chunkDataOffset = offset + 8;
            if (FourCcEquals(chunkId, "fmt ") && chunkSize >= 16)
            {
                audioFormat = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(chunkDataOffset, 2));
                channelCount = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(chunkDataOffset + 2, 2));
                bitsPerSample = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(chunkDataOffset + 14, 2));
            }
            else if (FourCcEquals(chunkId, "data"))
            {
                dataOffset = chunkDataOffset;
                dataSize = chunkSize;
            }

            offset = chunkDataOffset + chunkSize + (chunkSize % 2);
        }

        if (audioFormat != PcmFormat
            || bitsPerSample != Pcm16BitsPerSample
            || channelCount == 0
            || dataOffset < 0
            || dataSize <= 0)
        {
            return [];
        }

        var bytesPerFrame = channelCount * sizeof(short);
        var frameCount = dataSize / bytesPerFrame;
        if (frameCount <= 0)
        {
            return [];
        }

        var peakCount = Math.Min(targetPeakCount, frameCount);
        var peaks = new double[peakCount];
        var sampleData = data.Slice(dataOffset, frameCount * bytesPerFrame);

        for (var frameIndex = 0; frameIndex < frameCount; frameIndex++)
        {
            var bucketIndex = (int)((long)frameIndex * peakCount / frameCount);
            var frameOffset = frameIndex * bytesPerFrame;
            var framePeak = 0;
            for (var channelIndex = 0; channelIndex < channelCount; channelIndex++)
            {
                var sampleOffset = frameOffset + channelIndex * sizeof(short);
                var sample = BinaryPrimitives.ReadInt16LittleEndian(sampleData.Slice(sampleOffset, sizeof(short)));
                var magnitude = sample == short.MinValue
                    ? 32768
                    : Math.Abs(sample);
                framePeak = Math.Max(framePeak, magnitude);
            }

            peaks[bucketIndex] = Math.Max(peaks[bucketIndex], Math.Min(1.0, framePeak / 32768.0));
        }

        return peaks;
    }

    private static bool FourCcEquals(ReadOnlySpan<byte> value, string expected) =>
        value.Length == 4 && Encoding.ASCII.GetString(value).Equals(expected, StringComparison.Ordinal);
}
