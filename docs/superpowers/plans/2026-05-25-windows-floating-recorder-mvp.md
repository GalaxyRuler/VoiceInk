# Windows Floating Recorder MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a Windows mini floating recorder window that mirrors the macOS mini recorder's compact recording/processing presence while staying source-runnable and tied to the existing dictation controller.

**Architecture:** Core owns a testable `FloatingRecorderPresenter` that maps `DictationState` plus elapsed/status inputs to display state, title, detail, visibility, and command availability. WinUI owns a separate `FloatingRecorderWindow` that renders those states and delegates stop/cancel commands back to `MainWindow`. The first slice does not invent live partial transcription or real audio metering; it uses a timer-driven pulse as a visible recording/processing indicator until the capture/transcription stack exposes live data.

**Tech Stack:** .NET 10, WinUI 3 secondary `Window`, Windows App SDK `AppWindow`, `DispatcherQueueTimer`, xUnit.

---

## Files

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/FloatingRecorderViewState.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/FloatingRecorderPresenter.cs`.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Recorder/FloatingRecorderPresenterTests.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`.
- Modify `README.md`.
- Modify `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [ ] **Step 1: Update parity spec**

Record the macOS source behavior:

- `MiniRecorderPanel` is a floating bottom-centered panel.
- `MiniRecorderView` shows compact black recorder controls, recording/processing status, prompt and Power Mode affordances, and optional live transcript expansion.
- `NotchRecorderView` expands/collapses around recording/transcribing/enhancing states.

Record the Windows adaptation for this slice:

- Implement a compact always-on-top floating mini-recorder window.
- Show it while recording, transcribing, inserting, or while an explicit operation status is active.
- Include status title, detail text, elapsed timer, animated pulse bars, Stop and Cancel buttons during recording, and disabled Prompt/Power Mode affordance labels for design continuity.
- Leave true live partial transcript, real waveform metering, notch style, prompt picker, and Power Mode button behavior as later slices.

- [ ] **Step 2: Commit planning docs**

Run:

```powershell
git add docs\superpowers\plans\2026-05-25-windows-floating-recorder-mvp.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "docs(windows): plan floating recorder"
```

Expected: docs-only commit.

## Task 2: Red Tests

- [ ] **Step 1: Add presenter tests**

Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Recorder/FloatingRecorderPresenterTests.cs`:

```csharp
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
}
```

- [ ] **Step 2: Run red tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~FloatingRecorderPresenterTests
```

Expected: compile failure because `VoiceInk.Windows.Core.Recorder` types do not exist yet.

## Task 3: Core Presenter

- [ ] **Step 1: Add view-state record**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/FloatingRecorderViewState.cs`:

```csharp
namespace VoiceInk.Windows.Core.Recorder;

public sealed record FloatingRecorderViewState(
    bool IsVisible,
    string Title,
    string Detail,
    string Elapsed,
    bool CanStop,
    bool CanCancel,
    bool ShowPulse);
```

- [ ] **Step 2: Add presenter**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/FloatingRecorderPresenter.cs`:

```csharp
using VoiceInk.Windows.Core.Dictation;

namespace VoiceInk.Windows.Core.Recorder;

