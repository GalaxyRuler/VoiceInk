# Windows Audio Input Device Change Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add live audio input device-change refresh behavior.

**Architecture:** Add a native watcher around NAudio Core Audio endpoint notifications, debounce callback bursts, and wire the main window to refresh the already-existing audio input chooser.

**Tech Stack:** .NET 10, C#, xUnit, NAudio CoreAudio API, WinUI 3 DispatcherQueue.

---

### Task 1: Native Watcher

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Audio/IAudioEndpointNotificationRegistrar.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Audio/NAudioEndpointNotificationRegistrar.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Audio/NAudioInputDeviceChangeWatcher.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Audio/NAudioInputDeviceChangeWatcherTests.cs`

- [x] **Step 1: Write failing watcher tests**

Test registration/disposal, capture default-device notification, render default-device ignore, and debouncing.

- [x] **Step 2: Verify red**

Run focused watcher tests. Expected: fail because watcher types do not exist.

- [x] **Step 3: Implement watcher**

Add registrar wrapper and watcher implementation.

- [x] **Step 4: Verify green**

Run focused watcher tests. Expected: pass.

### Task 2: App Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Subscribe watcher**

Create the watcher in `MainWindow`, subscribe to `DevicesChanged`, and dispose/unsubscribe on close.

- [x] **Step 2: Refresh chooser on UI thread**

Use the dispatcher to call existing `RefreshAudioInputDevicesAsync`, recreate the controller only when safe, and show a short status.

- [x] **Step 3: Verify build**

Run x64 Debug build.

### Task 3: Verification, Review, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Run full tests and build**

Run full solution tests and x64 Debug build.

- [x] **Step 2: Review**

Request review if an agent slot is available; otherwise perform local diff review and document that.

Review found a recorder rebuild risk during active recording and a cross-thread coalescing risk. Both were fixed and re-reviewed with no remaining Critical or Important findings.

- [ ] **Step 3: Commit**

Commit the watcher slice.
