using VoiceInk.Windows.Core.Audio;

namespace VoiceInk.Windows.Core.AudioFiles;

public interface IAudioFileImportService
{
    Task<AudioCaptureResult> PrepareAsync(
        string sourcePath,
        string recordingsDirectory,
        CancellationToken cancellationToken);
}
