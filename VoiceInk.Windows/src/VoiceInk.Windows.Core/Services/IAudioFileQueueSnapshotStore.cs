using VoiceInk.Windows.Core.AudioFiles;

namespace VoiceInk.Windows.Core.Services;

public interface IAudioFileQueueSnapshotStore
{
    Task<AudioFileQueueSnapshot> LoadAsync(CancellationToken cancellationToken);

    Task SaveAsync(AudioFileQueueSnapshot snapshot, CancellationToken cancellationToken);
}
