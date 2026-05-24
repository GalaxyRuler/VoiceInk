namespace VoiceInk.Windows.Core.Transcription;

public sealed record TranscriptionProviderPreset(
    string Id,
    string DisplayName,
    string Endpoint,
    string DefaultModel,
    IReadOnlyList<string> ModelIds);
