namespace VoiceInk.Windows.Core.Transcription;

public static class TranscriptionProviderPresetCatalog
{
    public static TranscriptionProviderPreset Custom { get; } = new(
        "custom",
        "Custom OpenAI-compatible",
        string.Empty,
        string.Empty,
        []);

    public static TranscriptionProviderPreset Groq { get; } = new(
        "groq",
        "Groq",
        "https://api.groq.com/openai/v1/audio/transcriptions",
        "whisper-large-v3-turbo",
        [
            "whisper-large-v3-turbo",
            "whisper-large-v3"
        ]);

    public static IReadOnlyList<TranscriptionProviderPreset> All { get; } =
    [
        Custom,
        Groq
    ];

    public static TranscriptionProviderPreset Resolve(string? id) =>
        All.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase))
        ?? Custom;
}
