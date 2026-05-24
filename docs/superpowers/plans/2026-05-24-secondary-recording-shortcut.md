# Secondary Recording Shortcut Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the macOS-style Secondary Shortcut field to the Windows MVP as a second configurable toggle-recording hotkey.

**Architecture:** Core settings and shortcut registration own the secondary shortcut. The existing native `RegisterHotKey` service already supports multiple registrations, so the secondary shortcut can map to the same `ToggleRecording` action as the primary shortcut. WinUI exposes a second shortcut text field and persists it with the existing Apply Shortcuts flow.

**Tech Stack:** .NET 10, WinUI 3, Win32 `RegisterHotKey`/`WM_HOTKEY`, existing global hotkey service, xUnit.

---

## Files

- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs` for `SecondaryRecordingHotkey`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcutSettings.cs` for optional secondary registration.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shortcuts/GlobalShortcutTests.cs` for secondary registration and duplicate validation.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs` for settings roundtrip.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml` and `MainWindow.xaml.cs` for the secondary shortcut field.
- Modify `README.md` and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [ ] **Step 1: Mark secondary shortcut active**

Update the parity spec with an active slice note: Windows is adding a second toggle-recording shortcut now; push-to-talk/hybrid and key-up handling remain later native-hook work because `RegisterHotKey` emits `WM_HOTKEY` on activation, not key-up.

- [ ] **Step 2: Commit the plan**

Run:

```powershell
git add docs\superpowers\plans\2026-05-24-secondary-recording-shortcut.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "docs(windows): plan secondary recording shortcut"
```

Expected: docs-only commit.

## Task 2: Red Tests

- [ ] **Step 1: Add shortcut registration coverage**

Update `BuildRegistrations_UsesPrimaryAndOptionalHotkeys` with `SecondaryRecordingHotkey = "Ctrl+Alt+S"` and assert a second `GlobalShortcutAction.ToggleRecording` registration with display text `Ctrl+Alt+S` immediately after the primary registration.

- [ ] **Step 2: Add duplicate coverage**

Add `BuildRegistrations_ReportsDuplicateSecondaryRecordingAssignment`, using `Hotkey = "Ctrl+Alt+Space"` and `SecondaryRecordingHotkey = "Ctrl+Alt+Space"`, expecting `Secondary Shortcut already uses Ctrl+Alt+Space.`.

- [ ] **Step 3: Add settings test expectation**

In `JsonSettingsStoreTests.SaveAsync_PersistsSettings`, add `SecondaryRecordingHotkey = "Ctrl+Alt+S"`.

- [ ] **Step 4: Run red tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~GlobalShortcutTests
```

Expected: compile failure because `SecondaryRecordingHotkey` does not exist.

## Task 3: Core Implementation

- [ ] **Step 1: Add setting**

Add `SecondaryRecordingHotkey` to `AppSettings` with default `string.Empty`.

- [ ] **Step 2: Register optional secondary shortcut**

In `GlobalShortcutSettings.BuildRegistrations`, call `AddRegistration(settings.SecondaryRecordingHotkey, GlobalShortcutAction.ToggleRecording, "Secondary Shortcut", required: false, ...)` immediately after the primary shortcut registration.

- [ ] **Step 3: Verify focused tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~GlobalShortcutTests
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter FullyQualifiedName~JsonSettingsStoreTests
```

Expected: selected tests pass.

## Task 4: WinUI Wiring

- [ ] **Step 1: Add secondary shortcut field**

Add `SecondaryRecordingHotkeyTextBox` under the Primary Shortcut field with header `Secondary Shortcut` and placeholder `Ctrl+Alt+S`.

- [ ] **Step 2: Load and save the field**

Load `settings.SecondaryRecordingHotkey` during initialization. Include it in `CurrentSettingsAsync` only when `includeShortcutFields` is true, matching the other shortcut fields.

- [ ] **Step 3: Verify app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build passes with 0 warnings/errors.

## Task 5: Review, Docs, Commit

- [ ] **Step 1: Update docs**

Update README/spec implemented shortcut lists. Keep push-to-talk/hybrid/key-up modes as remaining shortcut gaps.

- [ ] **Step 2: Full verification**

Run full solution tests and Debug x64 build.

- [ ] **Step 3: Request review**

Request review, fix Critical and Important issues.

- [ ] **Step 4: Commit**

Run:

```powershell
git add README.md VoiceInk.Windows docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "feat(windows): add secondary recording shortcut"
```
