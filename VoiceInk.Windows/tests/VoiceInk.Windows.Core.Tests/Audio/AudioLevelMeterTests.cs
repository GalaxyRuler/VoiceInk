using VoiceInk.Windows.Core.Audio;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Audio;

public sealed class AudioLevelMeterTests
{
    [Fact]
    public void CalculatePcm16Peak_EmptyBuffer_ReturnsZero()
    {
        var level = AudioLevelMeter.CalculatePcm16Peak([], 0);

        Assert.Equal(0, level.Peak);
    }

    [Fact]
    public void CalculatePcm16Peak_Silence_ReturnsZero()
    {
        var level = AudioLevelMeter.CalculatePcm16Peak(Samples(0, 0, 0), 6);

        Assert.Equal(0, level.Peak);
    }

    [Fact]
    public void CalculatePcm16Peak_HalfScaleSamples_ReturnsNormalizedPeak()
    {
        var level = AudioLevelMeter.CalculatePcm16Peak(Samples(0, 16_384, -16_384), 6);

        Assert.InRange(level.Peak, 0.49, 0.51);
    }

    [Fact]
    public void CalculatePcm16Peak_FullScaleSamples_ClampsToOne()
    {
        var level = AudioLevelMeter.CalculatePcm16Peak(Samples(short.MaxValue, short.MinValue), 4);

        Assert.Equal(1, level.Peak);
    }

    [Fact]
    public void CalculatePcm16Peak_IgnoresOddTrailingByte()
    {
        var buffer = Samples(0, 16_384);
        Array.Resize(ref buffer, buffer.Length + 1);
        buffer[^1] = 0x7F;

        var level = AudioLevelMeter.CalculatePcm16Peak(buffer, buffer.Length);

        Assert.InRange(level.Peak, 0.49, 0.51);
    }

    [Fact]
    public void CalculatePcm16Peak_ReadsLittleEndianPcm16()
    {
        byte[] buffer = [0x00, 0x40, 0x00, 0xC0];

        var level = AudioLevelMeter.CalculatePcm16Peak(buffer, buffer.Length);

        Assert.InRange(level.Peak, 0.49, 0.51);
    }

    [Fact]
    public void CalculatePcm16Peak_BytesRecordedBeyondBuffer_UsesAvailableBytes()
    {
        var level = AudioLevelMeter.CalculatePcm16Peak(Samples(8_192), 1024);

        Assert.InRange(level.Peak, 0.24, 0.26);
    }

    private static byte[] Samples(params short[] samples)
    {
        var buffer = new byte[samples.Length * 2];
        for (var i = 0; i < samples.Length; i++)
        {
            var bytes = BitConverter.GetBytes(samples[i]);
            buffer[i * 2] = bytes[0];
            buffer[i * 2 + 1] = bytes[1];
        }

        return buffer;
    }
}
