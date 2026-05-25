using VoiceInk.Windows.Core.Dictation;
using VoiceInk.Windows.Core.Recorder;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Recorder;

public sealed class FloatingRecorderPresenterTests
{
    [Fact]
    public void FromState_Recording_ShowsElapsedAndRecordingCommands()
    {
        var state = FloatingRecorderPresenter.FromState(
            DictationState.Recording,
            TimeSpan.FromSeconds(65),
            "Recording",
            isOperationActive: false);

        Assert.True(state.IsVisible);
        Assert.Equal("Recording", state.Title);
        Assert.Equal("01:05", state.Elapsed);
        Assert.True(state.CanStop);
        Assert.True(state.CanCancel);
        Assert.True(state.ShowPulse);
        Assert.Equal(0, state.InputLevel);
    }

    [Fact]
    public void FromState_Recording_CarriesInputLevel()
    {
        var state = FloatingRecorderPresenter.FromState(
            DictationState.Recording,
            TimeSpan.FromSeconds(12),
            "Recording",
            isOperationActive: false,
            inputLevel: 0.72);

        Assert.Equal(0.72, state.InputLevel);
    }

    [Theory]
    [InlineData(DictationState.Transcribing, "Transcribing", "Processing speech")]
    [InlineData(DictationState.Inserting, "Inserting", "Inserting text")]
    public void FromState_ProcessingStates_ShowProcessingWithoutRecordingCommands(
        DictationState dictationState,
        string expectedTitle,
        string expectedDetail)
    {
        var state = FloatingRecorderPresenter.FromState(
            dictationState,
            TimeSpan.FromSeconds(5),
            null,
            isOperationActive: false);

        Assert.True(state.IsVisible);
        Assert.Equal(expectedTitle, state.Title);
        Assert.Equal(expectedDetail, state.Detail);
        Assert.False(state.CanStop);
        Assert.False(state.CanCancel);
        Assert.True(state.ShowPulse);
        Assert.Equal(0, state.InputLevel);
    }

    [Theory]
    [InlineData(DictationState.Idle)]
    [InlineData(DictationState.Error)]
    [InlineData(DictationState.Transcribing)]
    [InlineData(DictationState.Inserting)]
    public void FromState_NonRecordingStates_DisableRecorderCommands(DictationState dictationState)
    {
        var state = FloatingRecorderPresenter.FromState(
            dictationState,
            TimeSpan.FromSeconds(5),
            "Working",
            isOperationActive: false);

        Assert.False(state.CanStop);
        Assert.False(state.CanCancel);
    }

    [Fact]
    public void FromState_IdleWithoutOperation_HidesRecorder()
    {
        var state = FloatingRecorderPresenter.FromState(
            DictationState.Idle,
            TimeSpan.Zero,
            "Idle",
            isOperationActive: false);

        Assert.False(state.IsVisible);
        Assert.False(state.ShowPulse);
    }

    [Fact]
    public void FromState_ErrorWithoutOperation_HidesRecorder()
    {
        var state = FloatingRecorderPresenter.FromState(
            DictationState.Error,
            TimeSpan.Zero,
            "Model missing",
            isOperationActive: false);

        Assert.False(state.IsVisible);
        Assert.False(state.ShowPulse);
    }

    [Fact]
    public void FromState_OperationActive_ShowsStatusWithoutRecordingCommands()
    {
        var state = FloatingRecorderPresenter.FromState(
            DictationState.Idle,
            TimeSpan.Zero,
            "Starting recording",
            isOperationActive: true);

        Assert.True(state.IsVisible);
        Assert.Equal("Starting recording", state.Title);
        Assert.False(state.CanStop);
        Assert.False(state.CanCancel);
        Assert.True(state.ShowPulse);
    }

    [Fact]
    public void FromState_RecordingWithOperationActive_DisablesRecordingCommands()
    {
        var state = FloatingRecorderPresenter.FromState(
            DictationState.Recording,
            TimeSpan.FromSeconds(12),
            "Stopping and inserting",
            isOperationActive: true);

        Assert.True(state.IsVisible);
        Assert.Equal("Stopping and inserting", state.Detail);
        Assert.False(state.CanStop);
        Assert.False(state.CanCancel);
        Assert.True(state.ShowPulse);
    }

    [Fact]
    public void RefreshPolicy_RefreshesVisiblePulsingRecorderOnly()
    {
        var recording = FloatingRecorderPresenter.FromState(
            DictationState.Recording,
            TimeSpan.FromSeconds(12),
            "Recording",
            isOperationActive: false);
        var idle = FloatingRecorderPresenter.FromState(
            DictationState.Idle,
            TimeSpan.Zero,
            "Idle",
            isOperationActive: false);
        var error = FloatingRecorderPresenter.FromState(
            DictationState.Error,
            TimeSpan.Zero,
            "Model missing",
            isOperationActive: false);

        Assert.True(FloatingRecorderRefreshPolicy.ShouldRefresh(recording));
        Assert.False(FloatingRecorderRefreshPolicy.ShouldRefresh(idle));
        Assert.False(FloatingRecorderRefreshPolicy.ShouldRefresh(error));
    }

    [Fact]
    public void ActivityPolicy_ShowsRecorderOnlyForRecorderActivity()
    {
        Assert.True(FloatingRecorderActivityPolicy.ShouldShowForActivity(
            DictationState.Recording,
            isStarting: false,
            isStopping: false,
            isCanceling: false));
        Assert.True(FloatingRecorderActivityPolicy.ShouldShowForActivity(
            DictationState.Idle,
            isStarting: true,
            isStopping: false,
            isCanceling: false));
        Assert.True(FloatingRecorderActivityPolicy.ShouldShowForActivity(
            DictationState.Idle,
            isStarting: false,
            isStopping: true,
            isCanceling: false));
        Assert.True(FloatingRecorderActivityPolicy.ShouldShowForActivity(
            DictationState.Idle,
            isStarting: false,
            isStopping: false,
            isCanceling: true));

        Assert.False(FloatingRecorderActivityPolicy.ShouldShowForActivity(
            DictationState.Idle,
            isStarting: false,
            isStopping: false,
            isCanceling: false));
    }
}
