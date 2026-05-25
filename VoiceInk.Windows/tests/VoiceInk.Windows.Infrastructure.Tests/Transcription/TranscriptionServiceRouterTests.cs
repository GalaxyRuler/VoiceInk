using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Transcription;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Transcription;

public sealed class TranscriptionServiceRouterTests
{
    [Fact]
    public async Task TranscribeAsync_RoutesLocalWhisperOptionsToLocalService()
    {
        var local = new FakeTranscriptionService(new TranscriptionResult("local text", TimeSpan.Zero, "local-whisper"));
        var cloud = new FakeTranscriptionService(new TranscriptionResult("cloud text", TimeSpan.Zero, "openai-compatible"));
        var router = new TranscriptionServiceRouter(local, cloud);
        var audio = Audio();
        var options = new TranscriptionOptions("ggml-base.en.bin");

        var result = await router.TranscribeAsync(audio, options, CancellationToken.None);

        Assert.Equal("local text", result.Text);
        Assert.Equal(1, local.CallCount);
        Assert.Equal(0, cloud.CallCount);
        Assert.Equal(audio, local.LastAudio);
        Assert.Equal(options, local.LastOptions);
    }

    [Fact]
    public async Task TranscribeAsync_RoutesOpenAICompatibleOptionsToCloudService()
    {
        var local = new FakeTranscriptionService(new TranscriptionResult("local text", TimeSpan.Zero, "local-whisper"));
        var cloud = new FakeTranscriptionService(new TranscriptionResult("cloud text", TimeSpan.Zero, "openai-compatible"));
        var router = new TranscriptionServiceRouter(local, cloud);
        var audio = Audio();
        var options = new TranscriptionOptions(
            string.Empty,
            "en",
            string.Empty,
            TranscriptionProviderKind.OpenAICompatible,
            "https://api.example.test/v1/audio/transcriptions",
            "gpt-4o-transcribe");

        var result = await router.TranscribeAsync(audio, options, CancellationToken.None);

        Assert.Equal("cloud text", result.Text);
        Assert.Equal(0, local.CallCount);
        Assert.Equal(1, cloud.CallCount);
        Assert.Equal(audio, cloud.LastAudio);
        Assert.Equal(options, cloud.LastOptions);
    }

    [Fact]
    public async Task TranscribeAsync_RoutesDeepgramCloudOptionsToDeepgramService()
    {
        var local = new FakeTranscriptionService(new TranscriptionResult("local text", TimeSpan.Zero, "local-whisper"));
        var cloud = new FakeTranscriptionService(new TranscriptionResult("cloud text", TimeSpan.Zero, "openai-compatible"));
        var deepgram = new FakeTranscriptionService(new TranscriptionResult("deepgram text", TimeSpan.Zero, "deepgram"));
        var router = new TranscriptionServiceRouter(local, cloud, deepgram);
        var audio = Audio();
        var options = new TranscriptionOptions(
            string.Empty,
            "en",
            string.Empty,
            TranscriptionProviderKind.OpenAICompatible,
            "https://api.deepgram.com/v1/listen",
            "nova-3",
            "deepgram");

        var result = await router.TranscribeAsync(audio, options, CancellationToken.None);

        Assert.Equal("deepgram text", result.Text);
        Assert.Equal(0, local.CallCount);
        Assert.Equal(0, cloud.CallCount);
        Assert.Equal(1, deepgram.CallCount);
        Assert.Equal(audio, deepgram.LastAudio);
        Assert.Equal(options, deepgram.LastOptions);
    }

    [Fact]
    public async Task TranscribeAsync_RoutesAssemblyAiCloudOptionsToAssemblyAiService()
    {
        var local = new FakeTranscriptionService(new TranscriptionResult("local text", TimeSpan.Zero, "local-whisper"));
        var cloud = new FakeTranscriptionService(new TranscriptionResult("cloud text", TimeSpan.Zero, "openai-compatible"));
        var deepgram = new FakeTranscriptionService(new TranscriptionResult("deepgram text", TimeSpan.Zero, "deepgram"));
        var assemblyAI = new FakeTranscriptionService(new TranscriptionResult("assembly text", TimeSpan.Zero, "assemblyai"));
        var router = new TranscriptionServiceRouter(local, cloud, deepgram, assemblyAI);
        var audio = Audio();
        var options = new TranscriptionOptions(
            string.Empty,
            "en",
            string.Empty,
            TranscriptionProviderKind.OpenAICompatible,
            "https://streaming.assemblyai.com/v3/ws",
            "universal-3-pro",
            "assemblyai");

        var result = await router.TranscribeAsync(audio, options, CancellationToken.None);

        Assert.Equal("assembly text", result.Text);
        Assert.Equal(0, local.CallCount);
        Assert.Equal(0, cloud.CallCount);
        Assert.Equal(0, deepgram.CallCount);
        Assert.Equal(1, assemblyAI.CallCount);
        Assert.Equal(audio, assemblyAI.LastAudio);
        Assert.Equal(options, assemblyAI.LastOptions);
    }

    private static AudioCaptureResult Audio() =>
        new("sample.wav", TimeSpan.FromSeconds(1), SampleRate: 16000, ChannelCount: 1);

    private sealed class FakeTranscriptionService(TranscriptionResult result) : ITranscriptionService
    {
        public int CallCount { get; private set; }
        public AudioCaptureResult? LastAudio { get; private set; }
        public TranscriptionOptions? LastOptions { get; private set; }

        public Task<TranscriptionResult> TranscribeAsync(
            AudioCaptureResult audio,
            TranscriptionOptions options,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastAudio = audio;
            LastOptions = options;
            return Task.FromResult(result);
        }
    }
}
