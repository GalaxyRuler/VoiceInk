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

    public static TranscriptionProviderPreset Deepgram { get; } = new(
        "deepgram",
        "Deepgram",
        "https://api.deepgram.com/v1/listen",
        "nova-3",
        [
            "nova-3",
            "nova-3-medical"
        ]);

    public static TranscriptionProviderPreset AssemblyAI { get; } = new(
        "assemblyai",
        "AssemblyAI",
        "https://streaming.assemblyai.com/v3/ws",
        "universal-3-pro",
        [
            "universal-3-pro",
            "universal-streaming"
        ]);

    public static TranscriptionProviderPreset Mistral { get; } = new(
        "mistral",
        "Mistral",
        "https://api.mistral.ai/v1/audio/transcriptions",
        "voxtral-mini-latest",
        [
            "voxtral-mini-latest"
        ]);

    public static TranscriptionProviderPreset ElevenLabs { get; } = new(
        "elevenlabs",
        "ElevenLabs",
        "https://api.elevenlabs.io/v1/speech-to-text",
        "scribe_v2",
        [
            "scribe_v2",
            "scribe_v1"
        ]);

    public static TranscriptionProviderPreset Soniox { get; } = new(
        "soniox",
        "Soniox",
        "https://api.soniox.com/v1/transcriptions",
        "stt-async-v4",
        [
            "stt-async-v4"
        ]);

    public static TranscriptionProviderPreset Speechmatics { get; } = new(
        "speechmatics",
        "Speechmatics",
        "https://eu1.asr.api.speechmatics.com/v2/jobs",
        "speechmatics-enhanced",
        [
            "speechmatics-enhanced"
        ]);

    public static TranscriptionProviderPreset Gemini { get; } = new(
        "gemini",
        "Gemini",
        "https://generativelanguage.googleapis.com/v1beta/models",
        "gemini-2.5-flash",
        [
            "gemini-2.5-pro",
            "gemini-2.5-flash",
            "gemini-3.1-pro-preview",
            "gemini-3-flash-preview"
        ]);

    public static IReadOnlyList<TranscriptionProviderPreset> All { get; } =
    [
        Custom,
        Groq,
        Deepgram,
        AssemblyAI,
        Mistral,
        ElevenLabs,
        Soniox,
        Speechmatics,
        Gemini
    ];

    public static TranscriptionProviderPreset Resolve(string? id) =>
        All.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase))
        ?? Custom;
}
