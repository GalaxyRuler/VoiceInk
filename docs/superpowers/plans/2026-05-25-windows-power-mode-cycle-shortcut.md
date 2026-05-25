# Windows Power Mode Cycle Shortcut Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a configurable global shortcut that cycles Automatic and enabled Power Mode rules.

**Architecture:** Extend the existing settings-backed global shortcut model and add a small Core cycler helper. WinUI dispatch updates `selectedPowerModeRuleId`, saves settings, and refreshes the shell/floating recorder.

**Tech Stack:** .NET 10, WinUI 3, existing RegisterHotKey-based native hotkey service, xUnit.

---

### Task 1: Shortcut Model and Cycler Tests

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcutAction.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcutSettings.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/PowerMode/PowerModeShortcutCycler.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shortcuts/GlobalShortcutTests.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/PowerMode/PowerModeShortcutCyclerTests.cs`

- [ ] **Step 1: Write failing tests**

Add shortcut registration/duplicate tests and cycler behavior tests.

- [ ] **Step 2: Run focused tests and verify red**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~GlobalShortcutTests|FullyQualifiedName~PowerModeShortcutCyclerTests"
```

Expected: compile failure because the new shortcut field/action/cycler do not exist.

- [ ] **Step 3: Implement Core support**

Add `CyclePowerModeHotkey`, `GlobalShortcutAction.CyclePowerMode`, optional registration display name `Cycle Power Mode`, and `PowerModeShortcutCycler.NextRuleId`.

- [ ] **Step 4: Run focused tests and verify green**

Run the same focused command. Expected: all focused tests pass.

### Task 2: WinUI Dispatch

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`

- [ ] **Step 1: Add Settings field**

Add `CyclePowerModeHotkeyTextBox` to Settings > Shortcuts.

- [ ] **Step 2: Load and save the field**

Apply settings to the textbox and include it in `CurrentSettingsAsync` when shortcut fields are included.

- [ ] **Step 3: Dispatch the hotkey**

Add a `CyclePowerMode` case that computes the next rule id, saves settings, and reports the selected Power Mode.

### Task 3: Full Verification and Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [ ] **Step 1: Run full tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
```

- [ ] **Step 2: Run Debug x64 build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

- [ ] **Step 3: Run whitespace check**

Run:

```powershell
git diff --check
```

- [ ] **Step 4: Update completion tracker and commit**

Commit only intentional source/docs changes.
