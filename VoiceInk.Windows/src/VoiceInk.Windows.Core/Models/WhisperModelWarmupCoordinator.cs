using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Core.Models;

public sealed class WhisperModelWarmupCoordinator
{
    private readonly IWhisperModelWarmupService warmupService;
    private readonly Func<string, bool> modelExists;
    private readonly Func<DateTimeOffset> now;
    private readonly object gate = new();
    private Task? activeWarmupTask;
    private CancellationTokenSource? activeCancellation;

    public WhisperModelWarmupCoordinator(
        IWhisperModelWarmupService warmupService,
        Func<string, bool>? modelExists = null,
        Func<DateTimeOffset>? now = null)
    {
        this.warmupService = warmupService;
        this.modelExists = modelExists ?? File.Exists;
        this.now = now ?? (() => DateTimeOffset.Now);
    }

    public event EventHandler<WhisperModelWarmupState>? StateChanged;

    public WhisperModelWarmupState State { get; private set; } = WhisperModelWarmupState.Idle;

    public WhisperModelWarmupStartResult TryStart(
        AppSettings settings,
        string trigger,
        CancellationToken cancellationToken)
    {
        var trimmedTrigger = string.IsNullOrWhiteSpace(trigger) ? "warmup" : trigger.Trim();
        var skip = SkipReason(settings);
        if (skip is not null)
        {
            var skippedState = new WhisperModelWarmupState(
                WhisperModelWarmupStatus.Skipped,
                settings.ModelPath.Trim(),
                DisplayNameFor(settings.ModelPath),
                trimmedTrigger,
                skip);
            SetState(skippedState);
            return new WhisperModelWarmupStartResult(false, skippedState.Status, skippedState.Message);
        }

        cancellationToken.ThrowIfCancellationRequested();

        lock (gate)
        {
            if (activeWarmupTask is { IsCompleted: false })
            {
                return new WhisperModelWarmupStartResult(
                    false,
                    WhisperModelWarmupStatus.Warming,
                    $"Model warmup already in progress for {State.DisplayName}.");
            }

            var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            activeCancellation = linkedCancellation;
            var options = TranscriptionConfiguration.BuildOptions(settings, prompt: string.Empty);
            var startedAt = now();
            var displayName = DisplayNameFor(options.ModelPath);
            var warmingState = new WhisperModelWarmupState(
                WhisperModelWarmupStatus.Warming,
                options.ModelPath,
                displayName,
                trimmedTrigger,
                $"Warming up {displayName}",
                StartedAt: startedAt);
            SetState(warmingState);

            activeWarmupTask = Task.Run(
                () => RunWarmupAsync(options, warmingState, linkedCancellation),
                CancellationToken.None);
            return new WhisperModelWarmupStartResult(
                true,
                warmingState.Status,
                warmingState.Message,
                activeWarmupTask);
        }
    }

    public async Task CancelCurrentAsync()
    {
        Task? task;
        lock (gate)
        {
            activeCancellation?.Cancel();
            task = activeWarmupTask;
        }

        if (task is null)
        {
            return;
        }

        try
        {
            await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task RunWarmupAsync(
        TranscriptionOptions options,
        WhisperModelWarmupState startedState,
        CancellationTokenSource cancellation)
    {
        try
        {
            await warmupService.WarmupAsync(options, cancellation.Token).ConfigureAwait(false);
            var completedAt = now();
            SetState(startedState with
            {
                Status = WhisperModelWarmupStatus.Succeeded,
                Message = $"Model warmup completed for {startedState.DisplayName}",
                CompletedAt = completedAt,
                Duration = completedAt - startedState.StartedAt
            });
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            var completedAt = now();
            SetState(startedState with
            {
                Status = WhisperModelWarmupStatus.Canceled,
                Message = $"Model warmup canceled for {startedState.DisplayName}",
                CompletedAt = completedAt,
                Duration = completedAt - startedState.StartedAt
            });
        }
        catch (Exception ex)
        {
            var completedAt = now();
            SetState(startedState with
            {
                Status = WhisperModelWarmupStatus.Failed,
                Message = $"Model warmup failed for {startedState.DisplayName}: {ex.Message}",
                CompletedAt = completedAt,
                Duration = completedAt - startedState.StartedAt
            });
        }
        finally
        {
            lock (gate)
            {
                if (ReferenceEquals(activeCancellation, cancellation))
                {
                    activeWarmupTask = null;
                    activeCancellation = null;
                }
            }

            cancellation.Dispose();
        }
    }

    private string? SkipReason(AppSettings settings)
    {
        if (!settings.PrewarmModelOnWake)
        {
            return "Model prewarm is disabled.";
        }

        if (settings.TranscriptionProvider != TranscriptionProviderKind.LocalWhisper)
        {
            return "Model warmup only applies to local Whisper models.";
        }

        var modelPath = settings.ModelPath.Trim();
        if (string.IsNullOrWhiteSpace(modelPath))
        {
            return "Local whisper model path is required for warmup.";
        }

        return modelExists(modelPath)
            ? null
            : "Local whisper model file was not found.";
    }

    private void SetState(WhisperModelWarmupState state)
    {
        lock (gate)
        {
            State = state;
        }

        StateChanged?.Invoke(this, state);
    }

    private static string DisplayNameFor(string modelPath)
    {
        var trimmed = modelPath.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return "No model";
        }

        var fileName = Path.GetFileNameWithoutExtension(trimmed);
        return string.IsNullOrWhiteSpace(fileName) ? trimmed : fileName;
    }
}
