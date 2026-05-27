namespace VoiceInk.Windows.Core.Audio;

public sealed record VoiceActivityResult(
    bool HasSpeech,
    TimeSpan SpeechDuration);
