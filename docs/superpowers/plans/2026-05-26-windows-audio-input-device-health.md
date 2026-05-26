# Windows Audio Input Device Health Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add testable Audio Input device health rows and render them in WinUI.

**Architecture:** Add a Core presenter that derives display rows from existing audio choices, selected choice, prioritized devices, and mode. WinUI stores the presenter rows and binds them to a small `Device Health` list below the microphone selector.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Core Presenter

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioInputDeviceHealthPresenter.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Audio/AudioInputDeviceSelectionTests.cs`

- [x] **Step 1: Write failing tests**

Add tests for active custom devices and unavailable prioritized devices.

- [x] **Step 2: Run focused tests to verify RED**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter AudioInputDeviceSelectionTests
```

Expected: fail because `AudioInputDeviceHealthPresenter` does not exist.

- [x] **Step 3: Implement presenter**

Create `AudioInputDeviceHealthPresenter` and `AudioInputDeviceHealthRow`.

- [x] **Step 4: Run focused tests to verify GREEN**

Run the same focused command.

Expected: pass.

### Task 2: WinUI Binding

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add Device Health list**

Render presenter rows below the microphone picker.

- [x] **Step 2: Refresh rows with audio choices**

Update rows after refresh, apply, mode changes, and priority list edits.

- [x] **Step 3: Run focused tests and build**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter AudioInputDeviceSelectionTests
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: pass.

### Task 3: Docs, Review, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update docs**

Record Audio Input device health progress.

- [x] **Step 2: Run final verification**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: pass.

- [x] **Step 3: Review and commit**

Fix Critical/Important findings, then commit.
