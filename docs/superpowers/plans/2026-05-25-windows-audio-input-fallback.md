# Windows Audio Input Fallback Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ensure the active Windows capture service uses the resolved saved microphone selection at startup and after device refresh.

**Architecture:** Keep device resolution in `VoiceInk.Windows.Core.Audio` so it remains UI-independent and testable. Let the WinUI shell consume the resolved choice and recreate the capture controller when the active capture service does not match it.

**Tech Stack:** .NET 10, WinUI 3, NAudio, xUnit.

---

### Task 1: Expose Resolved Audio Input Choice

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioInputDeviceSelectionResult.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Audio/AudioInputDeviceSelectionTests.cs`

- [ ] **Step 1: Write failing tests**

Add assertions showing `AudioInputDeviceSelectionResult.SelectedChoice` returns the resolved custom device, rebound device, or System Default fallback.

- [ ] **Step 2: Run focused tests and verify red**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~AudioInputDeviceSelectionTests"
```

Expected: compile failure or test failure because `SelectedChoice` does not exist.

- [ ] **Step 3: Implement minimal selected-choice API**

Add a computed `SelectedChoice` property to `AudioInputDeviceSelectionResult` that returns the selected choice when the index is valid.

- [ ] **Step 4: Run focused tests and verify green**

Run the same focused test command. Expected: all audio input selection tests pass.

### Task 2: Apply Resolved Selection to Startup Capture

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [ ] **Step 1: Use the resolved UI selection after settings load**

After `ApplySettingsToUiAsync(...)` completes during startup, compare `activeAudioInputDeviceChoice` against `SelectedAudioInputDeviceChoice()` and call `RecreateController()` when they differ.

- [ ] **Step 2: Preserve manual refresh behavior**

After `RefreshAudioInputDevicesWithStatusAsync()` refreshes the combo box, recreate the controller if the resolved selected choice differs from the active capture choice and the app is not recording.

- [ ] **Step 3: Verify focused tests remain green**

Run the focused audio input selection tests.

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

Update the project completion bar for the completed audio input fallback slice, then commit only the intentional source/docs changes.
