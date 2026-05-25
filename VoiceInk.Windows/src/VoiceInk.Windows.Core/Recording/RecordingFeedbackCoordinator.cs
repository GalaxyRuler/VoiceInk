using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Recording;

public sealed class RecordingFeedbackCoordinator(
    IRecordingSoundFeedback soundFeedback,
    ISystemAudioFeedback systemAudioFeedback,
    IMediaPlaybackFeedback mediaPlaybackFeedback) : IRecordingCaptureStopFeedback
{
    private RecordingFeedbackSession? currentSession;
    private bool sessionRestored;
    private Task? restoreTask;
    private CancellationTokenSource? restoreCancellation;

    public async Task BeginAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await RestorePendingImmediatelyAsync().ConfigureAwait(false);
        if (currentSession is not null)
        {
            return;
        }

        var session = RecordingFeedbackSession.From(settings);
        currentSession = session;
        sessionRestored = false;

        if (session.SoundFeedbackEnabled)
        {
            Try(soundFeedback.PlayStartSound);
        }

        if (session.SystemMuteEnabled)
        {
            await TryAsync(() => systemAudioFeedback.MuteAsync(cancellationToken)).ConfigureAwait(false);
        }

        if (session.PauseMediaEnabled)
        {
            await TryAsync(() => mediaPlaybackFeedback.PauseAsync(cancellationToken)).ConfigureAwait(false);
        }
    }

    public async Task CaptureStoppedAsync(CancellationToken cancellationToken)
    {
        var session = currentSession;
        if (session is null)
        {
            return;
        }

        sessionRestored = true;
        StartBackgroundRestore(session);
    }

    public async Task CompleteAsync(bool playStopSound, CancellationToken cancellationToken)
    {
        var (session, wasRestored) = TakeCurrentSession();
        if (session is null)
        {
            return;
        }

        if (!wasRestored)
        {
            await RestoreAsync(session, cancellationToken).ConfigureAwait(false);
        }

        if (playStopSound && session.SoundFeedbackEnabled)
        {
            Try(soundFeedback.PlayStopSound);
        }
    }

    public async Task CancelAsync(CancellationToken cancellationToken)
    {
        var (session, wasRestored) = TakeCurrentSession();
        if (session is null)
        {
            return;
        }

        if (!wasRestored)
        {
            await RestoreAsync(session, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task CancelImmediatelyAsync(CancellationToken cancellationToken)
    {
        var (session, wasRestored) = TakeCurrentSession();
        if (session is null)
        {
            await RestorePendingImmediatelyAsync().ConfigureAwait(false);
            return;
        }

        if (wasRestored)
        {
            await RestorePendingImmediatelyAsync().ConfigureAwait(false);
        }
        else
        {
            await RestoreAsync(session with { AudioResumptionDelay = TimeSpan.Zero }, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task RestorePendingImmediatelyAsync()
    {
        var pendingRestoreTask = restoreTask;
        if (pendingRestoreTask is null)
        {
            return;
        }

        if (!pendingRestoreTask.IsCompleted)
        {
            restoreCancellation?.Cancel();
        }

        try
        {
            await pendingRestoreTask.ConfigureAwait(false);
        }
        finally
        {
            if (ReferenceEquals(pendingRestoreTask, restoreTask))
            {
                restoreTask = null;
                restoreCancellation?.Dispose();
                restoreCancellation = null;
            }
        }
    }

    private (RecordingFeedbackSession? Session, bool WasRestored) TakeCurrentSession()
    {
        var session = currentSession;
        var wasRestored = sessionRestored;
        currentSession = null;
        sessionRestored = false;
        return (session, wasRestored);
    }

    private async Task RestoreAsync(RecordingFeedbackSession session, CancellationToken cancellationToken)
    {
        if (session.SystemMuteEnabled)
        {
            await TryAsync(() => systemAudioFeedback.RestoreAsync(session.AudioResumptionDelay, cancellationToken))
                .ConfigureAwait(false);
        }

        if (session.PauseMediaEnabled)
        {
            await TryAsync(() => mediaPlaybackFeedback.ResumeAsync(session.AudioResumptionDelay, cancellationToken))
                .ConfigureAwait(false);
        }
    }

    private void StartBackgroundRestore(RecordingFeedbackSession session)
    {
        restoreCancellation?.Cancel();
        restoreCancellation?.Dispose();
        restoreCancellation = new CancellationTokenSource();
        restoreTask = RestoreAsync(session, restoreCancellation.Token);
    }

    private static void Try(Action action)
    {
        try
        {
            action();
        }
        catch
        {
        }
    }

    private static async Task TryAsync(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
        }
    }

    private sealed record RecordingFeedbackSession(
        bool SoundFeedbackEnabled,
        bool SystemMuteEnabled,
        bool PauseMediaEnabled,
        TimeSpan AudioResumptionDelay)
    {
        public static RecordingFeedbackSession From(AppSettings settings) =>
            new(
                settings.IsSoundFeedbackEnabled,
                settings.IsSystemMuteEnabled,
                settings.IsPauseMediaEnabled,
                TimeSpan.FromSeconds(NormalizedDelaySeconds(settings.AudioResumptionDelaySeconds)));

        private static double NormalizedDelaySeconds(double delaySeconds) =>
            double.IsFinite(delaySeconds)
                ? Math.Clamp(delaySeconds, 0, 5)
                : 0;
    }
}
