namespace VoiceInk.Windows.Core.Recording;

public sealed record RecordingSoundPlaybackSettings
{
    public RecordingSoundPlaybackSettings(string? mode, string? customSoundPath)
    {
        Mode = RecordingSoundModeSettings.Normalize(mode);
        CustomSoundPath = customSoundPath?.Trim() ?? string.Empty;
    }

    public string Mode { get; init; }
    public string CustomSoundPath { get; init; }
    public bool UsesCustomSound => Mode == RecordingSoundModeSettings.Custom
        && !string.IsNullOrWhiteSpace(CustomSoundPath);
}
