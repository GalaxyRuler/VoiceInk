using NAudio.Wave;
using VoiceInk.Windows.Native.Audio;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Audio;

public sealed class MediaFoundationAudioFileImportServiceTests
{
    [Fact]
    public async Task PrepareAsync_ConvertsWavToSixteenKilohertzMono()
    {
        using var source = new TempWaveFile(sampleRate: 44100, channels: 2);
        using var output = new TempDirectory();
        var service = new MediaFoundationAudioFileImportService();

        var result = await service.PrepareAsync(source.Path, output.Path, CancellationToken.None);

        Assert.Equal(16000, result.SampleRate);
        Assert.Equal(1, result.ChannelCount);
        Assert.True(File.Exists(result.FilePath));
        using var reader = new WaveFileReader(result.FilePath);
        Assert.Equal(16000, reader.WaveFormat.SampleRate);
        Assert.Equal(1, reader.WaveFormat.Channels);
    }

    [Fact]
    public async Task PrepareAsync_CleansPartialFileWhenAlreadyCanceled()
    {
        using var source = new TempWaveFile(sampleRate: 44100, channels: 2);
        using var output = new TempDirectory();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var service = new MediaFoundationAudioFileImportService();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.PrepareAsync(source.Path, output.Path, cts.Token));

        Assert.Empty(Directory.EnumerateFiles(output.Path));
    }

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"voiceink-import-tests-{Guid.NewGuid():N}");

        public TempDirectory()
        {
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }

    private sealed class TempWaveFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"voiceink-import-source-{Guid.NewGuid():N}.wav");

        public TempWaveFile(int sampleRate, int channels)
        {
            var waveFormat = new WaveFormat(sampleRate, channels);
            using var writer = new WaveFileWriter(Path, waveFormat);
            var buffer = new byte[sampleRate * channels * 2 / 10];
            writer.Write(buffer, 0, buffer.Length);
        }

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }
}
