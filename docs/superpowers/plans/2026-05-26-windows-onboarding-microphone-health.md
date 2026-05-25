# Windows Onboarding Microphone Health Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add first-run microphone health status and in-dialog refresh to onboarding.

**Architecture:** Extend the UI-independent onboarding status record/service with microphone status strings, then bind those values into the existing WinUI onboarding dialog. Keep Settings launch user-initiated.

**Tech Stack:** .NET 10, xUnit, WinUI 3, Windows `ms-settings:privacy-microphone`.

---

### Task 1: Document onboarding microphone health

**Files:**
- Create: `docs/superpowers/specs/2026-05-26-windows-onboarding-microphone-health-design.md`
- Create: `docs/superpowers/plans/2026-05-26-windows-onboarding-microphone-health.md`

- [x] **Step 1: Save design and implementation plan**

Capture behavior, non-goals, and verification.

### Task 2: Add failing status tests

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Onboarding/OnboardingSetupStatusServiceTests.cs`

- [x] **Step 1: Add microphone health assertions**

Assert detected devices produce `Microphone detected` and no-device state produces `No microphone detected`, recovery text, and `Check Windows Microphone Settings`.

- [x] **Step 2: Run focused onboarding tests and confirm failure**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter OnboardingSetupStatusServiceTests
```

Expected: compile failure because the new status properties do not exist yet.

### Task 3: Implement status fields

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Onboarding/OnboardingSetupStatus.cs`

- [x] **Step 1: Add computed microphone status properties**

Add `MicrophoneStatusTitle`, `MicrophoneStatusMessage`, and `MicrophoneActionText`.

- [x] **Step 2: Run focused tests and confirm pass**

Run the focused onboarding test command.

### Task 4: Wire onboarding dialog

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add microphone status text**

Add a wrapping `TextBlock` that updates from `OnboardingSetupStatusService.Build`.

- [x] **Step 2: Add refresh action**

Add `Refresh Microphones` beside `Open Windows Microphone Settings`; refresh audio devices and update the dialog combo/status in-place.

- [x] **Step 3: Build app**

Run Debug x64 app build.

### Task 5: Verify, document, and commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update docs and completion tracker**

Mention onboarding microphone health and refresh.

- [x] **Step 2: Run verification**

Run focused onboarding tests, full solution tests, Debug x64 build, `git diff --check`, and review.

- [x] **Step 3: Commit**

Commit with:

```powershell
git commit -m "feat(windows): add onboarding microphone health"
```
