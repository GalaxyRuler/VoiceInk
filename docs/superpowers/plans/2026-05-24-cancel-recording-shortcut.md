# Cancel Recording Shortcut Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a macOS-style Cancel Recording action to the Windows MVP for active recordings, preserving a canceled history row with the captured audio file and exposing the action from the shell and an optional global shortcut.

**Architecture:** Keep cancellation orchestration in `DictationController` so Core owns state transitions and history persistence. Reuse `IAudioCaptureService.StopAsync` to release microphone resources and keep the partial audio file, matching the macOS app's canceled transcription history behavior. Extend existing shortcut/settings registration instead of adding a separate hotkey path.

**Tech Stack:** .NET 10, WinUI 3, NAudio capture, SQLite history, xUnit, repo-local .NET 10 SDK.

---

## Files

- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/TranscriptionHistoryItem.cs` for the shared canceled text constant.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs` to add `CancelAsync`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs` for `CancelRecordingHotkey`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcutAction.cs` and `GlobalShortcutSettings.cs` for the cancel action.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictation/DictationControllerTests.cs` for cancel behavior.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shortcuts/GlobalShortcutTests.cs` and `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs` for shortcut/settings behavior.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml` and `MainWindow.xaml.cs` to add shell controls and hotkey routing.
- Modify `README.md` and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [ ] **Step 1: Mark cancel recording as the active slice**

Update the parity spec status with: active slice is cancel recording, matching macOS canceled history behavior for active recorder cancellation.

- [ ] **Step 2: Commit the plan**

Run:

```powershell
git add docs\superpowers\plans\2026-05-24-cancel-recording-shortcut.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "docs(windows): plan cancel recording shortcut"
```

Expected: a docs-only commit.

## Task 2: Red Tests

- [ ] **Step 1: Add DictationController cancel tests**

Add tests proving `CancelAsync`:

- Stops active capture.
- Saves a `Canceled` history row with text `The transcription was canceled.`, audio duration, model/language metadata, and audio file path.
- Does not transcribe or insert text.
- Does nothing when not recording.

- [ ] **Step 2: Add shortcut/settings tests**

Update shortcut registration tests to include `CancelRecordingHotkey = "Ctrl+Alt+C"` and assert `GlobalShortcutAction.CancelRecording`. Add a duplicate assignment test expecting `Cancel Recording already uses Ctrl+Alt+Space.`. Update the JSON settings round-trip test to persist `CancelRecordingHotkey`.

- [ ] **Step 3: Run red tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~DictationControllerTests|FullyQualifiedName~GlobalShortcutTests"
```

Expected: compile failure because `CancelAsync`, `CancelRecordingHotkey`, and `CancelRecording` do not exist.

## Task 3: Core Implementation

- [ ] **Step 1: Add history text constant**

Add `public const string CanceledTranscriptionText = "The transcription was canceled.";` to `TranscriptionHistoryItem`.

- [ ] **Step 2: Add `DictationController.CancelAsync`**

When state is `Recording`, acquire the lifecycle gate, stop capture with `CancellationToken.None`, honor caller cancellation after resources release, load settings, save a canceled history row, set state to `Idle`, and set `LastWarning` if history persistence fails.

- [ ] **Step 3: Add settings and shortcut action**

Add `CancelRecordingHotkey` to `AppSettings`, `CancelRecording` to `GlobalShortcutAction`, and an optional registration in `GlobalShortcutSettings.BuildRegistrations` with display name `Cancel Recording`.

- [ ] **Step 4: Run focused tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~DictationControllerTests|FullyQualifiedName~GlobalShortcutTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter FullyQualifiedName~JsonSettingsStoreTests
```

Expected: selected tests pass.

## Task 4: WinUI Wiring

- [ ] **Step 1: Add controls**

Add a `CancelHotkeyTextBox` under Shortcuts and a `CancelButton` next to Stop in the recording command row.

- [ ] **Step 2: Route commands**

Load/save `CancelRecordingHotkey`, route `GlobalShortcutAction.CancelRecording` to `CancelCurrentRecordingAsync`, and add `CancelButton_Click`.

- [ ] **Step 3: Implement shell cancel method**

`CancelCurrentRecordingAsync` should guard against concurrent operations, call `controller.CancelAsync`, refresh history, and report `Recording canceled` unless the controller exposes a warning.

- [ ] **Step 4: Verify app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build passes with 0 warnings/errors.

## Task 5: Review, Docs, Commit

- [ ] **Step 1: Update README and parity spec**

Mark cancel recording shell/shortcut/canceled-history behavior as implemented. Keep canceling in-flight transcription/enhancement listed as a later gap.

- [ ] **Step 2: Full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: full tests and build pass.

- [ ] **Step 3: Request code review**

Ask for review of the cancel slice. Fix all Critical and Important findings.

- [ ] **Step 4: Commit**

Run:

```powershell
git add README.md VoiceInk.Windows docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "feat(windows): add cancel recording shortcut"
```

Expected: commit succeeds; only `.superpowers/` remains untracked.
