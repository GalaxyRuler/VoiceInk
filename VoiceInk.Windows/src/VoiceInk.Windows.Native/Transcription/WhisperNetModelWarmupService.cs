using VoiceInk.Windows.Core.Models;
using VoiceInk.Windows.Core.Transcription;
using Whisper.net;

namespace VoiceInk.Windows.Native.Transcription;

public sealed class WhisperNetModelWarmupService : IWhisperModelWarmupService
{
    public Task WarmupAsync(TranscriptionOptions options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(options.ModelPath))
        {
            throw new FileNotFoundException("The configured whisper model file was not found.", options.ModelPath);
        }

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
        return Task.CompletedTask;
    }
}
