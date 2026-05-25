using System.Runtime.ExceptionServices;
using NAudio.Wave;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Native.Audio;

public sealed class NAudioCaptureService(string recordingsDirectory, int? deviceNumber = null)
    : IAudioCaptureService, IAudioChunkPublisher, IDisposable
{
    private readonly object writerLock = new();

    private WaveInEvent? waveIn;
    private WaveFileWriter? writer;
    private TaskCompletionSource<StoppedEventArgs>? recordingStopped;
    private string? currentFilePath;
    private DateTimeOffset startedAt;

    public event EventHandler<AudioInputLevel>? LevelAvailable;
    public event EventHandler<AudioChunk>? AudioChunkAvailable;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (waveIn is not null || writer is not null)
        {
            throw new InvalidOperationException("Recording is already active.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            Directory.CreateDirectory(recordingsDirectory);

            currentFilePath = Path.Combine(recordingsDirectory, $"{Guid.NewGuid():N}.wav");
            recordingStopped = new TaskCompletionSource<StoppedEventArgs>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            startedAt = DateTimeOffset.UtcNow;

            waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(16000, 16, 1),
                BufferMilliseconds = 50
            };
            if (deviceNumber is not null)
            {
                waveIn.DeviceNumber = deviceNumber.Value;
            }

            writer = new WaveFileWriter(currentFilePath, waveIn.WaveFormat);

            waveIn.DataAvailable += OnDataAvailable;
            waveIn.RecordingStopped += OnRecordingStopped;
            waveIn.StartRecording();
        }
        catch
        {
            var failedFilePath = currentFilePath;
            DisposeCurrentRecording();

            if (failedFilePath is not null)
            {
                try
                {
                    File.Delete(failedFilePath);
                }
                catch
                {
                    // Preserve the original startup failure if cleanup cannot delete the partial file.
                }
            }

            throw;
        }

        return Task.CompletedTask;
    }

    public async Task<AudioCaptureResult> StopAsync(CancellationToken cancellationToken)
    {
        if (waveIn is null || writer is null || recordingStopped is null || currentFilePath is null)
        {
            throw new InvalidOperationException("Recording has not started.");
        }

        var stoppedTask = recordingStopped.Task;
        var filePath = currentFilePath;

        StoppedEventArgs stoppedArgs;
        try
        {
            waveIn.StopRecording();
            stoppedArgs = await stoppedTask.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            DisposeCurrentRecording();
            throw;
        }

        var duration = DateTimeOffset.UtcNow - startedAt;
        DisposeCurrentRecording();

        if (stoppedArgs.Exception is not null)
        {
            ExceptionDispatchInfo.Capture(stoppedArgs.Exception).Throw();
        }

        return new AudioCaptureResult(filePath, duration, 16000, 1);
    }

    public void Dispose()
    {
        DisposeCurrentRecording();
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs args)
    {
        lock (writerLock)
        {
            writer?.Write(args.Buffer, 0, args.BytesRecorded);
            writer?.Flush();
        }

        PublishLevel(args.Buffer, args.BytesRecorded);
        PublishAudioChunk(args.Buffer, args.BytesRecorded);
    }

    private void PublishLevel(byte[] buffer, int bytesRecorded)
    {
        var level = AudioLevelMeter.CalculatePcm16Peak(buffer, bytesRecorded);
        try
        {
            LevelAvailable?.Invoke(this, level);
        }
        catch
        {
            // Meter subscribers must not disrupt recording.
        }
    }

    private void PublishAudioChunk(byte[] buffer, int bytesRecorded)
    {
        if (bytesRecorded <= 0)
        {
            return;
        }

        var format = waveIn?.WaveFormat;
        var chunkBytes = buffer.AsSpan(0, bytesRecorded).ToArray();
        var chunk = new AudioChunk(
            chunkBytes,
            format?.SampleRate ?? 16000,
            format?.Channels ?? 1);
        try
        {
            AudioChunkAvailable?.Invoke(this, chunk);
        }
        catch
        {
            // Live preview subscribers must not disrupt recording.
        }
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs args)
    {
        recordingStopped?.TrySetResult(args);
    }

    private void DisposeCurrentRecording()
    {
        var stoppedCompletion = recordingStopped;

        if (waveIn is not null)
        {
            waveIn.DataAvailable -= OnDataAvailable;
            waveIn.RecordingStopped -= OnRecordingStopped;
            waveIn.Dispose();
            waveIn = null;
        }

        lock (writerLock)
        {
            writer?.Dispose();
            writer = null;
        }

        recordingStopped = null;
        currentFilePath = null;
        startedAt = default;

        stoppedCompletion?.TrySetCanceled();
    }
}
