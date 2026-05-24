# Global Shortcuts Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add configurable Windows global shortcuts for recording toggle, paste-last transcription, and paste-last enhanced transcription.

**Architecture:** Keep parsing, validation, action names, and duplicate detection in `VoiceInk.Windows.Core`. Keep Win32 `RegisterHotKey` integration in `VoiceInk.Windows.Native`. The WinUI shell reads/writes shortcut strings through existing JSON settings, registers valid shortcuts after settings load, and dispatches `WM_HOTKEY` actions to existing controller and paste-last flows.

**Tech Stack:** .NET 10, WinUI 3, Win32 `RegisterHotKey`/`WM_HOTKEY`, existing JSON settings store, existing paste-last Core service.

---

## Source Notes

- macOS source of truth: `VoiceInk/Shortcuts/ShortcutAction.swift`, `VoiceInk/Shortcuts/RecordingShortcutManager.swift`, and `VoiceInk/Views/Settings/SettingsView.swift`.
- Windows docs reference: Microsoft Learn `RegisterHotKey`, `WM_HOTKEY`, and virtual-key documentation.
- Windows constraint: this slice supports key+modifier global hotkeys. macOS modifier-only shortcuts and key-up push-to-talk modes remain later work because Win32 `RegisterHotKey` does not represent that interaction model directly.

## File Structure

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcut.cs`: shortcut parser/display model.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcutAction.cs`: action enum for `ToggleRecording`, `PasteLastTranscription`, and `PasteLastEnhancedTranscription`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcutRegistration.cs`: action+shortcut registration record.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcutRegistrationResult.cs`: registrations plus validation errors.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcutSettings.cs`: build validated registrations from `AppSettings`.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shortcuts/GlobalShortcutTests.cs`: parser and settings validation tests.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`: add paste-last shortcut settings while preserving `Hotkey`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Native/Hotkeys/GlobalHotkeyService.cs`: register multiple actions and raise action-specific events.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: add Shortcuts section with text boxes and Apply button.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: load/save shortcut fields, register after settings load, handle shortcut actions.
- Modify docs/spec/README with supported shortcuts and remaining gaps.

## Task 1: Core Shortcut Model

**Files:**

- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcut.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcutAction.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcutRegistration.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcutRegistrationResult.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcutSettings.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shortcuts/GlobalShortcutTests.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`

- [x] **Step 1: Write failing parser and validation tests**

Add tests for:

```csharp
Assert.True(GlobalShortcut.TryParse("Ctrl+Alt+Space", out var shortcut, out var error));
Assert.Null(error);
Assert.Equal("Ctrl+Alt+Space", shortcut!.DisplayText);
Assert.Equal(0x20, shortcut.VirtualKey);
Assert.True(shortcut.Control);
Assert.True(shortcut.Alt);

Assert.True(GlobalShortcut.TryParse("control + shift + o", out var normalized, out error));
Assert.Equal("Ctrl+Shift+O", normalized!.DisplayText);

Assert.False(GlobalShortcut.TryParse("Win+Space", out _, out error));
Assert.Equal("Windows-key shortcuts are reserved by Windows.", error);

var settings = new AppSettings
{
    Hotkey = "Ctrl+Alt+Space",
    PasteLastTranscriptionHotkey = "Ctrl+Alt+V",
    PasteLastEnhancementHotkey = "Ctrl+Alt+E"
};
var result = GlobalShortcutSettings.BuildRegistrations(settings);
Assert.Empty(result.Errors);
Assert.Equal(3, result.Registrations.Count);

var duplicates = GlobalShortcutSettings.BuildRegistrations(settings with
{
    PasteLastTranscriptionHotkey = "Ctrl+Alt+Space"
});
Assert.Contains(duplicates.Errors, item => item.Contains("already uses Ctrl+Alt+Space", StringComparison.Ordinal));
```

- [x] **Step 2: Verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter GlobalShortcutTests
```

Expected: build fails because shortcut model types do not exist.

- [x] **Step 3: Implement Core model**

Implement:

- Accepted modifiers: `Ctrl`/`Control`, `Alt`, `Shift`.
- Rejected modifiers: `Win`/`Windows`, with error `Windows-key shortcuts are reserved by Windows.`
- Accepted keys: `Space`, `Esc`/`Escape`, letters `A-Z`, digits `0-9`, and `F1` through `F24`.
- Require at least one modifier and exactly one non-modifier key.
- `GlobalShortcutSettings.BuildRegistrations(AppSettings)` requires `Hotkey`, treats paste-last shortcut fields as optional, and reports duplicate shortcuts by display text.

