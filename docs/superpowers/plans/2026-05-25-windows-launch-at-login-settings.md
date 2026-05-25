# Windows Launch at Login Settings Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a macOS-aligned `Launch at Login` Settings toggle for the unpackaged Windows app.

**Architecture:** Persist the user intent in Core settings and keep the OS integration in a small Native service. Use the current user's Windows `Run` registry key for the source-runnable unpackaged app, and pass a startup argument so the app can launch hidden to tray at sign-in.

**Tech Stack:** .NET 10, WinUI 3, System.Text.Json settings, Microsoft.Win32 registry access, xUnit.

---

## File Structure

- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`: add `LaunchAtLogin`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Startup/StartupLaunchCommand.cs`: quote executable paths and append startup arguments.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Startup/StartupLaunchMode.cs`: detect normal vs login startup launch from command-line args.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Startup/StartupRegistrationState.cs`: represent enabled/disabled/warning state for UI.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IStartupRegistrationService.cs`: platform-neutral startup registration contract.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/Startup/RegistryStartupRegistrationService.cs`: HKCU Run implementation.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/App.xaml.cs`: pass startup-hidden intent into `MainWindow`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: add `Launch at Login` checkbox in Settings > General.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: load startup state, save/apply registration, hide at startup when requested, and roll back import failures.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Startup/...`: add command and launch-mode tests.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`: add settings persistence coverage.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Backup/VoiceInkSettingsBackupTests.cs`: add backup/merge coverage.
- Modify `README.md`, `docs/superpowers/project-completion.md`, and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`: document behavior and update the completion bar.

## Task 1: Core Startup Policy

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Startup/StartupLaunchCommand.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Startup/StartupLaunchMode.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Startup/StartupRegistrationState.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IStartupRegistrationService.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Startup/StartupLaunchCommandTests.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Startup/StartupLaunchModeTests.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Backup/VoiceInkSettingsBackupTests.cs`

- [x] **Step 1: Write failing command tests**

Add tests that `StartupLaunchCommand.Build` wraps executable paths in quotes, escapes embedded quotes, appends `--voiceink-startup`, and returns an empty command for a missing executable path.

- [x] **Step 2: Write failing launch mode tests**

Add tests that `StartupLaunchMode.IsLoginStartup` returns true when args contain `--voiceink-startup`, case-insensitively, and false for normal launches.

- [x] **Step 3: Write failing persistence tests**

Add `LaunchAtLogin = true` to JSON settings round-trip coverage and backup `RichSettings`, then assert General Settings export/import preserves it.

- [x] **Step 4: Verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~StartupLaunchCommandTests|FullyQualifiedName~StartupLaunchModeTests|FullyQualifiedName~VoiceInkSettingsBackupTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "JsonSettingsStoreTests.SaveAsync_PersistsSettings"
```

Expected: compile failures because startup policy/settings do not exist yet.

- [x] **Step 5: Implement Core support**

Add `LaunchAtLogin` to `AppSettings` equality/hash behavior, implement startup command/mode helpers, add `StartupRegistrationState`, and add `IStartupRegistrationService`.

- [x] **Step 6: Verify GREEN**

Run the same focused tests. Expected: all selected tests pass.

## Task 2: Native Registration And WinUI

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Startup/RegistryStartupRegistrationService.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/App.xaml.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Implement registry startup service**

Use `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` with value name `VoiceInk.Windows`. `SetEnabledAsync(true)` writes the exact command from `StartupLaunchCommand.Build(Environment.ProcessPath)`. `SetEnabledAsync(false)` deletes the value if present. `GetStateAsync` returns enabled only when the current Run value equals the expected command and warning when another value exists.

- [x] **Step 2: Add launch-hidden startup path**

In `App.xaml.cs`, use `Environment.GetCommandLineArgs()` and `StartupLaunchMode.IsLoginStartup(args.Skip(1))` to construct `MainWindow(startHiddenToTray: true)` for login startup launches.

- [x] **Step 3: Add WinUI Settings controls**

Add a `LaunchAtLoginCheckBox` with content `Launch at Login` in Settings > General, near `Reset Onboarding`.

- [x] **Step 4: Wire save/load/import**

Construct `RegistryStartupRegistrationService`, sync the checkbox from settings and current OS state, apply registry changes before saving settings, roll back registration on save/import failure, and hide the window to tray after initialization when `startHiddenToTray` is true.

- [x] **Step 5: Build**

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
- Modify: `docs/superpowers/plans/2026-05-25-windows-launch-at-login-settings.md`

- [x] **Step 1: Document behavior**

Update docs to describe `Launch at Login`, HKCU Run usage for the unpackaged source build, startup-hidden tray behavior, and the Settings completion bar.

- [x] **Step 2: Full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

- [x] **Step 3: Request review**

Ask for review focused on registry startup safety, settings/OS consistency, backup import rollback, launch-hidden behavior, and open-source/privacy constraints. Fix all Critical/Important findings.

- [x] **Step 4: Commit**

Commit with:

```powershell
git commit -m "feat(windows): add launch at login setting"
```
