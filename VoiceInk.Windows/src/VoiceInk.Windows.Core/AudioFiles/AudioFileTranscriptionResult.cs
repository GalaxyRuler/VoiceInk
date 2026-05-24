using VoiceInk.Windows.Core.History;

namespace VoiceInk.Windows.Core.AudioFiles;

public sealed record AudioFileTranscriptionResult(
    bool Success,
    string Message,
    TranscriptionHistoryItem? Item = null);
