using VoiceInk.Windows.Core.Recording;
using VoiceInk.Windows.Core.Settings;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Recording;

public sealed class RecordingFeedbackCoordinatorTests
{
    [Fact]
    public async Task BeginAndCompleteAsync_UsesEnabledSettingsAndSessionDelay()
    {
        var sound = new FakeRecordingSoundFeedback();
        var systemAudio = new FakeSystemAudioFeedback();
        var media = new FakeMediaPlaybackFeedback();
        var coordinator = new RecordingFeedbackCoordinator(sound, systemAudio, media);
        var settings = new AppSettings
        {
            IsSoundFeedbackEnabled = true,
            IsSystemMuteEnabled = true,
            IsPauseMediaEnabled = true,
            AudioResumptionDelaySeconds = 2
        };

        await coordinator.BeginAsync(settings, CancellationToken.None);
        await coordinator.CaptureStoppedAsync(CancellationToken.None);
        await coordinator.CompleteAsync(playStopSound: true, CancellationToken.None);

        Assert.Equal(["start", "stop"], sound.Events);
        Assert.Equal(1, systemAudio.MuteCount);
        Assert.Equal([TimeSpan.FromSeconds(2)], systemAudio.RestoreDelays);
        Assert.Equal(1, media.PauseCount);
        Assert.Equal([TimeSpan.FromSeconds(2)], media.ResumeDelays);
    }

    [Fact]
    public async Task CompleteAsync_UsesSettingsSnapshotCapturedAtBegin()
    {
        var sound = new FakeRecordingSoundFeedback();
        var systemAudio = new FakeSystemAudioFeedback();
        var media = new FakeMediaPlaybackFeedback();
        var coordinator = new RecordingFeedbackCoordinator(sound, systemAudio, media);
        var settings = new AppSettings
        {
            IsSoundFeedbackEnabled = true,
            IsSystemMuteEnabled = true,
            IsPauseMediaEnabled = true,
            AudioResumptionDelaySeconds = 3,
            StartSoundMode = RecordingSoundModeSettings.Custom,
            CustomStartSoundPath = @"C:\VoiceInk\Sounds\CustomStartSound.wav",
            StopSoundMode = RecordingSoundModeSettings.Custom,
            CustomStopSoundPath = @"C:\VoiceInk\Sounds\CustomStopSound.wav"
        };

        await coordinator.BeginAsync(settings, CancellationToken.None);
        await coordinator.BeginAsync(settings with
        {
            IsSoundFeedbackEnabled = false,
            IsSystemMuteEnabled = false,
            IsPauseMediaEnabled = false,
            AudioResumptionDelaySeconds = 0,
            StartSoundMode = RecordingSoundModeSettings.SystemDefault,
            CustomStartSoundPath = string.Empty,
            StopSoundMode = RecordingSoundModeSettings.SystemDefault,
            CustomStopSoundPath = string.Empty
        }, CancellationToken.None);
        await coordinator.CaptureStoppedAsync(CancellationToken.None);
        await coordinator.CompleteAsync(playStopSound: true, CancellationToken.None);

        Assert.Equal(["start", "stop"], sound.Events);
        Assert.Equal(RecordingSoundModeSettings.Custom, sound.StartSettings.Single().Mode);
        Assert.Equal(@"C:\VoiceInk\Sounds\CustomStartSound.wav", sound.StartSettings.Single().CustomSoundPath);
        Assert.Equal(RecordingSoundModeSettings.Custom, sound.StopSettings.Single().Mode);
        Assert.Equal(@"C:\VoiceInk\Sounds\CustomStopSound.wav", sound.StopSettings.Single().CustomSoundPath);
        Assert.Equal(1, systemAudio.MuteCount);
        Assert.Equal([TimeSpan.FromSeconds(3)], systemAudio.RestoreDelays);
        Assert.Equal(1, media.PauseCount);
        Assert.Equal([TimeSpan.FromSeconds(3)], media.ResumeDelays);
    }

    [Fact]
    public async Task CompleteAsync_DoesNotPlayStopSoundWhenInsertionDidNotComplete()
    {
        var sound = new FakeRecordingSoundFeedback();
        var systemAudio = new FakeSystemAudioFeedback();
        var media = new FakeMediaPlaybackFeedback();
        var coordinator = new RecordingFeedbackCoordinator(sound, systemAudio, media);

        await coordinator.BeginAsync(new AppSettings
        {
            IsSoundFeedbackEnabled = true,
            IsSystemMuteEnabled = true,
            IsPauseMediaEnabled = true,
            AudioResumptionDelaySeconds = 2
        }, CancellationToken.None);
        await coordinator.CaptureStoppedAsync(CancellationToken.None);
        await coordinator.CompleteAsync(playStopSound: false, CancellationToken.None);

        Assert.Equal(["start"], sound.Events);
        Assert.Equal([TimeSpan.FromSeconds(2)], systemAudio.RestoreDelays);
        Assert.Equal([TimeSpan.FromSeconds(2)], media.ResumeDelays);
    }

