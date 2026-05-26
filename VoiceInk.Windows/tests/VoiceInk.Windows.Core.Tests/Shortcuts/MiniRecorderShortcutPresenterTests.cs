using VoiceInk.Windows.Core.Shortcuts;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Shortcuts;

public sealed class MiniRecorderShortcutPresenterTests
{
    [Theory]
    [InlineData(0x31, 0)]
    [InlineData(0x39, 8)]
    [InlineData(0x30, 9)]
    public void TryCreate_ReturnsPromptSlotForControlDigit(int virtualKey, int expectedSlot)
    {
        var created = MiniRecorderShortcutPresenter.TryCreate(
            control: true,
            alt: false,
            shift: false,
            virtualKey,
            out var shortcut);

        Assert.True(created);
        Assert.Equal(MiniRecorderShortcutKind.Prompt, shortcut?.Kind);
        Assert.Equal(expectedSlot, shortcut?.SlotIndex);
    }

    [Fact]
    public void TryCreate_ReturnsPowerModeSlotForAltDigit()
    {
        var created = MiniRecorderShortcutPresenter.TryCreate(
            control: false,
            alt: true,
            shift: false,
            virtualKey: 0x32,
            out var shortcut);

        Assert.True(created);
        Assert.Equal(MiniRecorderShortcutKind.PowerMode, shortcut?.Kind);
        Assert.Equal(1, shortcut?.SlotIndex);
    }

    [Theory]
    [InlineData(true, true, false, 0x31)]
    [InlineData(true, false, true, 0x31)]
    [InlineData(false, false, false, 0x31)]
    [InlineData(true, false, false, 0x41)]
    public void TryCreate_IgnoresNonMiniRecorderShortcuts(bool control, bool alt, bool shift, int virtualKey)
    {
        Assert.False(MiniRecorderShortcutPresenter.TryCreate(control, alt, shift, virtualKey, out var shortcut));
        Assert.Null(shortcut);
    }
}
