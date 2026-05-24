using System.Diagnostics;
using System.Text;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Transcription;
using Whisper.net;

namespace VoiceInk.Windows.Native.Transcription;

public sealed class WhisperNetTranscriptionService : ITranscriptionService
{
    public async Task<TranscriptionResult> TranscribeAsync(
        AudioCaptureResult audio,
        TranscriptionOptions options,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(options.ModelPath))
        {
            throw new FileNotFoundException("The configured whisper model file was not found.", options.ModelPath);
        }

        if (!File.Exists(audio.FilePath))
        {
            throw new FileNotFoundException("The recorded audio file was not found.", audio.FilePath);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var stopwatch = Stopwatch.StartNew();
        var text = new StringBuilder();

        using var factory = WhisperFactory.FromPath(options.ModelPath);
        var builder = factory.CreateBuilder();
        if (string.Equals(options.Language, "auto", StringComparison.OrdinalIgnoreCase))
        {
            builder.WithLanguageDetection();
        }
        else
        {
            builder.WithLanguage(options.Language);
        }

        if (!string.IsNullOrWhiteSpace(options.Prompt))
        {
            builder.WithPrompt(options.Prompt);
        }

        using var processor = builder.Build();

        cancellationToken.ThrowIfCancellationRequested();

        await using var stream = File.OpenRead(audio.FilePath);
        await foreach (var segment in processor.ProcessAsync(stream, cancellationToken))
        {
            text.Append(segment.Text);
        }

        stopwatch.Stop();
        return new TranscriptionResult(text.ToString(), stopwatch.Elapsed, "local-whisper");
    }
}
