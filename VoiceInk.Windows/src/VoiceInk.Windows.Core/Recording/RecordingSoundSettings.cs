using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Recording;

public sealed record RecordingSoundSettings(
    RecordingSoundPlaybackSettings Start,
    RecordingSoundPlaybackSettings Stop)
{
    public static RecordingSoundSettings From(AppSettings settings) =>
        new(
            new RecordingSoundPlaybackSettings(settings.StartSoundMode, settings.CustomStartSoundPath),
            new RecordingSoundPlaybackSettings(settings.StopSoundMode, settings.CustomStopSoundPath));
}
