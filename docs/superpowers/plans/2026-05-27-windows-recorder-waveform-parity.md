# Windows Recorder Waveform Parity Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:test-driven-development. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the Windows recorder's five hard-coded meter bars with a macOS-aligned, 15-bar testable waveform presenter.

### Task 1: Core Waveform Presenter

**Files:**

- `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/FloatingRecorderWaveformPresenter.cs`
- `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Recorder/FloatingRecorderWaveformPresenterTests.cs`

- [x] **Step 1: Write failing tests**

Cover 15 bars, macOS-aligned height bounds, center weighting, silent pulse behavior, and invalid input clamping.

- [x] **Step 2: Run focused tests red**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~FloatingRecorderWaveformPresenterTests"
```

- [x] **Step 3: Implement presenter**

Add a small deterministic presenter that computes bar height/opacity from `inputLevel` and `animationStep`.

- [x] **Step 4: Run focused tests green**

Run the same focused command and confirm the new tests pass.

### Task 2: WinUI Rendering

**Files:**

- `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml`
- `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml.cs`

- [x] **Step 1: Replace hard-coded bar fields**

Use a named stack panel and create the 15 waveform bars in code-behind.

- [x] **Step 2: Render presenter output**

Call the Core waveform presenter from `ApplyMeter` and apply each bar's height/opacity.

- [x] **Step 3: Build app project**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj -c Debug -p:Platform=x64
```

### Task 3: Docs, Review, Commit

**Files:**

- `docs/superpowers/project-completion.md`
- `docs/superpowers/specs/2026-05-27-windows-recorder-waveform-parity.md`
- `docs/superpowers/plans/2026-05-27-windows-recorder-waveform-parity.md`

- [x] **Step 1: Update tracker**

Mark recorder waveform parity as the current completed slice and increase the floating recorder area.

- [x] **Step 2: Run verification**

Run focused Core tests, app project build, full solution tests, Debug x64 build, and `git diff --check`.

- [x] **Step 3: Commit**

Commit with:

```text
feat(windows): refine floating recorder waveform
```
