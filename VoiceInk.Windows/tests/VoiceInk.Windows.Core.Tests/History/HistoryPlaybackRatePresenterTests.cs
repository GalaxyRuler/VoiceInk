using VoiceInk.Windows.Core.History;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.History;

public sealed class HistoryPlaybackRatePresenterTests
{
    [Fact]
    public void Choices_ReturnsSupportedPlaybackRates()
    {
        Assert.Equal(
            ["0.75x", "1.0x", "1.25x", "1.5x", "2.0x"],
            HistoryPlaybackRatePresenter.Choices.Select(choice => choice.Label).ToArray());
        Assert.Equal(
            [0.75, 1.0, 1.25, 1.5, 2.0],
            HistoryPlaybackRatePresenter.Choices.Select(choice => choice.Value).ToArray());
    }

    [Fact]
    public void DefaultChoice_ReturnsNormalSpeed()
    {
        Assert.Equal("1.0x", HistoryPlaybackRatePresenter.DefaultChoice.Label);
        Assert.Equal(1.0, HistoryPlaybackRatePresenter.DefaultChoice.Value);
    }

    [Theory]
    [InlineData(0.75, 0)]
    [InlineData(1.0, 1)]
    [InlineData(1.25, 2)]
    [InlineData(1.5, 3)]
    [InlineData(2.0, 4)]
    public void SelectedIndexFor_ReturnsMatchingIndex(double value, int expectedIndex)
    {
        Assert.Equal(expectedIndex, HistoryPlaybackRatePresenter.SelectedIndexFor(value));
    }

    [Fact]
    public void ChoiceAtOrDefault_ClampsInvalidIndexToDefault()
    {
        Assert.Equal(1.0, HistoryPlaybackRatePresenter.ChoiceAtOrDefault(-1).Value);
        Assert.Equal(1.0, HistoryPlaybackRatePresenter.ChoiceAtOrDefault(99).Value);
    }
}
