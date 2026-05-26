using VoiceInk.Windows.Core.PowerMode;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.PowerMode;

public sealed class PowerModeAutoSendKeyPresenterTests
{
    [Fact]
    public void Choices_MatchMacOrderWithWindowsControlEnterAdaptation()
    {
        Assert.Equal(
            [
                PowerModeAutoSendKey.None,
                PowerModeAutoSendKey.Enter,
                PowerModeAutoSendKey.ShiftEnter,
                PowerModeAutoSendKey.CommandEnter
            ],
            PowerModeAutoSendKeyPresenter.Choices.Select(choice => choice.Key).ToArray());
        Assert.Equal("None", PowerModeAutoSendKeyPresenter.Choices[0].DisplayName);
        Assert.Equal("Return", PowerModeAutoSendKeyPresenter.Choices[1].DisplayName);
        Assert.Equal("Shift + Return", PowerModeAutoSendKeyPresenter.Choices[2].DisplayName);
        Assert.Equal("Ctrl + Return", PowerModeAutoSendKeyPresenter.Choices[3].DisplayName);
    }

    [Theory]
    [InlineData(PowerModeAutoSendKey.None, 0)]
    [InlineData(PowerModeAutoSendKey.Enter, 1)]
    [InlineData(PowerModeAutoSendKey.ShiftEnter, 2)]
    [InlineData(PowerModeAutoSendKey.CommandEnter, 3)]
    public void SelectedIndexFor_MapsKnownKeys(PowerModeAutoSendKey key, int expectedIndex)
    {
        Assert.Equal(expectedIndex, PowerModeAutoSendKeyPresenter.SelectedIndexFor(key));
    }
}
