# Windows Audio Endpoint Identity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Persist and prefer Windows Core Audio endpoint IDs for custom and prioritized microphone identity.

**Architecture:** Add endpoint ID fields to Core audio/settings records with backwards-compatible empty defaults. Keep `WaveInEvent` capture by device number, but enrich the native device provider with Core Audio endpoint IDs and make the selection algorithm prefer endpoint identity.

**Tech Stack:** .NET 10, NAudio `WaveIn` plus `MMDeviceEnumerator`, WinUI 3, xUnit.

---

### Task 1: Core Endpoint Identity

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioInputDevice.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioInputDeviceChoice.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/PrioritizedAudioInputDevice.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioInputDeviceSelection.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Audio/AudioInputDeviceSelectionTests.cs`

- [x] Add failing tests for endpoint rebinding and prioritized endpoint matching.
- [x] Add endpoint ID fields with empty defaults.
- [x] Prefer endpoint ID in custom/prioritized selection.
- [x] Run focused audio selection tests.

### Task 2: Native Provider And App Persistence

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Audio/NAudioInputDeviceProvider.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Backup/VoiceInkSettingsBackupTests.cs`

- [x] Enrich WaveIn devices with Core Audio endpoint IDs when names can be matched.
- [x] Persist selected custom endpoint ID and prioritized endpoint IDs.
- [x] Preserve existing behavior when endpoint IDs are unavailable.
- [x] Run full solution tests.

### Task 3: Docs And Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] Update docs and completion tracker.
- [x] Run Debug x64 build, `git diff --check`, review, and commit.
