namespace VoiceInk.Windows.Core.Transcription;

public sealed record TranscriptionResult(
    string Text,
    TimeSpan Duration,
    string ProviderName);
