# Windows Audio Input Status UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a persistent Audio Input page status notice that explains the active microphone selection and availability fallback.

**Architecture:** Keep availability reasoning in `VoiceInk.Windows.Core.Audio` and have WinUI render the resulting notice through an `InfoBar`. Preserve the existing combo-box selection and shell status warning behavior.

**Tech Stack:** .NET 10, WinUI 3 `InfoBar`, xUnit core tests.

---

### Task 1: Core Notice Model

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioInputDeviceSelectionNoticeKind.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioInputDeviceSelectionNotice.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioInputDeviceSelectionResult.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioInputDeviceSelection.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Audio/AudioInputDeviceSelectionTests.cs`

- [x] **Step 1: Write failing notice tests**

Add assertions for success, info, warning, and error notices in the existing audio selection tests.

- [x] **Step 2: Verify red**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter AudioInputDeviceSelectionTests
```

Focused tests failed at compile time because the notice model did not exist.

- [x] **Step 3: Implement notice model**

Add notice kind/title/message/action text data to `AudioInputDeviceSelectionResult` and populate it from `BuildChoices`.

- [x] **Step 4: Verify focused tests**

Focused audio selection tests passed.

### Task 2: WinUI InfoBar Rendering

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add Audio Input InfoBar**

Add `AudioInputStatusInfoBar` under the Audio Input header and before the selector.

- [x] **Step 2: Bind notice updates**

Store the latest selection notice from `RefreshAudioInputDevicesAsync`, map Core notice kinds to `InfoBarSeverity`, and update title/message/action text after refreshes and applies. Fixed review feedback so Apply recomputes the notice from the current combo selection immediately.

- [x] **Step 3: Verify app build**

App project x64 build passed.

### Task 3: Docs, Review, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update completion tracker**

Raised the Audio Input bar and marked the current slice as Audio Input status UI.

- [x] **Step 2: Full verification and review**

Ran focused tests, full solution tests, Debug x64 build, and code review. Fixed Important review feedback for stale status notices after Apply.

- [x] **Step 3: Commit**

Commit only intentional source/docs changes.
