using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Core.Models;

public interface IWhisperModelWarmupService
{
    Task WarmupAsync(TranscriptionOptions options, CancellationToken cancellationToken);
}
