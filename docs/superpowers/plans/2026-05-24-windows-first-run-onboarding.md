# Windows First-Run Onboarding Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a source-runnable Windows first-run setup flow that mirrors the macOS onboarding intent: model setup, microphone guidance, shortcut setup, and a short try-it path.

**Architecture:** Core stores onboarding completion in `AppSettings` and exposes a small setup-status helper that can be tested without WinUI. Infrastructure persists the new setting through the existing JSON settings store. WinUI shows a first-run `ContentDialog` after settings/audio devices load, updates model path, audio input, and primary shortcut fields, opens Windows microphone privacy settings when requested, and marks onboarding complete on save or skip.

**Tech Stack:** .NET 10, WinUI 3 `ContentDialog`, Windows settings URI launch through `Process.Start`, JSON settings persistence, xUnit.

---

## Files

- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs` to add `HasCompletedOnboarding`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Onboarding/OnboardingSetupStatus.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Onboarding/OnboardingSetupStatusService.cs`.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Onboarding/OnboardingSetupStatusServiceTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`.
- Modify `README.md` and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [ ] **Step 1: Mark onboarding active in the parity spec**

Record the macOS source flow: welcome, microphone/device/accessibility/screen/shortcut permissions, model download, and try-it tutorial. Record the Windows adaptation: first-run setup dialog for local model path, microphone privacy/settings, audio input selection, primary shortcut, and try-it instructions. Note that direct model download/import cards remain in the model-management subsystem.

- [ ] **Step 2: Commit the plan**

Run:

```powershell
git add docs\superpowers\plans\2026-05-24-windows-first-run-onboarding.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "docs(windows): plan first run onboarding"
```

Expected: docs-only commit.

## Task 2: Red Tests

- [ ] **Step 1: Add Core setup-status tests**

Create tests proving:

- Blank model path with default shortcut is incomplete and reports model setup missing.
- Nonblank model path and primary shortcut are enough to complete setup even when no audio devices are detected.
- Blank primary shortcut is incomplete and reports shortcut setup missing.

- [ ] **Step 2: Add settings persistence test**

Update `JsonSettingsStoreTests.SaveAsync_PersistsSettings` to set `HasCompletedOnboarding = true` and assert the saved/reloaded record preserves it.

- [ ] **Step 3: Run red tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~OnboardingSetupStatusServiceTests
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter FullyQualifiedName~JsonSettingsStoreTests
```

Expected: Core compile failure because the onboarding status service does not exist, and Infrastructure failure/compile error until `HasCompletedOnboarding` exists.

## Task 3: Core And Persistence

- [ ] **Step 1: Add settings property**

Add `public bool HasCompletedOnboarding { get; init; }` to `AppSettings`. Default is `false`.

- [ ] **Step 2: Add onboarding status helper**

`OnboardingSetupStatus` contains `HasModelPath`, `HasPrimaryShortcut`, `HasAudioInputChoices`, and `CanCompleteSetup`.

`OnboardingSetupStatusService.Build(AppSettings settings, bool hasAudioInputChoices)` returns:

- `HasModelPath = !string.IsNullOrWhiteSpace(settings.ModelPath)`
- `HasPrimaryShortcut = !string.IsNullOrWhiteSpace(settings.Hotkey)`
- `HasAudioInputChoices = hasAudioInputChoices`
- `CanCompleteSetup = HasModelPath && HasPrimaryShortcut`

- [ ] **Step 3: Verify focused tests**

Run the same Core and Infrastructure filtered test commands from Task 2. Expected: selected tests pass.

## Task 4: WinUI Dialog

- [ ] **Step 1: Show only on first run**

After `InitializeAsync` loads settings, audio devices, dictionary, history, and hotkeys, call `ShowOnboardingIfNeededAsync(settings)` if `settings.HasCompletedOnboarding` is false. Guard with `isOnboardingOpen` so only one dialog can appear.

- [ ] **Step 2: Build dialog content**

The dialog content includes:

- Local whisper model path `TextBox`, prefilled from `ModelPathTextBox`.
- `Browse .bin` button that opens a `FileOpenPicker` filtered to `.bin`, then fills both dialog and main model path fields.
- Microphone row showing current audio input, an onboarding audio input `ComboBox` bound to `audioInputChoices`, and an `Open Windows Microphone Settings` button launching `ms-settings:privacy-microphone`.
- Primary shortcut `TextBox`, prefilled from `RecordingHotkeyTextBox`.
- Try-it instructions matching the macOS tutorial intent: click a text field, press shortcut, speak, press shortcut again.
- Status text for validation/errors.

- [ ] **Step 3: Save and skip behavior**

Primary button `Save Setup` validates the model path and primary shortcut using `OnboardingSetupStatusService` plus existing hotkey registration validation. On success it persists model path, selected audio input, primary shortcut, and `HasCompletedOnboarding = true`, refreshes hotkeys/controller state, and closes. Secondary button `Skip For Now` marks `HasCompletedOnboarding = true` and closes without requiring a model path.

- [ ] **Step 4: Verify app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build passes with 0 warnings/errors.

## Task 5: Docs, Review, Commit

- [ ] **Step 1: Update README/spec**

Document first-run onboarding as implemented. Leave model catalog/download/import cards, permission health checks, and reset-onboarding settings as remaining gaps.

- [ ] **Step 2: Full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: all tests and Debug x64 build pass.

- [ ] **Step 3: Request review and fix findings**

Request subagent review against this plan and the macOS onboarding source. Fix all Critical and Important findings before committing.

- [ ] **Step 4: Commit**

Run:

```powershell
git add README.md `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Settings\AppSettings.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Onboarding\OnboardingSetupStatus.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Onboarding\OnboardingSetupStatusService.cs `
  VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\Onboarding\OnboardingSetupStatusServiceTests.cs `
  VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\Settings\JsonSettingsStoreTests.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.App\MainWindow.xaml.cs `
  docs\superpowers\plans\2026-05-24-windows-first-run-onboarding.md `
  docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "feat(windows): add first run onboarding"
```
