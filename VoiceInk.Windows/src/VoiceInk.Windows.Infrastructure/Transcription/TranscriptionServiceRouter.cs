using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed class TranscriptionServiceRouter(
    ITranscriptionService localWhisperService,
    ITranscriptionService openAICompatibleService,
    ITranscriptionService? deepgramService = null,
    ITranscriptionService? assemblyAIService = null,
    ITranscriptionService? elevenLabsService = null) : ITranscriptionService
{
    public Task<TranscriptionResult> TranscribeAsync(
        AudioCaptureResult audio,
        TranscriptionOptions options,
        CancellationToken cancellationToken)
    {
        var service = options.Provider switch
        {
            TranscriptionProviderKind.LocalWhisper => localWhisperService,
            TranscriptionProviderKind.OpenAICompatible when IsDeepgram(options) && deepgramService is not null =>
                deepgramService,
            TranscriptionProviderKind.OpenAICompatible when IsAssemblyAI(options) && assemblyAIService is not null =>
                assemblyAIService,
            TranscriptionProviderKind.OpenAICompatible when IsElevenLabs(options) && elevenLabsService is not null =>
                elevenLabsService,
            TranscriptionProviderKind.OpenAICompatible => openAICompatibleService,
            _ => throw new InvalidOperationException($"Unsupported transcription provider: {options.Provider}.")
        };

        return service.TranscribeAsync(audio, options, cancellationToken);
    }

    private static bool IsDeepgram(TranscriptionOptions options) =>
        string.Equals(options.CloudProviderId, "deepgram", StringComparison.OrdinalIgnoreCase);

    private static bool IsAssemblyAI(TranscriptionOptions options) =>
        string.Equals(options.CloudProviderId, "assemblyai", StringComparison.OrdinalIgnoreCase);

    private static bool IsElevenLabs(TranscriptionOptions options) =>
        string.Equals(options.CloudProviderId, "elevenlabs", StringComparison.OrdinalIgnoreCase);
}
