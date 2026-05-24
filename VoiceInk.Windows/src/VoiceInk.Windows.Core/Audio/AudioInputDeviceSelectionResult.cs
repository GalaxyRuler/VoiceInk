namespace VoiceInk.Windows.Core.Audio;

public sealed record AudioInputDeviceSelectionResult(
    IReadOnlyList<AudioInputDeviceChoice> Choices,
    int SelectedIndex,
    string? Warning);
