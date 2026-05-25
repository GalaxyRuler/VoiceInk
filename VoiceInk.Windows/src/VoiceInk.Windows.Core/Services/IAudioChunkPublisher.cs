using VoiceInk.Windows.Core.Audio;

namespace VoiceInk.Windows.Core.Services;

public interface IAudioChunkPublisher
{
    event EventHandler<AudioChunk>? AudioChunkAvailable;
}
