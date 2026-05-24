using NAudio.Wave;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Native.Audio;

public sealed class NAudioCaptureService(string recordingsDirectory) : IAudioCaptureService, IDisposable
{
    private WaveInEvent? waveIn;
    private WaveFileWriter? writer;
    private string? currentFilePath;
    private DateTimeOffset startedAt;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(recordingsDirectory);

        currentFilePath = Path.Combine(recordingsDirectory, $"{Guid.NewGuid():N}.wav");
        startedAt = DateTimeOffset.UtcNow;

        waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 16, 1),
            BufferMilliseconds = 50
        };
        writer = new WaveFileWriter(currentFilePath, waveIn.WaveFormat);

        waveIn.DataAvailable += OnDataAvailable;
        waveIn.RecordingStopped += OnRecordingStopped;
        waveIn.StartRecording();

        return Task.CompletedTask;
    }

    public Task<AudioCaptureResult> StopAsync(CancellationToken cancellationToken)
    {
        if (waveIn is null || writer is null || currentFilePath is null)
        {
            throw new InvalidOperationException("Recording has not started.");
        }

        waveIn.StopRecording();
        var duration = DateTimeOffset.UtcNow - startedAt;
        var result = new AudioCaptureResult(currentFilePath, duration, 16000, 1);

        DisposeCurrentRecording();
        return Task.FromResult(result);
    }

    public void Dispose()
    {
        DisposeCurrentRecording();
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs args)
    {
        writer?.Write(args.Buffer, 0, args.BytesRecorded);
        writer?.Flush();
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs args)
    {
        if (args.Exception is not null)
        {
            DisposeCurrentRecording();
        }
    }

    private void DisposeCurrentRecording()
    {
        if (waveIn is not null)
        {
            waveIn.DataAvailable -= OnDataAvailable;
            waveIn.RecordingStopped -= OnRecordingStopped;
            waveIn.Dispose();
            waveIn = null;
        }

        writer?.Dispose();
        writer = null;
    }
}
