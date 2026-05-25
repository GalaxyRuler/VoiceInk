namespace VoiceInk.Windows.Core.Audio;

public sealed record AudioInputDeviceSelectionResult(
    IReadOnlyList<AudioInputDeviceChoice> Choices,
    int SelectedIndex,
    string? Warning)
{
    public AudioInputDeviceChoice? SelectedChoice =>
        SelectedIndex >= 0 && SelectedIndex < Choices.Count
            ? Choices[SelectedIndex]
            : null;
}
