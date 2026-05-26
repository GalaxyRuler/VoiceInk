namespace VoiceInk.Windows.Core.Transcription;

public static class TranscriptionProviderPresetCatalog
{
    public static TranscriptionProviderPreset Custom { get; } = new(
        "custom",
        "Custom OpenAI-compatible",
        string.Empty,
        string.Empty,
        [],
        "Custom endpoint for any OpenAI-compatible speech-to-text API.",
        "Depends on provider",
        "Custom",
        "Custom",
        "Provider-defined");

    public static TranscriptionProviderPreset Groq { get; } = new(
        "groq",
        "Groq",
        "https://api.groq.com/openai/v1/audio/transcriptions",
        "whisper-large-v3-turbo",
        [
            "whisper-large-v3-turbo",
            "whisper-large-v3"
        ],
        "Whisper Large v3 Turbo with Groq's low-latency inference.",
        "Multilingual",
        "Fast",
        "High",
        "Batch");

    public static TranscriptionProviderPreset Deepgram { get; } = new(
        "deepgram",
        "Deepgram",
        "https://api.deepgram.com/v1/listen",
        "nova-3",
        [
            "nova-3",
            "nova-3-medical"
        ],
        "Deepgram Nova transcription with fast batch requests and live recorder preview.",
        "Multilingual",
        "Very fast",
        "High",
        "Realtime preview");

    public static TranscriptionProviderPreset AssemblyAI { get; } = new(
        "assemblyai",
        "AssemblyAI",
        "https://streaming.assemblyai.com/v3/ws",
        "universal-3-pro",
        [
            "universal-3-pro",
            "universal-streaming"
        ],
        "AssemblyAI Universal models for high-accuracy multilingual transcription.",
        "Multilingual",
        "Very fast",
        "Very high",
        "Realtime preview");

    public static TranscriptionProviderPreset Mistral { get; } = new(
        "mistral",
        "Mistral",
        "https://api.mistral.ai/v1/audio/transcriptions",
        "voxtral-mini-latest",
        [
            "voxtral-mini-latest"
        ],
        "Mistral Voxtral model for fast multilingual audio transcription.",
        "Multilingual",
        "Very fast",
        "Very high",
        "Batch");

    public static TranscriptionProviderPreset ElevenLabs { get; } = new(
        "elevenlabs",
        "ElevenLabs",
        "https://api.elevenlabs.io/v1/speech-to-text",
        "scribe_v2",
        [
            "scribe_v2",
            "scribe_v1"
        ],
        "ElevenLabs Scribe transcription with broad language coverage.",
        "Multilingual",
        "Very fast",
        "Very high",
        "Batch");

    public static TranscriptionProviderPreset Soniox { get; } = new(
        "soniox",
        "Soniox",
        "https://api.soniox.com/v1/transcriptions",
        "stt-async-v4",
        [
            "stt-async-v4"
        ],
        "Soniox V4 async transcription with custom vocabulary support.",
        "Multilingual",
        "Very fast",
        "Very high",
        "Batch");

    public static TranscriptionProviderPreset Speechmatics { get; } = new(
        "speechmatics",
        "Speechmatics",
        "https://eu1.asr.api.speechmatics.com/v2/jobs",
        "speechmatics-enhanced",
        [
            "speechmatics-enhanced"
        ],
        "Speechmatics enhanced transcription with 50+ language support.",
        "Multilingual",
        "Very fast",
        "Very high",
        "Batch");

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
        ],
        "Gemini audio understanding through inline audio generateContent requests.",
        "Multilingual",
        "Fast",
        "High",
        "Batch");

    public static TranscriptionProviderPreset Xai { get; } = new(
        "xai",
        "xAI",
        "https://api.x.ai/v1/stt",
        "grok-stt",
        [
            "grok-stt"
        ],
        "xAI Grok speech-to-text with multilingual batch transcription.",
        "Multilingual",
        "Very fast",
        "Very high",
        "Batch");

    public static TranscriptionProviderPreset Cartesia { get; } = new(
        "cartesia",
        "Cartesia",
        "https://api.cartesia.ai/stt",
        "ink-whisper",
        [
            "ink-whisper"
        ],
        "Cartesia Ink Whisper for streaming-first low-latency transcription.",
        "Multilingual",
        "Very fast",
        "High",
        "Realtime preview");

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
        Gemini,
        Xai,
        Cartesia
    ];

    public static TranscriptionProviderPreset Resolve(string? id) =>
        All.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase))
        ?? Custom;
}