public static class FloatingRecorderPresenter
{
    public static FloatingRecorderViewState FromState(
        DictationState state,
        TimeSpan elapsed,
        string? status,
        bool isOperationActive)
    {
        return state switch
        {
            DictationState.Recording => new(
                true,
                "Recording",
                "Listening",
                FormatElapsed(elapsed),
                CanStop: true,
                CanCancel: true,
                ShowPulse: true),
            DictationState.Transcribing => new(
                true,
                "Transcribing",
                "Processing speech",
                FormatElapsed(elapsed),
                CanStop: false,
                CanCancel: false,
                ShowPulse: true),
            DictationState.Inserting => new(
                true,
                "Inserting",
                "Inserting text",
                FormatElapsed(elapsed),
                CanStop: false,
                CanCancel: false,
                ShowPulse: true),
            DictationState.Error => new(
                true,
                "Error",
                status ?? "Recording failed",
                FormatElapsed(elapsed),
                CanStop: false,
                CanCancel: false,
                ShowPulse: false),
            _ when isOperationActive => new(
                true,
                string.IsNullOrWhiteSpace(status) ? "Working" : status,
                "VoiceInk is busy",
                FormatElapsed(elapsed),
                CanStop: false,
                CanCancel: false,
                ShowPulse: true),
            _ => new(false, "Idle", status ?? "Idle", FormatElapsed(TimeSpan.Zero), false, false, false)
        };
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        var safeElapsed = elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
        var totalMinutes = (int)safeElapsed.TotalMinutes;
        return $"{totalMinutes:00}:{safeElapsed.Seconds:00}";
    }
}
```

- [ ] **Step 3: Verify focused tests**

Run the focused test command from Task 2. Expected: presenter tests pass.

## Task 4: WinUI Floating Recorder Window

- [ ] **Step 1: Add floating recorder XAML**

Create `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml` with:

- A black rounded `Border` root sized around `320x128`.
- `TitleTextBlock`, `DetailTextBlock`, `ElapsedTextBlock`.
- Three pulse bars named `PulseBar1`, `PulseBar2`, `PulseBar3`.
- Disabled continuity buttons/labels `PromptButton` and `PowerModeButton`.
- `StopFloatingRecorderButton` and `CancelFloatingRecorderButton` wired to code-behind.

- [ ] **Step 2: Add floating recorder code-behind**

Create `FloatingRecorderWindow.xaml.cs`:

- Constructor accepts `Func<Task> stopRequested` and `Func<Task> cancelRequested`.
- Configure window size to `320x128`.
- Use `OverlappedPresenter` to set `IsAlwaysOnTop = true`, `IsResizable = false`, `IsMaximizable = false`, `IsMinimizable = false` where available.
- Hide the title bar with `ExtendsContentIntoTitleBar = true`.
- Position bottom center of the display area using `DisplayArea.GetFromWindowId(...)`.
- Use `DispatcherQueueTimer` every 180ms to animate pulse bar opacity/height.
- `Apply(FloatingRecorderViewState state)` updates text, button enabled states, pulse visibility, and shows/hides the native window.

- [ ] **Step 3: Wire from MainWindow**

Modify `MainWindow.xaml.cs`:

- Add fields `FloatingRecorderWindow? floatingRecorderWindow; DateTimeOffset? recordingStartedAt; string? floatingRecorderStatus;`.
- Set `recordingStartedAt = DateTimeOffset.Now` after successful `controller.StartAsync`.
- Clear `recordingStartedAt` after stop/cancel finishes and when idle/error without active operation.
- Add `EnsureFloatingRecorderWindow()`.
- In `RefreshUiFromControllerState`, after `displayStatus` is computed, call `UpdateFloatingRecorder(displayStatus, operationActive)`.
- `UpdateFloatingRecorder` builds a `FloatingRecorderPresenter.FromState(...)` state and applies it.
- Use existing `StopCurrentRecordingAsync` and `CancelCurrentRecordingAsync` for floating window commands.

- [ ] **Step 4: Verify app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build passes with 0 warnings/errors.

## Task 5: Docs, Review, Commit

- [ ] **Step 1: Update README/spec**

Document:

- Windows now has a compact floating mini-recorder during recording/processing.
- It includes state text, elapsed timer, pulse animation, stop/cancel controls, and prompt/Power Mode affordances.
- Live partial transcript, real waveform/level metering, notch style, prompt picker, and Power Mode behavior remain future gaps.

- [ ] **Step 2: Full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: all tests pass and Debug x64 build passes with 0 warnings/errors.

- [ ] **Step 3: Request review and fix findings**

Request subagent review against this plan and the macOS recorder source. Fix all Critical and Important findings before committing.

- [ ] **Step 4: Commit**

Run:

```powershell
git add README.md `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Recorder\FloatingRecorderViewState.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Recorder\FloatingRecorderPresenter.cs `
  VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\Recorder\FloatingRecorderPresenterTests.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.App\FloatingRecorderWindow.xaml `
  VoiceInk.Windows\src\VoiceInk.Windows.App\FloatingRecorderWindow.xaml.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.App\MainWindow.xaml.cs `
  docs\superpowers\plans\2026-05-25-windows-floating-recorder-mvp.md `
  docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "feat(windows): add floating recorder window"
```
