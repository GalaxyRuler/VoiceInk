using VoiceInk.Windows.Core.Audio;

namespace VoiceInk.Windows.Core.Services;

public interface ILiveTranscriptionPreviewSession : IAsyncDisposable
{
    void EnqueueAudio(AudioChunk chunk);

    Task CompleteAsync(CancellationToken cancellationToken);
}
