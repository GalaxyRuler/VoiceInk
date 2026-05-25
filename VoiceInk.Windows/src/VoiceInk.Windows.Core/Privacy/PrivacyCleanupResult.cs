namespace VoiceInk.Windows.Core.Privacy;

public sealed record PrivacyCleanupResult(
    bool IsEnabled,
    int DeletedTranscriptionCount,
    int DeletedAudioFileCount,
    int FailedAudioFileCount,
    int ClearedAudioReferenceCount,
    long DeletedAudioBytes);
