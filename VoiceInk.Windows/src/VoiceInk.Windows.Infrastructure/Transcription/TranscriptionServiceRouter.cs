using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed class TranscriptionServiceRouter(
    ITranscriptionService localWhisperService,
    ITranscriptionService openAICompatibleService) : ITranscriptionService
{
    public Task<TranscriptionResult> TranscribeAsync(
        AudioCaptureResult audio,
        TranscriptionOptions options,
        CancellationToken cancellationToken)
    {
        var service = options.Provider switch
        {
            TranscriptionProviderKind.LocalWhisper => localWhisperService,
            TranscriptionProviderKind.OpenAICompatible => openAICompatibleService,
            _ => throw new InvalidOperationException($"Unsupported transcription provider: {options.Provider}.")
        };

        return service.TranscribeAsync(audio, options, cancellationToken);
    }
}
