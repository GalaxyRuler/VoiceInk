# Windows Onboarding Setup Stages Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add presenter-backed onboarding setup stages and render them in the first-run dialog.

**Architecture:** Extend `OnboardingChecklistPresenter` with stage presentation rows derived from the existing setup status. Bind the dialog to compact stage display strings while keeping existing checklist and save logic intact.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Presenter Stages

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Onboarding/OnboardingChecklistPresenter.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Onboarding/OnboardingChecklistPresenterTests.cs`

- [x] **Step 1: Write failing tests**

Assert stage rows for incomplete and complete setup.

- [x] **Step 2: Run focused tests to verify RED**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter OnboardingChecklistPresenterTests
```

Expected: fail because stage rows do not exist.

- [x] **Step 3: Implement stage rows**

Add `OnboardingSetupStagePresentation` and populate rows in order.

- [x] **Step 4: Run focused tests to verify GREEN**

Run the same focused command.

Expected: pass.

### Task 2: Dialog Binding

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add setup-stage list**

Show stage display rows above the checklist.

- [x] **Step 2: Refresh stage list with status**

Update stages whenever model path, shortcut, or microphone status refreshes.

- [x] **Step 3: Run focused tests and build**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter OnboardingChecklistPresenterTests
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: pass.

### Task 3: Docs, Review, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update docs**

Record onboarding setup-stage progress.

- [x] **Step 2: Run final verification**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: pass.

- [x] **Step 3: Review and commit**

Fix Critical/Important findings, then commit.
