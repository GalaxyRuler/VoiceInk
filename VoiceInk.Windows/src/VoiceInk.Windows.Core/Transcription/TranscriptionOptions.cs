namespace VoiceInk.Windows.Core.Transcription;

public sealed record TranscriptionOptions(
    string ModelPath,
    string Language = "auto");
