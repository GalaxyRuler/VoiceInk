using VoiceInk.Windows.Native.Audio;
using VoiceInk.Windows.Core.Audio;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Audio;

public sealed class WavVoiceActivityDetectorTests
{
    [Fact]
    public async Task AnalyzeAsync_ReturnsNoSpeechForSilentPcm16Wav()
    {
        using var temp = new TempWavFile();
        temp.WritePcm16(samples: Enumerable.Repeat((short)0, 16000));
        var detector = new WavVoiceActivityDetector();

        var result = await detector.AnalyzeAsync(
            new AudioCaptureResult(temp.Path, TimeSpan.FromSeconds(1), 16000, 1),
            CancellationToken.None);

        Assert.False(result.HasSpeech);
        Assert.Equal(TimeSpan.Zero, result.SpeechDuration);
    }

    [Fact]
    public async Task AnalyzeAsync_ReturnsSpeechForVoicedPcm16Wav()
    {
        using var temp = new TempWavFile();
        var samples = Enumerable.Range(0, 16000)
            .Select(index => index < 4000 ? (short)3000 : (short)0);
        temp.WritePcm16(samples);
        var detector = new WavVoiceActivityDetector();

        var result = await detector.AnalyzeAsync(
            new AudioCaptureResult(temp.Path, TimeSpan.FromSeconds(1), 16000, 1),
            CancellationToken.None);

        Assert.True(result.HasSpeech);
        Assert.True(result.SpeechDuration >= TimeSpan.FromMilliseconds(250));
    }

    [Fact]
    public async Task AnalyzeAsync_ReturnsNoSpeechForDefaultTwoHundredMillisecondBurst()
    {
        using var temp = new TempWavFile();
        var samples = Enumerable.Range(0, 16000)
            .Select(index => index < 3200 ? (short)3000 : (short)0);
        temp.WritePcm16(samples);
        var detector = new WavVoiceActivityDetector();

        var result = await detector.AnalyzeAsync(
            new AudioCaptureResult(temp.Path, TimeSpan.FromSeconds(1), 16000, 1),
            CancellationToken.None);

        Assert.False(result.HasSpeech);
        Assert.Equal(TimeSpan.FromMilliseconds(200), result.SpeechDuration);
    }

    [Fact]
    public async Task AnalyzeAsync_UsesExplicitMinimumSpeechDurationOverride()
    {
        using var temp = new TempWavFile();
        var samples = Enumerable.Range(0, 16000)
            .Select(index => index < 3200 ? (short)3000 : (short)0);
        temp.WritePcm16(samples);
        var detector = new WavVoiceActivityDetector(
            minimumSpeechDuration: TimeSpan.FromMilliseconds(150));

        var result = await detector.AnalyzeAsync(
            new AudioCaptureResult(temp.Path, TimeSpan.FromSeconds(1), 16000, 1),
            CancellationToken.None);

        Assert.True(result.HasSpeech);
        Assert.Equal(TimeSpan.FromMilliseconds(200), result.SpeechDuration);
    }

    [Fact]
    public async Task AnalyzeAsync_FailsOpenWhenFileCannotBeRead()
    {
        var detector = new WavVoiceActivityDetector();

        var result = await detector.AnalyzeAsync(
            new AudioCaptureResult("missing.wav", TimeSpan.FromSeconds(1), 16000, 1),
            CancellationToken.None);

        Assert.True(result.HasSpeech);
        Assert.Equal(TimeSpan.Zero, result.SpeechDuration);
    }

    private sealed class TempWavFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"voiceink-vad-{Guid.NewGuid():N}.wav");

        public void WritePcm16(IEnumerable<short> samples)
        {
            var sampleArray = samples.ToArray();
            var dataLength = sampleArray.Length * sizeof(short);
            using var stream = File.Create(Path);
            using var writer = new BinaryWriter(stream);
            writer.Write("RIFF"u8);
            writer.Write(36 + dataLength);
            writer.Write("WAVE"u8);
            writer.Write("fmt "u8);
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(16000);
            writer.Write(16000 * sizeof(short));
            writer.Write((short)sizeof(short));
            writer.Write((short)16);
            writer.Write("data"u8);
            writer.Write(dataLength);
            foreach (var sample in sampleArray)
            {
                writer.Write(sample);
            }
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
