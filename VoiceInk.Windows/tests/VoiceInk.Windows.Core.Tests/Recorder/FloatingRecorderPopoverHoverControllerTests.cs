using VoiceInk.Windows.Core.Recorder;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Recorder;

public sealed class FloatingRecorderPopoverHoverControllerTests
{
    [Fact]
    public void ButtonEntered_OpensAndCancelsPendingDismissal()
    {
        var controller = new FloatingRecorderPopoverHoverController();

        Assert.Equal(FloatingRecorderPopoverHoverAction.ScheduleDismissal, controller.ButtonExited());
        Assert.True(controller.IsDismissalScheduled);

        var action = controller.ButtonEntered();

        Assert.Equal(FloatingRecorderPopoverHoverAction.Open, action);
        Assert.True(controller.IsButtonHovered);
        Assert.False(controller.IsDismissalScheduled);
    }

    [Fact]
    public void ButtonExited_SchedulesDismissalWhenPanelIsNotHovered()
    {
        var controller = new FloatingRecorderPopoverHoverController();
        controller.ButtonEntered();

        var action = controller.ButtonExited();

        Assert.Equal(FloatingRecorderPopoverHoverAction.ScheduleDismissal, action);
        Assert.False(controller.IsButtonHovered);
        Assert.True(controller.IsDismissalScheduled);
    }

    [Fact]
    public void PanelEntered_CancelsDismissalAndKeepsPopoverOpen()
    {
        var controller = new FloatingRecorderPopoverHoverController();
        controller.ButtonEntered();
        controller.ButtonExited();

        var action = controller.PanelEntered();

        Assert.Equal(FloatingRecorderPopoverHoverAction.CancelDismissal, action);
        Assert.True(controller.IsPanelHovered);
        Assert.False(controller.IsDismissalScheduled);
    }

    [Fact]
    public void PanelExited_SchedulesDismissalWhenButtonIsNotHovered()
    {
        var controller = new FloatingRecorderPopoverHoverController();
        controller.PanelEntered();

        var action = controller.PanelExited();

        Assert.Equal(FloatingRecorderPopoverHoverAction.ScheduleDismissal, action);
        Assert.False(controller.IsPanelHovered);
        Assert.True(controller.IsDismissalScheduled);
    }

    [Fact]
    public void DismissalTimerElapsed_ClosesOnlyWhenNothingIsHovered()
    {
        var controller = new FloatingRecorderPopoverHoverController();
        controller.ButtonEntered();
        controller.PanelEntered();
        controller.ButtonExited();

        Assert.Equal(FloatingRecorderPopoverHoverAction.CancelDismissal, controller.DismissalTimerElapsed());
        Assert.False(controller.IsDismissalScheduled);

        controller.PanelExited();
        var action = controller.DismissalTimerElapsed();

        Assert.Equal(FloatingRecorderPopoverHoverAction.Close, action);
        Assert.False(controller.IsDismissalScheduled);
    }

    [Fact]
    public void Reset_ClearsHoverStateAndDismissal()
    {
        var controller = new FloatingRecorderPopoverHoverController();
        controller.ButtonEntered();
        controller.PanelEntered();
        controller.PanelExited();

        controller.Reset();

        Assert.False(controller.IsButtonHovered);
        Assert.False(controller.IsPanelHovered);
        Assert.False(controller.IsDismissalScheduled);
    }
}