- [x] **Step 4: Verify GREEN**

Run the same filtered Core test command.

Expected: all `GlobalShortcutTests` pass.

- [x] **Step 5: Commit**

Commit message:

```text
feat(windows): add global shortcut settings model
```

## Task 2: Native Multi-Hotkey Registration

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Hotkeys/GlobalHotkeyService.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Update native registration**

Replace single `RegisterCtrlAltSpace()` with `RegisterHotkeys(IEnumerable<GlobalShortcutRegistration>)`.

Rules:

- Use deterministic ids starting at `0x5649`.
- Add `MOD_NOREPEAT`.
- Store id-to-action mapping.
- Raise `HotkeyPressed` with `GlobalShortcutAction`.
- Unregister every registered id on dispose.

- [x] **Step 2: Wire app dispatch**

After settings load, call `GlobalShortcutSettings.BuildRegistrations(settings)`, then register each valid shortcut. On hotkey:

- `ToggleRecording` calls `ToggleCurrentRecordingAsync()`.
- `PasteLastTranscription` calls `PasteLastAsync(LastTranscriptionTextKind.Final)`.
- `PasteLastEnhancedTranscription` calls `PasteLastAsync(LastTranscriptionTextKind.EnhancedPreferred)`.

- [x] **Step 3: Build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 errors.

- [x] **Step 4: Commit**

Commit message:

```text
feat(windows): register global shortcut actions
```

## Task 3: Shell Shortcut Settings

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`

- [x] **Step 1: Add settings persistence test**

Extend the existing `SaveAsync_PersistsSettings` test so `AppSettings` includes:

```csharp
PasteLastTranscriptionHotkey = "Ctrl+Alt+V",
PasteLastEnhancementHotkey = "Ctrl+Alt+E"
```

Expected: the round-trip equality assertion verifies JSON persistence.

- [x] **Step 2: Verify RED or targeted coverage**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter SaveAsync_PersistsSettings
```

Expected before `AppSettings` fields exist: build fails. If Task 1 already added fields, this verifies persistence coverage.

- [x] **Step 3: Add shell controls**

Add a `Shortcuts` section with:

- `RecordingHotkeyTextBox`, default text `Ctrl+Alt+Space`.
- `PasteLastHotkeyTextBox`, optional.
- `PasteLastEnhancedHotkeyTextBox`, optional.
- `ApplyShortcutsButton`.

`ApplyShortcutsButton_Click` validates registrations, saves settings, re-registers hotkeys, and reports `Shortcuts updated` or validation errors.

- [x] **Step 4: Verify**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter SaveAsync_PersistsSettings
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: targeted settings test passes and build succeeds.

- [x] **Step 5: Commit**

Commit message:

```text
feat(windows): expose configurable shortcut settings
```

## Task 4: Docs, Review, And Verification

**Files:**

- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/plans/2026-05-24-global-shortcuts.md`

- [x] **Step 1: Update docs**

Document supported configurable shortcuts: recording toggle, paste last transcription, and paste last enhanced transcription. Keep secondary shortcuts, push-to-talk/hybrid key-up behavior, retry-last, cancel-recording, quick-add, toggle enhancement, and Power Mode shortcuts as gaps.

- [x] **Step 2: Run full tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
```

Expected: all tests pass.

- [x] **Step 3: Run Debug x64 build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 errors.

- [x] **Step 4: Request review and fix Important findings**

Review the slice from this plan commit through HEAD. Fix Critical and Important findings before proceeding.

Review result: one Important transactional-registration finding was fixed by restoring
persisted live shortcuts on registration/save failure. One Minor parser validation gap was
fixed by rejecting empty shortcut parts.

- [x] **Step 5: Commit docs**

Commit message:

```text
docs(windows): note configurable shortcut support
```

## Plan Self-Review

Spec coverage:

- Covers configurable shortcut parsing, storage, registration, shell editing, paste-last utility shortcuts, and collision validation.
- Leaves macOS-only modifier-only/key-up modes and actions without Windows implementations for later slices.

Placeholder scan:

- No placeholder sections remain.

Type consistency:

- Uses `GlobalShortcut`, `GlobalShortcutAction`, `GlobalShortcutRegistration`, `GlobalShortcutSettings.BuildRegistrations`, and existing `AppSettings` names consistently.
