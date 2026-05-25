namespace VoiceInk.Windows.Core.Recorder;

public enum FloatingRecorderPopoverHoverAction
{
    None,
    Open,
    ScheduleDismissal,
    CancelDismissal,
    Close
}

public sealed class FloatingRecorderPopoverHoverController
{
    public bool IsButtonHovered { get; private set; }

    public bool IsPanelHovered { get; private set; }

    public bool IsDismissalScheduled { get; private set; }

    public FloatingRecorderPopoverHoverAction ButtonEntered()
    {
        IsButtonHovered = true;
        IsDismissalScheduled = false;
        return FloatingRecorderPopoverHoverAction.Open;
    }

    public FloatingRecorderPopoverHoverAction ButtonExited()
    {
        IsButtonHovered = false;
        return ScheduleIfNeeded();
    }

    public FloatingRecorderPopoverHoverAction PanelEntered()
    {
        IsPanelHovered = true;
        if (!IsDismissalScheduled)
        {
            return FloatingRecorderPopoverHoverAction.None;
        }

        IsDismissalScheduled = false;
        return FloatingRecorderPopoverHoverAction.CancelDismissal;
    }

    public FloatingRecorderPopoverHoverAction PanelExited()
    {
        IsPanelHovered = false;
        return ScheduleIfNeeded();
    }

    public FloatingRecorderPopoverHoverAction DismissalTimerElapsed()
    {
        IsDismissalScheduled = false;
        return IsButtonHovered || IsPanelHovered
            ? FloatingRecorderPopoverHoverAction.CancelDismissal
            : FloatingRecorderPopoverHoverAction.Close;
    }

    public void Reset()
    {
        IsButtonHovered = false;
        IsPanelHovered = false;
        IsDismissalScheduled = false;
    }

    private FloatingRecorderPopoverHoverAction ScheduleIfNeeded()
    {
        if (IsButtonHovered || IsPanelHovered)
        {
            IsDismissalScheduled = false;
            return FloatingRecorderPopoverHoverAction.CancelDismissal;
        }

        IsDismissalScheduled = true;
        return FloatingRecorderPopoverHoverAction.ScheduleDismissal;
    }
}
