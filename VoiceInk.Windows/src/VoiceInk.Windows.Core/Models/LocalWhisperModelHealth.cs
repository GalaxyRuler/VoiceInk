namespace VoiceInk.Windows.Core.Models;

public enum LocalWhisperModelHealthStatus
{
    NotSelected,
    InvalidExtension,
    Missing,
    Empty,
    SuspiciouslySmall,
    InvalidHeader,
    Ready
}

public sealed record LocalWhisperModelHealth(
    LocalWhisperModelHealthStatus Status,
    string Message,
    bool CanUse);
