# Open History Shortcut Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the macOS-style Open History Window shortcut action to the Windows MVP, adapted to the current inline-history WinUI shell.

**Architecture:** Core remains limited to settings and shortcut registration. The WinUI shell handles the platform-specific behavior: restore the main window, request foreground activation, refresh the history list, and focus the history search box/list area. This mirrors the intent of the macOS separate history window while avoiding premature multi-window architecture.

**Tech Stack:** .NET 10, WinUI 3, Win32 `ShowWindow`/`SetForegroundWindow`, existing global hotkey service, xUnit.

---

## Files

- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs` for `OpenHistoryHotkey`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcutAction.cs` and `GlobalShortcutSettings.cs` for `OpenHistoryWindow`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shortcuts/GlobalShortcutTests.cs` and `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml` and `MainWindow.xaml.cs` to expose the shortcut field and route the action.
- Modify `README.md` and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [ ] **Step 1: Mark Open History active**

Update the parity spec status with the active slice note: Open History restores/focuses the Windows shell and inline History area because a dedicated history window is not yet present.

- [ ] **Step 2: Commit the plan**

Run:

```powershell
git add docs\superpowers\plans\2026-05-24-open-history-shortcut.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "docs(windows): plan open history shortcut"
```

Expected: docs-only commit.

## Task 2: Red Tests

- [ ] **Step 1: Add shortcut tests**

Update `BuildRegistrations_UsesPrimaryAndOptionalHotkeys` with `OpenHistoryHotkey = "Ctrl+Alt+H"` and assert `GlobalShortcutAction.OpenHistoryWindow`. Add `BuildRegistrations_ReportsDuplicateOpenHistoryAssignment` expecting `Open History Window already uses Ctrl+Alt+Space.`.

- [ ] **Step 2: Add settings test expectation**

In `JsonSettingsStoreTests.SaveAsync_PersistsSettings`, add `OpenHistoryHotkey = "Ctrl+Alt+H"`.

- [ ] **Step 3: Run red tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~GlobalShortcutTests
```

Expected: compile failure because the setting/action do not exist.

## Task 3: Core Implementation

- [ ] **Step 1: Add setting and action**

Add `OpenHistoryHotkey` to `AppSettings` and `OpenHistoryWindow` to `GlobalShortcutAction`.

- [ ] **Step 2: Add shortcut registration**

Register optional `settings.OpenHistoryHotkey` in `GlobalShortcutSettings.BuildRegistrations` with display name `Open History Window`.

- [ ] **Step 3: Verify focused tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~GlobalShortcutTests
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter FullyQualifiedName~JsonSettingsStoreTests
```

Expected: selected tests pass.

## Task 4: WinUI Wiring

- [ ] **Step 1: Add shortcut field**

Add `OpenHistoryHotkeyTextBox` under Shortcuts with header `Open History Window` and placeholder `Ctrl+Alt+H`.

- [ ] **Step 2: Load/save and route**

Load/save `OpenHistoryHotkey` and route `GlobalShortcutAction.OpenHistoryWindow` to `OpenHistoryWindowAsync`.

- [ ] **Step 3: Implement shell behavior**

`OpenHistoryWindowAsync` should restore the window with `ShowWindow(SW_RESTORE)`, call `Activate()`, request foreground with `SetForegroundWindow`, refresh history, focus `HistorySearchTextBox`, and set status `History opened`. It should not mutate history filters.

- [ ] **Step 4: Verify app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build passes with 0 warnings/errors.

## Task 5: Review, Docs, Commit

- [ ] **Step 1: Update docs**

Update README/spec implemented shortcut list. Keep dedicated multi-window history as a later shell/navigation gap.

- [ ] **Step 2: Full verification**

Run full solution tests and Debug x64 build.

- [ ] **Step 3: Request review**

Request review, fix Critical and Important issues.

- [ ] **Step 4: Commit**

Run:

```powershell
git add README.md VoiceInk.Windows docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "feat(windows): add open history shortcut"
```
