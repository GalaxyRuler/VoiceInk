# Windows Floating Recorder Controls Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Stop and Cancel controls to the floating Windows recorder without activating the recorder window, preserving the dictation paste target while giving users macOS-style recorder controls.

**Architecture:** Core already exposes recorder command availability in `FloatingRecorderViewState`. WinUI will bind those states to compact icon-like controls and route clicks through callbacks owned by `MainWindow`. The floating recorder window will install a small Win32 subclass that returns `MA_NOACTIVATE` for `WM_MOUSEACTIVATE`, and it will keep using `AppWindow.Show(activateWindow: false)` so clicks do not steal foreground focus.

**Tech Stack:** .NET 10, WinUI 3, Windows App SDK `AppWindow.Show(false)`, Win32 `WM_MOUSEACTIVATE`, xUnit.

---

### Task 1: Core Recorder Command State Coverage

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Recorder/FloatingRecorderPresenterTests.cs`

- [x] **Step 1: Add tests**

Add/confirm tests that:

- Recording state exposes `CanStop = true` and `CanCancel = true`.
- Recording with an active operation disables both controls.
- Transcribing/inserting/idle/error states disable both controls.

- [x] **Step 2: Run focused tests**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FloatingRecorderPresenterTests"
```

Expected: tests pass because command state already exists.

### Task 2: Floating Window Non-Activating Controls

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml.cs`

- [x] **Step 1: Add controls**

Add compact Stop and Cancel buttons to the recorder body. They should be disabled unless `FloatingRecorderViewState.CanStop` / `CanCancel` are true. Keep Prompt and Power controls disabled as future affordances.

- [x] **Step 2: Add callbacks**

Expose `Func<Task>? StopRequested` and `Func<Task>? CancelRequested` on `FloatingRecorderWindow`. Click handlers should call the callbacks asynchronously and disable only when the view state disallows them.

- [x] **Step 3: Add no-activate hook**

Install a Win32 subclass with `SetWindowSubclass` after the window handle exists. On `WM_MOUSEACTIVATE`, return `MA_NOACTIVATE` so clicks are processed but the floating window does not activate. Remove the subclass on `Closed`.

### Task 3: MainWindow Wiring, Docs, Verification, Commit

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Wire callbacks**

When creating the floating recorder, set callbacks to `StopCurrentRecordingAsync` and `CancelCurrentRecordingAsync`. Do not call `Activate()` from the floating recorder path.

- [x] **Step 2: Update docs**

Document that the compact recorder now has non-activating Stop and Cancel controls. Keep live partial transcript, notch style, prompt picker behavior, and Power Mode controls as later work.

- [x] **Step 3: Verify and review**

Run focused tests, Debug x64 build, request review, then full solution tests and build after fixes.

- [x] **Step 4: Commit**

```powershell
git add VoiceInk.Windows README.md docs\superpowers
git commit -m "feat(windows): add floating recorder controls"
```
