using System.Text.Json;
using System.Threading.Channels;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed class SonioxLiveTranscriptionPreviewService(
    ISecretStore secretStore,
    Func<IStreamingWebSocket> webSocketFactory,
    TimeSpan? connectionTimeout = null,
    TimeSpan? cleanupTimeout = null) : ILiveTranscriptionPreviewService
{
    private readonly TimeSpan connectionTimeout = connectionTimeout ?? TimeSpan.FromSeconds(3);
    private readonly TimeSpan cleanupTimeout = cleanupTimeout ?? TimeSpan.FromSeconds(1);

    public async Task<ILiveTranscriptionPreviewSession?> TryStartAsync(
        AppSettings settings,
        Action<string> partialTranscriptUpdated,
        CancellationToken cancellationToken)
    {
        if (!settings.ShowLiveTranscriptPreview
            || settings.TranscriptionProvider != TranscriptionProviderKind.OpenAICompatible
            || !string.Equals(
                TranscriptionProviderPresetCatalog.Resolve(settings.CloudTranscriptionProviderId).Id,
                TranscriptionProviderPresetCatalog.Soniox.Id,
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var apiKey = await ReadApiKeyAsync(settings.CloudTranscriptionProviderId, cancellationToken)
            .ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        var socket = webSocketFactory();
        try
        {
            await ConnectWithTimeoutAsync(socket, cancellationToken).ConfigureAwait(false);
            await socket.SendTextAsync(StartMessage(settings, apiKey), cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await CleanupUnstartedSocketAsync(socket).ConfigureAwait(false);
            throw;
        }

        return new SonioxLiveTranscriptionPreviewSession(socket, partialTranscriptUpdated, cleanupTimeout);
    }

    private async Task ConnectWithTimeoutAsync(IStreamingWebSocket socket, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(connectionTimeout);
        try
        {
            await socket.ConnectAsync(
                    new Uri("wss://stt-rt.soniox.com/transcribe-websocket"),
                    string.Empty,
                    timeout.Token)
                .WaitAsync(timeout.Token)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested
            && timeout.IsCancellationRequested)
        {
            throw new TimeoutException("Soniox live transcript preview connection timed out.");
        }
    }

    private static string StartMessage(AppSettings settings, string apiKey)
    {
        var language = string.IsNullOrWhiteSpace(settings.Language)
            || string.Equals(settings.Language.Trim(), "auto", StringComparison.OrdinalIgnoreCase)
                ? null
                : settings.Language.Trim();

        return JsonSerializer.Serialize(new
        {
            api_key = apiKey,
            model = "stt-rt-preview",
            audio_format = "s16le",
            num_channels = 1,
            sample_rate = 16000,
            language_hints = language is null ? Array.Empty<string>() : [language],
            enable_endpoint_detection = true,
            max_endpoint_delay_ms = 1000
        });
    }

    private async Task<string?> ReadApiKeyAsync(string providerId, CancellationToken cancellationToken)
    {
        foreach (var secretName in TranscriptionConfiguration.SecretNamesForCloudProvider(providerId))
        {
            var apiKey = await secretStore.ReadSecretAsync(secretName, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                return apiKey;
            }
        }

        return null;
    }

    private static async Task CleanupUnstartedSocketAsync(IStreamingWebSocket socket)
    {
        try
        {
            socket.Abort();
        }
        catch
        {
        }

        try
        {
            await socket.DisposeAsync().ConfigureAwait(false);
        }
        catch
        {
        }
    }

    private sealed class SonioxLiveTranscriptionPreviewSession : ILiveTranscriptionPreviewSession
    {
        private readonly IStreamingWebSocket socket;
        private readonly Action<string> partialTranscriptUpdated;
        private readonly Channel<AudioChunk> audioChunks = Channel.CreateBounded<AudioChunk>(
            new BoundedChannelOptions(128)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            });
        private readonly TimeSpan cleanupTimeout;
        private readonly CancellationTokenSource lifetime = new();
        private readonly Task sendTask;
        private readonly Task receiveTask;
        private int isCompleting;
        private int isDisposed;

        public SonioxLiveTranscriptionPreviewSession(
            IStreamingWebSocket socket,
            Action<string> partialTranscriptUpdated,
            TimeSpan cleanupTimeout)
        {
            this.socket = socket;
            this.partialTranscriptUpdated = partialTranscriptUpdated;
            this.cleanupTimeout = cleanupTimeout;
            sendTask = Task.Run(SendLoopAsync);
            receiveTask = Task.Run(ReceiveLoopAsync);
        }

        public void EnqueueAudio(AudioChunk chunk)
        {
            if (Volatile.Read(ref isDisposed) != 0 || Volatile.Read(ref isCompleting) != 0)
            {
                return;
            }

            audioChunks.Writer.TryWrite(chunk);
        }

        public async Task CompleteAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.Exchange(ref isCompleting, 1) == 0)
            {
                audioChunks.Writer.TryComplete();
            }

            var sendCompleted = await WaitNoThrowAsync(sendTask, cleanupTimeout, cancellationToken)
                .ConfigureAwait(false);
            if (!sendCompleted)
            {
                lifetime.Cancel();
                AbortSocketNoThrow();
            }

            if (sendCompleted)
            {
                await SendEmptyFrameNoThrowAsync(cancellationToken).ConfigureAwait(false);
                var closeCompleted = await CloseSocketNoThrowAsync(cancellationToken).ConfigureAwait(false);
                if (!closeCompleted)
                {
                    AbortSocketNoThrow();
                }
            }

            lifetime.Cancel();
            await WaitNoThrowAsync(receiveTask, cleanupTimeout, cancellationToken).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref isDisposed, 1) != 0)
            {
                return;
            }

            await CompleteAsync(CancellationToken.None).ConfigureAwait(false);
            lifetime.Cancel();
            await DisposeSocketNoThrowAsync().ConfigureAwait(false);
            lifetime.Dispose();
        }

        private async Task SendLoopAsync()
        {
            try
            {
                await foreach (var chunk in audioChunks.Reader.ReadAllAsync(lifetime.Token).ConfigureAwait(false))
                {
                    if (chunk.Pcm16Bytes.Length == 0)
                    {
                        continue;
                    }

                    await socket.SendBinaryAsync(chunk.Pcm16Bytes, lifetime.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
            {
            }
            catch
            {
            }
        }

        private async Task ReceiveLoopAsync()
        {
            try
            {
                await foreach (var message in socket.ReceiveTextMessagesAsync(lifetime.Token).ConfigureAwait(false))
                {
                    var transcript = SonioxStreamingMessageParser.Parse(message);
                    if (transcript is null)
                    {
                        continue;
                    }

                    Emit(transcript.Text);
                }
            }
            catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
            {
            }
            catch
            {
            }
        }

        private void Emit(string text)
        {
            try
            {
                partialTranscriptUpdated(text);
            }
            catch
            {
            }
        }

        private async Task SendEmptyFrameNoThrowAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(cleanupTimeout);
                await socket.SendBinaryAsync(Array.Empty<byte>(), timeout.Token)
                    .WaitAsync(timeout.Token)
                    .ConfigureAwait(false);
            }
            catch
            {
            }
        }

        private async Task<bool> CloseSocketNoThrowAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(cleanupTimeout);
                await socket.CloseAsync(timeout.Token).WaitAsync(timeout.Token).ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void AbortSocketNoThrow()
        {
            try
            {
                socket.Abort();
            }
            catch
            {
            }
        }

        private async Task DisposeSocketNoThrowAsync()
        {
            try
            {
                await socket.DisposeAsync().ConfigureAwait(false);
            }
            catch
            {
            }
        }

        private static async Task<bool> WaitNoThrowAsync(Task task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            try
            {
                using var bounded = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                bounded.CancelAfter(timeout);
                await task.WaitAsync(bounded.Token).ConfigureAwait(false);
                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch
            {
                return true;
            }
        }
    }
}
