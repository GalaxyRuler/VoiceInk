using VoiceInk.Windows.Core.Shortcuts;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Shortcuts;

public sealed class GlobalShortcutSessionChangePolicyTests
{
    [Theory]
    [InlineData(0x02B1, 0x7)]
    [InlineData(0x02B1, 0x8)]
    [InlineData(0x02B1, 0xF)]
    public void ShouldResetPressedState_ForSessionBoundaryEvents(int message, int eventCode)
    {
        Assert.True(GlobalShortcutSessionChangePolicy.ShouldResetPressedState(message, eventCode));
    }

    [Theory]
    [InlineData(0x0312, 0x8)]
    [InlineData(0x02B1, 0x1)]
    [InlineData(0x02B1, 0x9)]
    public void ShouldResetPressedState_IgnoresUnrelatedMessages(int message, int eventCode)
    {
        Assert.False(GlobalShortcutSessionChangePolicy.ShouldResetPressedState(message, eventCode));
    }
}
