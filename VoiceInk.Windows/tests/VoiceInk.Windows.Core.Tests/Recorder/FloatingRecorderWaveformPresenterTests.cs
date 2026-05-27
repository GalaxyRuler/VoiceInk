using VoiceInk.Windows.Core.Recorder;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Recorder;

public sealed class FloatingRecorderWaveformPresenterTests
{
    [Fact]
    public void Present_ReturnsMacOsAlignedBarCountAndBounds()
    {
        var bars = FloatingRecorderWaveformPresenter.Present(inputLevel: 0.7, animationStep: 3);

        Assert.Equal(15, bars.Count);
        Assert.All(bars, bar =>
        {
            Assert.InRange(bar.Height, 4, 28);
            Assert.InRange(bar.Opacity, 0.25, 1);
        });
    }

    [Fact]
    public void Present_WeightsCenterBarsHigherThanEdgesForVoiceInput()
    {
        var bars = FloatingRecorderWaveformPresenter.Present(inputLevel: 0.8, animationStep: 1);

        Assert.True(bars[7].Height > bars[0].Height);
        Assert.True(bars[7].Height > bars[14].Height);
    }

    [Fact]
    public void Present_AnimatesLowInputInsteadOfFlattening()
    {
        var first = FloatingRecorderWaveformPresenter.Present(inputLevel: 0, animationStep: 0);
        var second = FloatingRecorderWaveformPresenter.Present(inputLevel: 0, animationStep: 1);

        Assert.Contains(first, bar => bar.Height > 4);
        Assert.NotEqual(first.Select(bar => bar.Height), second.Select(bar => bar.Height));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(-1)]
    public void Present_ClampsInvalidInputToSafePulse(double inputLevel)
    {
        var bars = FloatingRecorderWaveformPresenter.Present(inputLevel, animationStep: 2);

        Assert.Equal(15, bars.Count);
        Assert.All(bars, bar => Assert.InRange(bar.Height, 4, 28));
    }
}
