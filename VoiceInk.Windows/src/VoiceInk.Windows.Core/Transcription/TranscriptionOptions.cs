using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Transcription;

public sealed record TranscriptionOptions(
    string ModelPath,
    string Language = "auto",
    string Prompt = "",
    TranscriptionProviderKind Provider = TranscriptionProviderKind.LocalWhisper,
    string CloudEndpoint = "",
    string CloudModel = "");
