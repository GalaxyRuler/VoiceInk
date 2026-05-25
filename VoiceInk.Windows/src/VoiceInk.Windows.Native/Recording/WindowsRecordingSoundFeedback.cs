using System.Media;
using VoiceInk.Windows.Core.Recording;

namespace VoiceInk.Windows.Native.Recording;

public sealed class WindowsRecordingSoundFeedback : IRecordingSoundFeedback
{
    public void PlayStartSound() => Play(SystemSounds.Asterisk);

    public void PlayStopSound() => Play(SystemSounds.Exclamation);

    private static void Play(SystemSound sound)
    {
        try
        {
            sound.Play();
        }
        catch
        {
            // Sound feedback should never block recording.
        }
    }
}