    [Fact]
    public async Task CaptureStoppedAsync_DoesNotBlockOnDelayedRestore()
    {
        var systemAudio = new FakeSystemAudioFeedback
        {
            RestoreGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        var coordinator = new RecordingFeedbackCoordinator(
            new FakeRecordingSoundFeedback(),
            systemAudio,
            new FakeMediaPlaybackFeedback());

        await coordinator.BeginAsync(new AppSettings
        {
            IsSystemMuteEnabled = true,
            AudioResumptionDelaySeconds = 5
        }, CancellationToken.None);

        await coordinator.CaptureStoppedAsync(CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(1));

        Assert.Equal([TimeSpan.FromSeconds(5)], systemAudio.RestoreDelays);
        Assert.False(systemAudio.RestoreGate.Task.IsCompleted);
        systemAudio.RestoreGate.SetResult();
        await coordinator.RestorePendingImmediatelyAsync();
    }

    [Fact]
    public async Task CancelAsync_RestoresAudioAndMediaWithoutStopSound()
    {
        var sound = new FakeRecordingSoundFeedback();
        var systemAudio = new FakeSystemAudioFeedback();
        var media = new FakeMediaPlaybackFeedback();
        var coordinator = new RecordingFeedbackCoordinator(sound, systemAudio, media);
        var settings = new AppSettings
        {
            IsSoundFeedbackEnabled = true,
            IsSystemMuteEnabled = true,
            IsPauseMediaEnabled = true,
            AudioResumptionDelaySeconds = 1
        };

        await coordinator.BeginAsync(settings, CancellationToken.None);
        await coordinator.CancelAsync(CancellationToken.None);

        Assert.Equal(["start"], sound.Events);
        Assert.Equal([TimeSpan.FromSeconds(1)], systemAudio.RestoreDelays);
        Assert.Equal([TimeSpan.FromSeconds(1)], media.ResumeDelays);
    }

    [Fact]
    public async Task CancelImmediatelyAsync_RestoresAudioAndMediaWithoutDelay()
    {
        var sound = new FakeRecordingSoundFeedback();
        var systemAudio = new FakeSystemAudioFeedback();
        var media = new FakeMediaPlaybackFeedback();
        var coordinator = new RecordingFeedbackCoordinator(sound, systemAudio, media);
        var settings = new AppSettings
        {
            IsSoundFeedbackEnabled = true,
            IsSystemMuteEnabled = true,
            IsPauseMediaEnabled = true,
            AudioResumptionDelaySeconds = 5
        };

        await coordinator.BeginAsync(settings, CancellationToken.None);
        await coordinator.CancelImmediatelyAsync(CancellationToken.None);

        Assert.Equal(["start"], sound.Events);
        Assert.Equal([TimeSpan.Zero], systemAudio.RestoreDelays);
        Assert.Equal([TimeSpan.Zero], media.ResumeDelays);
    }

    [Fact]
    public async Task BeginAndCompleteAsync_DoesNothingForDisabledSettings()
    {
        var sound = new FakeRecordingSoundFeedback();
        var systemAudio = new FakeSystemAudioFeedback();
        var media = new FakeMediaPlaybackFeedback();
        var coordinator = new RecordingFeedbackCoordinator(sound, systemAudio, media);

        await coordinator.BeginAsync(new AppSettings
        {
            IsSoundFeedbackEnabled = false,
            IsSystemMuteEnabled = false,
            IsPauseMediaEnabled = false
        }, CancellationToken.None);
        await coordinator.CaptureStoppedAsync(CancellationToken.None);
        await coordinator.CompleteAsync(playStopSound: true, CancellationToken.None);

        Assert.Empty(sound.Events);
        Assert.Equal(0, systemAudio.MuteCount);
        Assert.Empty(systemAudio.RestoreDelays);
        Assert.Equal(0, media.PauseCount);
        Assert.Empty(media.ResumeDelays);
    }

    private sealed class FakeRecordingSoundFeedback : IRecordingSoundFeedback
    {
        public List<string> Events { get; } = [];
        public List<RecordingSoundPlaybackSettings> StartSettings { get; } = [];
        public List<RecordingSoundPlaybackSettings> StopSettings { get; } = [];

        public void PlayStartSound(RecordingSoundPlaybackSettings settings)
        {
            Events.Add("start");
            StartSettings.Add(settings);
        }

        public void PlayStopSound(RecordingSoundPlaybackSettings settings)
        {
            Events.Add("stop");
            StopSettings.Add(settings);
        }
    }

    private sealed class FakeSystemAudioFeedback : ISystemAudioFeedback
    {
        public int MuteCount { get; private set; }
        public List<TimeSpan> RestoreDelays { get; } = [];

        public Task MuteAsync(CancellationToken cancellationToken)
        {
            MuteCount++;
            return Task.CompletedTask;
        }

        public TaskCompletionSource? RestoreGate { get; init; }

        public async Task RestoreAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            RestoreDelays.Add(delay);
            if (RestoreGate is not null)
            {
                try
                {
                    await RestoreGate.Task.WaitAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                }
            }
        }
    }

    private sealed class FakeMediaPlaybackFeedback : IMediaPlaybackFeedback
    {
        public int PauseCount { get; private set; }
        public List<TimeSpan> ResumeDelays { get; } = [];

        public Task PauseAsync(CancellationToken cancellationToken)
        {
            PauseCount++;
            return Task.CompletedTask;
        }

        public Task ResumeAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            ResumeDelays.Add(delay);
            return Task.CompletedTask;
        }
    }
}
