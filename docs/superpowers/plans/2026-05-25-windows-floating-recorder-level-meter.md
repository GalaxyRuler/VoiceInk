# Windows Floating Recorder Level Meter Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the Windows floating recorder's purely decorative pulse with a real microphone level meter while recording, matching the macOS mini recorder's live waveform intent.

**Architecture:** Core owns testable PCM16 level calculation and recorder state carries an input level. Native capture raises normalized input-level events from the same NAudio buffers already being written to disk. WinUI stores the latest level and updates the floating recorder bars on the existing dispatcher refresh path without making level metering part of dictation control flow.

**Tech Stack:** .NET 10, WinUI 3, NAudio `WaveInEvent.DataAvailable`, xUnit.

---

### Task 1: Core Level Calculation And Recorder State

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioInputLevel.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioLevelMeter.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/FloatingRecorderViewState.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/FloatingRecorderPresenter.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Audio/AudioLevelMeterTests.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Recorder/FloatingRecorderPresenterTests.cs`

- [x] **Step 1: Write failing tests**

Cover:

- Empty buffers return level `0`.
- PCM16 silence returns `0`.
- Half-scale positive/negative samples return approximately `0.5`.
- Full-scale samples clamp to `1`.
- Odd trailing bytes are ignored.
- Recording state carries the supplied input level; non-recording states use `0`.

- [x] **Step 2: Verify tests fail**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "AudioLevelMeterTests|FloatingRecorderPresenterTests"
```

Expected: compile/test failure because `AudioLevelMeter`, `AudioInputLevel`, and recorder `InputLevel` do not exist yet.

- [x] **Step 3: Implement Core**

Add:

- `AudioInputLevel` record with a normalized `Peak` value clamped to `[0, 1]`.
- `AudioLevelMeter.CalculatePcm16Peak(byte[] buffer, int bytesRecorded)` that reads little-endian 16-bit signed samples and returns normalized peak.
- `FloatingRecorderViewState.InputLevel`.
- `FloatingRecorderPresenter.FromState(..., double inputLevel = 0)` that includes input level only in `Recording` state.

- [x] **Step 4: Verify focused Core tests pass**

Run the same filtered command. Expected: all focused tests pass.

### Task 2: Native Capture Level Events

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Audio/NAudioCaptureService.cs`

- [x] **Step 1: Emit input levels from DataAvailable**

Use `AudioLevelMeter.CalculatePcm16Peak(args.Buffer, args.BytesRecorded)` inside `OnDataAvailable` and raise `LevelAvailable` after writing the buffer. Do not block recording if level subscribers throw.

- [x] **Step 2: Verify native build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds.

### Task 3: Floating Recorder UI Wiring And Docs

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Wire latest level into recorder state**

In `MainWindow.xaml.cs`, subscribe to `audioCapture.LevelAvailable` when creating capture services, keep a `latestRecordingInputLevel` field, reset it when recording stops/cancels, and pass it to `FloatingRecorderPresenter.FromState`.

- [x] **Step 2: Render bars from input level**

In `FloatingRecorderWindow`, replace the fixed three-bar decorative pulse with five bars. When `InputLevel > 0`, scale bars by level with small offsets so speech produces visible motion. When not recording or level is zero, keep the existing processing pulse.

- [x] **Step 3: Update docs and completion bar**

Document that the compact Windows recorder now uses live local microphone level data during recording. Live transcript, non-activating stop/cancel controls, prompt picker, Power Mode popover, and notch style remain later recorder work.

- [ ] **Step 4: Verify, review, and commit**

Run focused tests, full tests, and Debug x64 build. Request code review and fix Critical/Important findings. Commit as:

```powershell
git add VoiceInk.Windows README.md docs\superpowers
git commit -m "feat(windows): add recorder level meter"
```
