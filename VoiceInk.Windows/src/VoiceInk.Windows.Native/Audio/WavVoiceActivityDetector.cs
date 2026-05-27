using System.Text;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Native.Audio;

public sealed class WavVoiceActivityDetector(
    int amplitudeThreshold = 500,
    TimeSpan? minimumSpeechDuration = null) : IVoiceActivityDetector
{
    private readonly TimeSpan minimumSpeechDuration = minimumSpeechDuration ?? TimeSpan.FromMilliseconds(250);

    public Task<VoiceActivityResult> AnalyzeAsync(
        AudioCaptureResult audio,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return Task.FromResult(Analyze(audio.FilePath, cancellationToken));
        }
        catch
        {
            return Task.FromResult(new VoiceActivityResult(true, TimeSpan.Zero));
        }
    }

    private VoiceActivityResult Analyze(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return new VoiceActivityResult(true, TimeSpan.Zero);
        }

        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream, Encoding.ASCII);
        if (ReadFourCc(reader) != "RIFF")
        {
            return new VoiceActivityResult(true, TimeSpan.Zero);
        }

        reader.ReadInt32();
        if (ReadFourCc(reader) != "WAVE")
        {
            return new VoiceActivityResult(true, TimeSpan.Zero);
        }

        var sampleRate = 0;
        var channels = 0;
        var bitsPerSample = 0;
        while (stream.Position + 8 <= stream.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var chunkId = ReadFourCc(reader);
            var chunkSize = reader.ReadInt32();
            var chunkEnd = stream.Position + chunkSize;
            if (chunkEnd > stream.Length)
            {
                return new VoiceActivityResult(true, TimeSpan.Zero);
            }

            if (chunkId == "fmt ")
            {
                var audioFormat = reader.ReadInt16();
                channels = reader.ReadInt16();
                sampleRate = reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt16();
                bitsPerSample = reader.ReadInt16();
                if (audioFormat != 1 || sampleRate <= 0 || channels <= 0 || bitsPerSample != 16)
                {
                    return new VoiceActivityResult(true, TimeSpan.Zero);
                }
            }
            else if (chunkId == "data")
            {
                return AnalyzePcm16Data(reader, chunkSize, sampleRate, channels, cancellationToken);
            }

            stream.Position = chunkEnd + (chunkSize % 2);
        }

        return new VoiceActivityResult(true, TimeSpan.Zero);
    }

    private VoiceActivityResult AnalyzePcm16Data(
        BinaryReader reader,
        int dataBytes,
        int sampleRate,
        int channels,
        CancellationToken cancellationToken)
    {
        if (sampleRate <= 0 || channels <= 0)
        {
            return new VoiceActivityResult(true, TimeSpan.Zero);
        }

        var frameCount = dataBytes / (sizeof(short) * channels);
        var voicedFrames = 0;
        for (var frame = 0; frame < frameCount; frame++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var framePeak = 0;
            for (var channel = 0; channel < channels; channel++)
            {
                var sample = reader.ReadInt16();
                framePeak = Math.Max(framePeak, Math.Abs((int)sample));
            }

            if (framePeak >= amplitudeThreshold)
            {
                voicedFrames++;
            }
        }

        var speechDuration = TimeSpan.FromSeconds((double)voicedFrames / sampleRate);
        return new VoiceActivityResult(speechDuration >= minimumSpeechDuration, speechDuration);
    }

    private static string ReadFourCc(BinaryReader reader)
    {
        var bytes = reader.ReadBytes(4);
        return bytes.Length == 4 ? Encoding.ASCII.GetString(bytes) : string.Empty;
    }
}
