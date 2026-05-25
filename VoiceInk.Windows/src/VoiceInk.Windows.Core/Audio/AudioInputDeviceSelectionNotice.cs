namespace VoiceInk.Windows.Core.Audio;

public sealed record AudioInputDeviceSelectionNotice(
    AudioInputDeviceSelectionNoticeKind Kind,
    string Title,
    string Message,
    string ActionText);
