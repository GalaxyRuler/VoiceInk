# Windows Clipboard Paste Settings Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add macOS-aligned clipboard restore delay and paste-method controls to Windows Settings.

**Architecture:** Keep setting defaults and paste-method normalization in Core, load insertion settings at paste time in the native Windows text injection service, and keep WinUI as a settings editor. Clipboard restore uses a transient session marker so VoiceInk restores only its own paste payload.

**Tech Stack:** .NET 10, WinUI 3, System.Text.Json settings, WinForms clipboard STA bridge, Win32 `SendInput`, xUnit.

---

## File Structure

- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`: add `ClipboardRestoreDelaySeconds` and `PasteMethod`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/PasteMethodSettings.cs`: constants and policy helpers for supported paste methods and restore-delay clamping.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/ClipboardTextInjectionService.cs`: read settings at insertion time, use session-marked clipboard restore, and add direct Unicode text input.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: add Clipboard controls under Settings.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: load/save clipboard settings and construct the injection service from the settings store.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Text/...`: add paste policy tests.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`: add settings round-trip coverage.
- Modify `README.md`, `docs/superpowers/project-completion.md`, and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`: document the completed behavior.

## Task 1: Core Paste Settings

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/PasteMethodSettings.cs`
- Create or modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Text/PasteMethodSettingsTests.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`

- [x] **Step 1: Write failing policy tests**

Test that unknown paste methods normalize to `default`, `directText` is preserved, and restore delays below `0.25` seconds clamp to `0.25`.

- [x] **Step 2: Write failing settings persistence test**

Add to `JsonSettingsStoreTests.SaveAsync_PersistsSettings`:

```csharp
ClipboardRestoreDelaySeconds = 3.0,
PasteMethod = "directText",
```

- [x] **Step 3: Verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "PasteMethodSettingsTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "JsonSettingsStoreTests.SaveAsync_PersistsSettings"
```

Expected: compile failures because the paste policy/settings do not exist yet.

- [x] **Step 4: Implement minimal Core support**

Add settings defaults:

```csharp
public double ClipboardRestoreDelaySeconds { get; init; } = 2.0;
public string PasteMethod { get; init; } = PasteMethodSettings.Default;
```

Add paste policy helpers with `Default = "default"` and `DirectText = "directText"`.

- [x] **Step 5: Verify GREEN**

Run the same focused tests. Expected: all selected tests pass.

## Task 2: Native Injection And WinUI Controls

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/ClipboardTextInjectionService.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Update native injection service**

Change the constructor to accept `ISettingsStore`. On each `InsertAsync`, load settings, normalize paste method, and choose:

- `default`: set clipboard with text plus a VoiceInk paste session marker, send Ctrl+V, restore after effective delay only if the marker and text still match.
- `directText`: send Unicode keyboard input with `KEYEVENTF_UNICODE` and do not touch the clipboard.

- [x] **Step 2: Add WinUI controls**

Add a Settings `Clipboard` group with `Keep Clipboard Content`, `Restore Delay`, and `Paste Method`. Use delay choices `250ms`, `500ms`, `1s`, `2s`, `3s`, `4s`, and `5s`. Use paste choices `Default` and `Direct Text`.

- [x] **Step 3: Wire load/save**

Set the controls in `ApplySettingsToUiAsync` and read them in `CurrentSettingsAsync`.

- [x] **Step 4: Build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds.

## Task 3: Docs, Review, Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Document behavior**

Update docs to describe clipboard restore delay, session-marker safety, and the Windows `Direct Text` paste method.

- [x] **Step 2: Full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

- [x] **Step 3: Request review**

Ask for review focused on clipboard data safety, settings persistence, and SendInput behavior. Fix all Critical/Important findings.

- [x] **Step 4: Commit**

Commit with:

```powershell
git commit -m "feat(windows): add clipboard paste settings"
```
