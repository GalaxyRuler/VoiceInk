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

    [Fact]
    public async Task TranscribeAsync_RoutesElevenLabsCloudOptionsToElevenLabsService()
    {
        var local = new FakeTranscriptionService(new TranscriptionResult("local text", TimeSpan.Zero, "local-whisper"));
        var cloud = new FakeTranscriptionService(new TranscriptionResult("cloud text", TimeSpan.Zero, "openai-compatible"));
        var deepgram = new FakeTranscriptionService(new TranscriptionResult("deepgram text", TimeSpan.Zero, "deepgram"));
        var assemblyAI = new FakeTranscriptionService(new TranscriptionResult("assembly text", TimeSpan.Zero, "assemblyai"));
        var elevenLabs = new FakeTranscriptionService(new TranscriptionResult("scribe text", TimeSpan.Zero, "elevenlabs"));
        var router = new TranscriptionServiceRouter(local, cloud, deepgram, assemblyAI, elevenLabs);
        var audio = Audio();
        var options = new TranscriptionOptions(
            string.Empty,
            "en",
            string.Empty,
            TranscriptionProviderKind.OpenAICompatible,
            "https://api.elevenlabs.io/v1/speech-to-text",
            "scribe_v2",
            "elevenlabs");

        var result = await router.TranscribeAsync(audio, options, CancellationToken.None);

        Assert.Equal("scribe text", result.Text);
        Assert.Equal(0, local.CallCount);
        Assert.Equal(0, cloud.CallCount);
        Assert.Equal(0, deepgram.CallCount);
        Assert.Equal(0, assemblyAI.CallCount);
        Assert.Equal(1, elevenLabs.CallCount);
        Assert.Equal(audio, elevenLabs.LastAudio);
        Assert.Equal(options, elevenLabs.LastOptions);
    }

    [Fact]
    public async Task TranscribeAsync_RoutesSonioxCloudOptionsToSonioxService()
    {
        var local = new FakeTranscriptionService(new TranscriptionResult("local text", TimeSpan.Zero, "local-whisper"));
        var cloud = new FakeTranscriptionService(new TranscriptionResult("cloud text", TimeSpan.Zero, "openai-compatible"));
        var deepgram = new FakeTranscriptionService(new TranscriptionResult("deepgram text", TimeSpan.Zero, "deepgram"));
        var assemblyAI = new FakeTranscriptionService(new TranscriptionResult("assembly text", TimeSpan.Zero, "assemblyai"));
        var elevenLabs = new FakeTranscriptionService(new TranscriptionResult("scribe text", TimeSpan.Zero, "elevenlabs"));
        var soniox = new FakeTranscriptionService(new TranscriptionResult("soniox text", TimeSpan.Zero, "soniox"));
        var router = new TranscriptionServiceRouter(local, cloud, deepgram, assemblyAI, elevenLabs, soniox);
        var audio = Audio();
        var options = new TranscriptionOptions(
            string.Empty,
            "en",
            string.Empty,
            TranscriptionProviderKind.OpenAICompatible,
            "https://api.soniox.com/v1/transcriptions",
            "stt-async-v4",
            "soniox");

        var result = await router.TranscribeAsync(audio, options, CancellationToken.None);

        Assert.Equal("soniox text", result.Text);
        Assert.Equal(0, local.CallCount);
        Assert.Equal(0, cloud.CallCount);
        Assert.Equal(0, deepgram.CallCount);
        Assert.Equal(0, assemblyAI.CallCount);
        Assert.Equal(0, elevenLabs.CallCount);
        Assert.Equal(1, soniox.CallCount);
        Assert.Equal(audio, soniox.LastAudio);
        Assert.Equal(options, soniox.LastOptions);
    }

    [Fact]
    public async Task TranscribeAsync_RoutesSpeechmaticsCloudOptionsToSpeechmaticsService()
    {
        var local = new FakeTranscriptionService(new TranscriptionResult("local text", TimeSpan.Zero, "local-whisper"));
        var cloud = new FakeTranscriptionService(new TranscriptionResult("cloud text", TimeSpan.Zero, "openai-compatible"));
        var deepgram = new FakeTranscriptionService(new TranscriptionResult("deepgram text", TimeSpan.Zero, "deepgram"));
        var assemblyAI = new FakeTranscriptionService(new TranscriptionResult("assembly text", TimeSpan.Zero, "assemblyai"));
        var elevenLabs = new FakeTranscriptionService(new TranscriptionResult("scribe text", TimeSpan.Zero, "elevenlabs"));
        var soniox = new FakeTranscriptionService(new TranscriptionResult("soniox text", TimeSpan.Zero, "soniox"));
        var speechmatics = new FakeTranscriptionService(new TranscriptionResult("speechmatics text", TimeSpan.Zero, "speechmatics"));
        var router = new TranscriptionServiceRouter(
            local,
            cloud,
            deepgram,
            assemblyAI,
            elevenLabs,
            soniox,
            speechmatics);
        var audio = Audio();
        var options = new TranscriptionOptions(
            string.Empty,
            "auto",
            string.Empty,
            TranscriptionProviderKind.OpenAICompatible,
            "https://eu1.asr.api.speechmatics.com/v2/jobs",
            "speechmatics-enhanced",
            "speechmatics");

        var result = await router.TranscribeAsync(audio, options, CancellationToken.None);

        Assert.Equal("speechmatics text", result.Text);
        Assert.Equal(0, local.CallCount);
        Assert.Equal(0, cloud.CallCount);
        Assert.Equal(0, deepgram.CallCount);
        Assert.Equal(0, assemblyAI.CallCount);
        Assert.Equal(0, elevenLabs.CallCount);
        Assert.Equal(0, soniox.CallCount);
        Assert.Equal(1, speechmatics.CallCount);
        Assert.Equal(audio, speechmatics.LastAudio);
        Assert.Equal(options, speechmatics.LastOptions);
    }

    [Fact]
    public async Task TranscribeAsync_RoutesGeminiCloudOptionsToGeminiService()
    {
        var local = new FakeTranscriptionService(new TranscriptionResult("local text", TimeSpan.Zero, "local-whisper"));
        var cloud = new FakeTranscriptionService(new TranscriptionResult("cloud text", TimeSpan.Zero, "openai-compatible"));
        var deepgram = new FakeTranscriptionService(new TranscriptionResult("deepgram text", TimeSpan.Zero, "deepgram"));
        var assemblyAI = new FakeTranscriptionService(new TranscriptionResult("assembly text", TimeSpan.Zero, "assemblyai"));
        var elevenLabs = new FakeTranscriptionService(new TranscriptionResult("scribe text", TimeSpan.Zero, "elevenlabs"));
        var soniox = new FakeTranscriptionService(new TranscriptionResult("soniox text", TimeSpan.Zero, "soniox"));
        var speechmatics = new FakeTranscriptionService(new TranscriptionResult("speechmatics text", TimeSpan.Zero, "speechmatics"));
        var gemini = new FakeTranscriptionService(new TranscriptionResult("gemini text", TimeSpan.Zero, "gemini"));
        var router = new TranscriptionServiceRouter(
            local,
            cloud,
            deepgram,
            assemblyAI,
            elevenLabs,
            soniox,
            speechmatics,
            gemini);
        var audio = Audio();
        var options = new TranscriptionOptions(
            string.Empty,
            "auto",
            string.Empty,
            TranscriptionProviderKind.OpenAICompatible,
            "https://generativelanguage.googleapis.com/v1beta/models",
            "gemini-2.5-flash",
            "gemini");

        var result = await router.TranscribeAsync(audio, options, CancellationToken.None);

        Assert.Equal("gemini text", result.Text);
        Assert.Equal(0, local.CallCount);
        Assert.Equal(0, cloud.CallCount);
        Assert.Equal(0, deepgram.CallCount);
        Assert.Equal(0, assemblyAI.CallCount);
        Assert.Equal(0, elevenLabs.CallCount);
        Assert.Equal(0, soniox.CallCount);
        Assert.Equal(0, speechmatics.CallCount);
        Assert.Equal(1, gemini.CallCount);
        Assert.Equal(audio, gemini.LastAudio);
        Assert.Equal(options, gemini.LastOptions);
    }

    [Fact]
    public async Task TranscribeAsync_RoutesXaiCloudOptionsToXaiService()
    {
        var local = new FakeTranscriptionService(new TranscriptionResult("local text", TimeSpan.Zero, "local-whisper"));
        var cloud = new FakeTranscriptionService(new TranscriptionResult("cloud text", TimeSpan.Zero, "openai-compatible"));
        var deepgram = new FakeTranscriptionService(new TranscriptionResult("deepgram text", TimeSpan.Zero, "deepgram"));
        var assemblyAI = new FakeTranscriptionService(new TranscriptionResult("assembly text", TimeSpan.Zero, "assemblyai"));
        var elevenLabs = new FakeTranscriptionService(new TranscriptionResult("scribe text", TimeSpan.Zero, "elevenlabs"));
        var soniox = new FakeTranscriptionService(new TranscriptionResult("soniox text", TimeSpan.Zero, "soniox"));
        var speechmatics = new FakeTranscriptionService(new TranscriptionResult("speechmatics text", TimeSpan.Zero, "speechmatics"));
        var gemini = new FakeTranscriptionService(new TranscriptionResult("gemini text", TimeSpan.Zero, "gemini"));
        var xai = new FakeTranscriptionService(new TranscriptionResult("grok text", TimeSpan.Zero, "xai"));
        var router = new TranscriptionServiceRouter(
            local,
            cloud,
            deepgram,
            assemblyAI,
            elevenLabs,
            soniox,
            speechmatics,
            gemini,
            xai);
        var options = new TranscriptionOptions(
            string.Empty,
            "auto",
            string.Empty,
            TranscriptionProviderKind.OpenAICompatible,
            "https://api.x.ai/v1/stt",
            "grok-stt",
            "xai");

        var result = await router.TranscribeAsync(Audio(), options, CancellationToken.None);

        Assert.Equal("grok text", result.Text);
        Assert.Equal(1, xai.CallCount);
        Assert.Equal(options, xai.LastOptions);
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
