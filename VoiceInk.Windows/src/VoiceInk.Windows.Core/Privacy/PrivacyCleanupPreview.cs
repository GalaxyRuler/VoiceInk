namespace VoiceInk.Windows.Core.Privacy;

public sealed record PrivacyCleanupPreview(
    bool IsEnabled,
    int FileCount,
    long TotalBytes);
