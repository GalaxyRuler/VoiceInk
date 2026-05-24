# Windows Tray Shell Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Adapt the macOS menu-bar shell into a Windows tray icon with always-available VoiceInk commands, hide/show behavior, and no commercial menu surfaces.

**Architecture:** Core owns a small tray presenter that maps dictation/settings state into menu labels and enabled states. Native owns a `System.Windows.Forms.NotifyIcon` wrapper behind events so WinUI does not depend directly on tray implementation details. The WinUI shell wires tray events to existing recording, history, quick-add, and shutdown flows, and keeps the tray menu refreshed from the same state used by the main window.

**Tech Stack:** .NET 10, WinUI 3, Windows App SDK window show/hide interop, `System.Windows.Forms.NotifyIcon`, xUnit.

---

## Files

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/TrayShellState.cs` for tray menu state.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/TrayShellPresenter.cs` for deterministic state mapping.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shell/TrayShellPresenterTests.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/Tray/TrayIconService.cs` for the Windows tray icon and menu.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs` to instantiate the tray service, wire events, show/hide the shell, and dispose it.
- Modify `README.md` and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [ ] **Step 1: Mark tray shell active in the parity spec**

Update the shell/navigation section to record the macOS source behavior: menu-bar commands, menu-bar-only mode, reopen behavior, and app staying alive after windows close. Record the Windows adaptation: tray icon, show/hide shell command, toggle recording, quick add, open history, and quit; commercial updater/support items remain omitted.

- [ ] **Step 2: Commit the plan**

Run:

```powershell
git add docs\superpowers\plans\2026-05-24-windows-tray-shell.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "docs(windows): plan tray shell"
```

Expected: docs-only commit.

## Task 2: Red Tests

- [ ] **Step 1: Add tray presenter tests**

Create `TrayShellPresenterTests` proving:

- Idle/settings-loaded state enables toggle recording and labels it `Start Recording`.
- Recording state enables toggle recording and labels it `Stop Recording`.
- Busy transcribing state disables toggle recording, quick add, and open history while retaining a visible status.
- Settings-not-loaded state disables operational commands and uses `Loading settings` status.

- [ ] **Step 2: Run red tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~TrayShellPresenterTests
```

Expected: compile failure because `TrayShellPresenter` and `TrayShellState` do not exist yet.

## Task 3: Core Presenter

- [ ] **Step 1: Implement tray state record**

`TrayShellState` contains `StatusText`, `ToggleRecordingLabel`, `CanToggleRecording`, `CanQuickAddDictionary`, and `CanOpenHistory`.

- [ ] **Step 2: Implement presenter**

`TrayShellPresenter.FromState(bool settingsLoaded, DictationState dictationState, bool operationActive, string? statusOverride)` returns:

- `Loading settings` with operational commands disabled when settings are not loaded.
- `Start Recording` enabled when idle/error and no operation is active.
- `Stop Recording` enabled when recording and no operation is active.
- Quick Add enabled only when idle, settings are loaded, and no operation is active.
- Open History enabled when settings are loaded and no operation is active.
- Status text from `statusOverride` when supplied, otherwise the dictation-state display string.

- [ ] **Step 3: Verify focused tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~TrayShellPresenterTests
```

Expected: selected tests pass.

## Task 4: Native Tray Service

- [ ] **Step 1: Implement `TrayIconService`**

Use `NotifyIcon`, `ContextMenuStrip`, and `ToolStripMenuItem` in `VoiceInk.Windows.Native`. Expose events for `ShowRequested`, `HideRequested`, `ToggleRecordingRequested`, `QuickAddDictionaryRequested`, `OpenHistoryRequested`, and `ExitRequested`. Add menu items in this order:

- `Show VoiceInk`
- `Hide VoiceInk`
- separator
- dynamic toggle recording item
- `Quick Add to Dictionary`
- `History`
- separator
- `Quit VoiceInk`

The tray text should be `VoiceInk - <status>` truncated to the Windows notify-icon tooltip length. Use `Icon.ExtractAssociatedIcon(Environment.ProcessPath)` with `SystemIcons.Application` fallback so the source-built app has an icon without adding packaging assets yet.

- [ ] **Step 2: Keep updates idempotent**

`UpdateState(TrayShellState state)` updates menu labels/enabled states and tooltip text without recreating the icon. `Dispose()` hides and disposes all tray resources.

- [ ] **Step 3: Verify app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build passes.

## Task 5: WinUI Wiring, Docs, Review

- [ ] **Step 1: Wire tray lifecycle**

Instantiate `TrayIconService` in `MainWindow`, wire events to existing async methods, and call `UpdateTrayFromControllerState` from `RefreshUiFromControllerState`. `Show` restores/activates the window. `Hide` uses `ShowWindow(..., 0)` to hide the WinUI shell. `Exit` marks the window as exiting and calls `Close()`.

- [ ] **Step 2: Preserve tray availability**

Disposing the main window disposes the tray service, global hotkeys, audio capture, and cancellation token. The explicit hide command keeps the process alive and global shortcuts registered.

- [ ] **Step 3: Update README/spec**

Document that the Windows MVP now has a tray icon with show/hide, recording toggle, Quick Add, History, and Quit. Keep menu model/provider/enhancement/power-mode menu items as remaining parity gaps until those subsystems exist.

- [ ] **Step 4: Full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: all tests and Debug x64 build pass.

- [ ] **Step 5: Request review and fix findings**

Request subagent review against this plan and the macOS menu-bar behavior. Fix all Critical and Important findings before committing.

- [ ] **Step 6: Commit**

Run:

```powershell
git add .gitignore README.md `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Shell\TrayShellState.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Shell\TrayShellPresenter.cs `
  VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\Shell\TrayShellPresenterTests.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.Native\Tray\TrayIconService.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.App\MainWindow.xaml.cs `
  docs\superpowers\plans\2026-05-24-windows-tray-shell.md `
  docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "feat(windows): add tray shell"
```
