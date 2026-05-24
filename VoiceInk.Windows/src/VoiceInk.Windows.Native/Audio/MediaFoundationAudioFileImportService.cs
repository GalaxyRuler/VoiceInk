using NAudio.Wave;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.AudioFiles;

namespace VoiceInk.Windows.Native.Audio;

public sealed class MediaFoundationAudioFileImportService : IAudioFileImportService
{
    private const int WhisperSampleRate = 16000;
    private const int WhisperChannelCount = 1;

    public Task<AudioCaptureResult> PrepareAsync(
        string sourcePath,
        string recordingsDirectory,
        CancellationToken cancellationToken) =>
        Task.Run(() => Prepare(sourcePath, recordingsDirectory, cancellationToken));

    private static AudioCaptureResult Prepare(
        string sourcePath,
        string recordingsDirectory,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("The selected audio file was not found.", sourcePath);
        }

        Directory.CreateDirectory(recordingsDirectory);
        var destinationPath = Path.Combine(recordingsDirectory, $"transcribed_{Guid.NewGuid():N}.wav");

        try
        {
            using var reader = new MediaFoundationReader(sourcePath);
            var duration = reader.TotalTime;
            using var resampler = new MediaFoundationResampler(
                reader,
                new WaveFormat(WhisperSampleRate, 16, WhisperChannelCount))
            {
                ResamplerQuality = 60
            };
            WriteWaveFile(destinationPath, resampler, cancellationToken);

            return new AudioCaptureResult(destinationPath, duration, WhisperSampleRate, WhisperChannelCount);
        }
        catch (OperationCanceledException)
        {
            TryDeletePartialFile(destinationPath);
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            TryDeletePartialFile(destinationPath);
            throw new InvalidOperationException($"Could not decode selected media file: {ex.Message}", ex);
        }
    }

    private static void WriteWaveFile(
        string destinationPath,
        IWaveProvider waveProvider,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var writer = new WaveFileWriter(destinationPath, waveProvider.WaveFormat);
        var buffer = new byte[waveProvider.WaveFormat.AverageBytesPerSecond / 4];
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytesRead = waveProvider.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                break;
            }

            writer.Write(buffer, 0, bytesRead);
        }
    }

    private static void TryDeletePartialFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Best-effort cleanup; the caller receives the original decoder error.
        }
    }
}
