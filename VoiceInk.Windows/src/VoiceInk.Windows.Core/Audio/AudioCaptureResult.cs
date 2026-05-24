namespace VoiceInk.Windows.Core.Audio;

public sealed record AudioCaptureResult(
    string FilePath,
    TimeSpan Duration,
    int SampleRate,
    int ChannelCount);
