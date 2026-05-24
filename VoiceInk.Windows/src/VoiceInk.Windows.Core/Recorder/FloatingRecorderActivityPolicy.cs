using VoiceInk.Windows.Core.Dictation;

namespace VoiceInk.Windows.Core.Recorder;

public static class FloatingRecorderActivityPolicy
{
    public static bool ShouldShowForActivity(
        DictationState state,
        bool isStarting,
        bool isStopping,
        bool isCanceling) =>
        state is DictationState.Recording or DictationState.Transcribing or DictationState.Inserting
        || isStarting
        || isStopping
        || isCanceling;
}
