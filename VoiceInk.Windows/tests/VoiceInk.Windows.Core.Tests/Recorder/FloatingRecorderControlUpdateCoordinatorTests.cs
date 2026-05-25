using VoiceInk.Windows.Core.Recorder;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Recorder;

public sealed class FloatingRecorderControlUpdateCoordinatorTests
{
    [Fact]
    public async Task WaitForPendingUpdateAsync_WaitsForActiveUpdate()
    {
        var coordinator = new FloatingRecorderControlUpdateCoordinator();
        var updateStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var allowUpdateToFinish = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var updateFinished = false;

        var updateTask = coordinator.RunUpdateAsync(
            async cancellationToken =>
            {
                updateStarted.SetResult();
                await allowUpdateToFinish.Task.WaitAsync(cancellationToken);
                updateFinished = true;
            },
            CancellationToken.None);
        await updateStarted.Task.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.True(coordinator.IsUpdating);

        var waitTask = coordinator.WaitForPendingUpdateAsync(CancellationToken.None);
        await Task.Delay(25);

        Assert.False(waitTask.IsCompleted);
        Assert.False(updateFinished);

        allowUpdateToFinish.SetResult();
        await waitTask.WaitAsync(TimeSpan.FromSeconds(1));
        await updateTask.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.True(updateFinished);
        Assert.False(coordinator.IsUpdating);
    }

    [Fact]
    public async Task RunUpdateAsync_IgnoresConcurrentUpdateUntilCurrentUpdateCompletes()
    {
        var coordinator = new FloatingRecorderControlUpdateCoordinator();
        var allowFirstUpdateToFinish = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var runs = 0;

        var firstTask = coordinator.RunUpdateAsync(
            async cancellationToken =>
            {
                runs++;
                await allowFirstUpdateToFinish.Task.WaitAsync(cancellationToken);
            },
            CancellationToken.None);
        await Task.Delay(25);

        var secondTask = coordinator.RunUpdateAsync(
            _ =>
            {
                runs++;
                return Task.CompletedTask;
            },
            CancellationToken.None);

        Assert.Same(firstTask, secondTask);
        Assert.Equal(1, runs);

        allowFirstUpdateToFinish.SetResult();
        await firstTask.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.False(coordinator.IsUpdating);
        Assert.Equal(1, runs);
    }
}
