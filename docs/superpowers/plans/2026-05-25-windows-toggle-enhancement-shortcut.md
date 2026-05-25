# Windows Toggle Enhancement Shortcut Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a configurable global shortcut that toggles AI enhancement on or off.

**Architecture:** Extend the existing settings-backed global shortcut registration model with one optional hotkey and one new action. Dispatch the action in `MainWindow` by flipping `IsEnhancementEnabled`, saving settings, and refreshing UI state.

**Tech Stack:** .NET 10, WinUI 3, existing Windows `RegisterHotKey` service, xUnit tests.

---

### Task 1: Shortcut Model And Persistence

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcutAction.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcutSettings.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shortcuts/GlobalShortcutTests.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Backup/VoiceInkSettingsBackupTests.cs`

- [ ] **Step 1: Write failing shortcut tests**

Add `ToggleEnhancementHotkey = "Ctrl+Alt+X"` to `BuildRegistrations_UsesPrimaryAndOptionalHotkeys` and assert a `GlobalShortcutAction.ToggleEnhancement` registration. Add a duplicate test where `ToggleEnhancementHotkey = "Ctrl+Alt+Space"` expects `Toggle Enhancement already uses Ctrl+Alt+Space.`

- [ ] **Step 2: Run red shortcut tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~GlobalShortcutTests"
```

Expected: compile/test failure because the setting/action is missing.

- [ ] **Step 3: Implement shortcut model**

Add `ToggleEnhancementHotkey` to `AppSettings`, equality, and hash code. Add `ToggleEnhancement` to `GlobalShortcutAction`. Add an optional registration with display name `Toggle Enhancement` in `GlobalShortcutSettings.BuildRegistrations`.

- [ ] **Step 4: Add persistence tests**

Update JSON settings and backup tests to set and assert `ToggleEnhancementHotkey = "Ctrl+Alt+X"`.

- [ ] **Step 5: Run focused tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~GlobalShortcutTests|FullyQualifiedName~VoiceInkSettingsBackupTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~JsonSettingsStoreTests"
```

Expected: all focused tests pass.

### Task 2: WinUI Settings And Hotkey Dispatch

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Diagnostics/DiagnosticEventSanitizer.cs`

- [ ] **Step 1: Add Shortcut textbox**

Add `ToggleEnhancementHotkeyTextBox` in the Settings > Shortcuts stack after `Quick Add to Dictionary`, with header `Toggle Enhancement` and placeholder `Ctrl+Alt+X`.

- [ ] **Step 2: Bind settings load/save**

Populate the textbox in `LoadSettingsIntoUiAsync`. Include it in `CurrentSettingsAsync` when `includeShortcutFields` is true.

- [ ] **Step 3: Dispatch hotkey action**

Add a `GlobalShortcutAction.ToggleEnhancement` case in `HotkeyService_HotkeyPressed`. Implement `ToggleEnhancementAsync` that returns early during active operations, flips the checkbox, saves settings, and reports `Enhancement enabled` or `Enhancement disabled`.

- [ ] **Step 4: Update diagnostics allow-list**

Add `Enhancement enabled`, `Enhancement disabled`, and `Enhancement toggle failed` to `DiagnosticEventSanitizer`.

- [ ] **Step 5: Run app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 warnings/errors.

### Task 3: Docs, Verification, And Commit

**Files:**

- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Add: `docs/superpowers/specs/2026-05-25-windows-toggle-enhancement-shortcut-design.md`
- Add: `docs/superpowers/plans/2026-05-25-windows-toggle-enhancement-shortcut.md`

- [ ] **Step 1: Update docs**

Mark toggle-enhancement shortcut implemented. Update the current slice and completion bar.

- [ ] **Step 2: Run full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: tests pass, build succeeds, and diff check reports no whitespace errors.

- [ ] **Step 3: Commit**

Run:

```powershell
git add -- VoiceInk.Windows docs/superpowers
git commit -m "feat(windows): add toggle enhancement shortcut"
```
