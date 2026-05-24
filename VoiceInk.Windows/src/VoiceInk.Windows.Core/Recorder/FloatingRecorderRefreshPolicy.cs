namespace VoiceInk.Windows.Core.Recorder;

public static class FloatingRecorderRefreshPolicy
{
    public static bool ShouldRefresh(FloatingRecorderViewState state) =>
        state.IsVisible && state.ShowPulse;
}
