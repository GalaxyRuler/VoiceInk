using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed class MistralLiveTranscriptionPreviewService(
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
                TranscriptionProviderPresetCatalog.Mistral.Id,
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
            await ConnectWithTimeoutAsync(
                    socket,
                    MistralStreamingUriFactory.Build(settings),
                    new Dictionary<string, string>
                    {
                        ["Authorization"] = $"Bearer {apiKey}"
                    },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            await CleanupUnstartedSocketAsync(socket).ConfigureAwait(false);
            throw;
        }

        return new MistralLiveTranscriptionPreviewSession(socket, partialTranscriptUpdated, cleanupTimeout);
    }

    private async Task ConnectWithTimeoutAsync(
        IStreamingWebSocket socket,
        Uri uri,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(connectionTimeout);
        try
        {
            await socket.ConnectAsync(uri, headers, timeout.Token)
                .WaitAsync(timeout.Token)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested
            && timeout.IsCancellationRequested)
        {
            throw new TimeoutException("Mistral live transcript preview connection timed out.");
        }
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

    private sealed class MistralLiveTranscriptionPreviewSession : ILiveTranscriptionPreviewSession
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
        private bool sessionUpdated;

        public MistralLiveTranscriptionPreviewSession(
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
                await EnsureSessionUpdatedNoThrowAsync(16000, cancellationToken).ConfigureAwait(false);
                await SendTextNoThrowAsync(CreateTypeMessage("input_audio.flush"), cancellationToken).ConfigureAwait(false);
                await SendTextNoThrowAsync(CreateTypeMessage("input_audio.end"), cancellationToken).ConfigureAwait(false);
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

                    await EnsureSessionUpdatedAsync(chunk.SampleRate, lifetime.Token).ConfigureAwait(false);
                    await socket.SendTextAsync(CreateAudioAppendMessage(chunk.Pcm16Bytes), lifetime.Token)
                        .ConfigureAwait(false);
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
            var transcript = new StringBuilder();
            try
            {
                await foreach (var message in socket.ReceiveTextMessagesAsync(lifetime.Token).ConfigureAwait(false))
                {
                    var parsed = MistralStreamingMessageParser.Parse(message);
                    if (parsed is null)
                    {
                        continue;
                    }

                    if (parsed.IsFinal)
                    {
                        Emit(parsed.Text);
                        continue;
                    }

                    transcript.Append(parsed.Text);
                    Emit(transcript.ToString());
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

        private async Task EnsureSessionUpdatedNoThrowAsync(int sampleRate, CancellationToken cancellationToken)
        {
            try
            {
                await EnsureSessionUpdatedAsync(sampleRate, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        private async Task EnsureSessionUpdatedAsync(int sampleRate, CancellationToken cancellationToken)
        {
            if (sessionUpdated)
            {
                return;
            }

            await socket.SendTextAsync(CreateSessionUpdateMessage(sampleRate), cancellationToken)
                .ConfigureAwait(false);
            sessionUpdated = true;
        }

        private async Task SendTextNoThrowAsync(string text, CancellationToken cancellationToken)
        {
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(cleanupTimeout);
                await socket.SendTextAsync(text, timeout.Token).WaitAsync(timeout.Token).ConfigureAwait(false);
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

        private static string CreateSessionUpdateMessage(int sampleRate) =>
            JsonSerializer.Serialize(new
            {
                session = new
                {
                    audio_format = new
                    {
                        encoding = "pcm_s16le",
                        sample_rate = sampleRate
                    }
                },
                type = "session.update"
            });

        private static string CreateAudioAppendMessage(ReadOnlyMemory<byte> audioBytes) =>
            JsonSerializer.Serialize(new
            {
                audio = Convert.ToBase64String(audioBytes.Span),
                type = "input_audio.append"
            });

        private static string CreateTypeMessage(string type) =>
            JsonSerializer.Serialize(new
            {
                type
            });

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
