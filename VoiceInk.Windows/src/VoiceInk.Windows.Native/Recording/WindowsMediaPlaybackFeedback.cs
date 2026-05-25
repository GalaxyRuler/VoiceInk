using Windows.Media.Control;
using VoiceInk.Windows.Core.Recording;

namespace VoiceInk.Windows.Native.Recording;

public sealed class WindowsMediaPlaybackFeedback : IMediaPlaybackFeedback
{
    private readonly object gate = new();
    private string? pausedSessionSourceAppUserModelId;

    public async Task PauseAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (gate)
        {
            if (pausedSessionSourceAppUserModelId is not null)
            {
                return;
            }
        }

        try
        {
            var manager = await GlobalSystemMediaTransportControlsSessionManager
                .RequestAsync()
                .AsTask(CancellationToken.None)
                .ConfigureAwait(false);
            var session = manager.GetCurrentSession();
            if (session?.GetPlaybackInfo().PlaybackStatus
                != GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
            {
                return;
            }

            if (!await session.TryPauseAsync().AsTask(cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            lock (gate)
            {
                pausedSessionSourceAppUserModelId = session.SourceAppUserModelId;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            lock (gate)
            {
                pausedSessionSourceAppUserModelId = null;
            }
        }
    }

    public async Task ResumeAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        string? sourceAppUserModelId;
        lock (gate)
        {
            sourceAppUserModelId = pausedSessionSourceAppUserModelId;
        }

        if (string.IsNullOrWhiteSpace(sourceAppUserModelId))
        {
            return;
        }

        if (delay > TimeSpan.Zero)
        {
            try
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
        }

        try
        {
            var manager = await GlobalSystemMediaTransportControlsSessionManager
                .RequestAsync()
                .AsTask(CancellationToken.None)
                .ConfigureAwait(false);
            var session = manager.GetCurrentSession();
            if (session is not null
                && string.Equals(
                    session.SourceAppUserModelId,
                    sourceAppUserModelId,
                    StringComparison.OrdinalIgnoreCase)
                && session.GetPlaybackInfo().PlaybackStatus
                    == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
            {
                _ = await session.TryPlayAsync().AsTask(CancellationToken.None).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch
        {
        }
        finally
        {
            lock (gate)
            {
                pausedSessionSourceAppUserModelId = null;
            }
        }
    }
}
