using VoiceInk.Windows.Core.History;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.History;

public sealed class HistoryWaveformPeakExtractorTests
{
    [Fact]
    public void ExtractPeaks_ReturnsEmptyForInvalidBytes()
    {
        Assert.Empty(HistoryWaveformPeakExtractor.ExtractPeaks([1, 2, 3], targetPeakCount: 16));
    }

    [Fact]
    public void ExtractPeaks_ReturnsZerosForSilentPcm16()
    {
        var wav = CreatePcm16Wave(channelCount: 1, [0, 0, 0, 0]);

        var peaks = HistoryWaveformPeakExtractor.ExtractPeaks(wav, targetPeakCount: 4);

        Assert.Equal([0.0, 0.0, 0.0, 0.0], peaks);
    }

    [Fact]
    public void ExtractPeaks_ReturnsMonoPeakBuckets()
    {
        var wav = CreatePcm16Wave(channelCount: 1, [0, 8192, -16384, 32767]);

        var peaks = HistoryWaveformPeakExtractor.ExtractPeaks(wav, targetPeakCount: 2);

        Assert.Collection(
            peaks,
            first => Assert.InRange(first, 0.24, 0.26),
            second => Assert.InRange(second, 0.99, 1.0));
    }

    [Fact]
    public void ExtractPeaks_UsesLargestChannelPeakForStereoFrames()
    {
        var wav = CreatePcm16Wave(channelCount: 2, [0, 16384, 8192, -32768]);

        var peaks = HistoryWaveformPeakExtractor.ExtractPeaks(wav, targetPeakCount: 2);

        Assert.Collection(
            peaks,
            first => Assert.InRange(first, 0.49, 0.51),
            second => Assert.Equal(1.0, second));
    }

    [Fact]
    public void ExtractPeaks_DoesNotReturnMorePeaksThanFrames()
    {
        var wav = CreatePcm16Wave(channelCount: 1, [0, 32767]);

        var peaks = HistoryWaveformPeakExtractor.ExtractPeaks(wav, targetPeakCount: 8);

        Assert.Equal(2, peaks.Count);
    }

    private static byte[] CreatePcm16Wave(int channelCount, short[] samples)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        var dataByteCount = samples.Length * sizeof(short);
        var blockAlign = channelCount * sizeof(short);
        writer.Write("RIFF"u8);
        writer.Write(36 + dataByteCount);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)channelCount);
        writer.Write(16000);
        writer.Write(16000 * blockAlign);
        writer.Write((short)blockAlign);
        writer.Write((short)16);
        writer.Write("data"u8);
        writer.Write(dataByteCount);
        foreach (var sample in samples)
        {
            writer.Write(sample);
        }

        return stream.ToArray();
    }
}
