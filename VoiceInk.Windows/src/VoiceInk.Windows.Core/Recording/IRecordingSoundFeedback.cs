namespace VoiceInk.Windows.Core.Recording;

public interface IRecordingSoundFeedback
{
    void PlayStartSound(RecordingSoundPlaybackSettings settings);
    void PlayStopSound(RecordingSoundPlaybackSettings settings);
}
