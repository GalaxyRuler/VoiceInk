# Windows Onboarding Checklist Guidance Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a testable onboarding presenter that gives the Windows first-run setup dialog macOS-aligned progress and checklist guidance.

**Architecture:** Add a small Core presenter over the existing `OnboardingSetupStatus` record. The WinUI dialog will consume presenter strings and keep platform actions in `MainWindow.xaml.cs`.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Onboarding Checklist Presenter

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Onboarding/OnboardingChecklistPresenter.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Onboarding/OnboardingChecklistPresenterTests.cs`

- [x] **Step 1: Write failing presenter tests**

Create tests for incomplete setup, ready setup, and missing microphone advisory states.

- [x] **Step 2: Run focused tests to verify RED**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter OnboardingChecklistPresenterTests
```

Expected: fail because `OnboardingChecklistPresenter` does not exist.

- [x] **Step 3: Implement minimal presenter**

Add immutable presentation records and a `Build` method that maps setup readiness to title, subtitle, progress label, checklist rows, and next action.

- [x] **Step 4: Run focused tests to verify GREEN**

Run the same filtered test command.

Expected: pass.

### Task 2: WinUI Dialog Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Replace raw-only status copy with presenter copy**

Use the presenter from `RefreshOnboardingStatus` to set progress, checklist, detailed health, and save readiness text.

- [x] **Step 2: Run focused tests and build**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter OnboardingChecklistPresenterTests
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: tests and build pass.

### Task 3: Docs And Verification

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update tracker and parity spec**

Record the onboarding checklist/progress slice and update the current slice bar.

- [x] **Step 2: Run final verification**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: tests, build, and diff check pass.

- [x] **Step 3: Review and commit**

Review the diff for Critical/Important issues, fix any findings, then commit:

```powershell
git add VoiceInk.Windows docs/superpowers
git commit -m "feat(windows): add onboarding checklist guidance"
```
