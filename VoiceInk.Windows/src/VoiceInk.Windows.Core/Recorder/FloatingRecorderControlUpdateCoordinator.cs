namespace VoiceInk.Windows.Core.Recorder;

public sealed class FloatingRecorderControlUpdateCoordinator
{
    private readonly object sync = new();
    private Task pendingUpdate = Task.CompletedTask;

    public bool IsUpdating
    {
        get
        {
            lock (sync)
            {
                return !pendingUpdate.IsCompleted;
            }
        }
    }

    public Task RunUpdateAsync(
        Func<CancellationToken, Task> updateAsync,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(updateAsync);

        TaskCompletionSource pendingSource;
        lock (sync)
        {
            if (!pendingUpdate.IsCompleted)
            {
                return pendingUpdate;
            }

            pendingSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            pendingUpdate = pendingSource.Task;
        }

        _ = CompleteUpdateAsync(updateAsync, cancellationToken, pendingSource);
        return pendingSource.Task;
    }

    public async Task WaitForPendingUpdateAsync(CancellationToken cancellationToken)
    {
        Task updateTask;
        lock (sync)
        {
            updateTask = pendingUpdate;
        }

        await updateTask.WaitAsync(cancellationToken);
    }

    private async Task CompleteUpdateAsync(
        Func<CancellationToken, Task> updateAsync,
        CancellationToken cancellationToken,
        TaskCompletionSource pendingSource)
    {
        try
        {
            await updateAsync(cancellationToken);
            pendingSource.TrySetResult();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            pendingSource.TrySetCanceled(cancellationToken);
        }
        catch (Exception ex)
        {
            pendingSource.TrySetException(ex);
        }
        finally
        {
            lock (sync)
            {
                if (ReferenceEquals(pendingUpdate, pendingSource.Task))
                {
                    pendingUpdate = Task.CompletedTask;
                }
            }
        }
    }
}
