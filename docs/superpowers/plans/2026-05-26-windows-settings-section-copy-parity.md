# Windows Settings Section Copy Parity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add macOS-style Settings section descriptions to the Windows Settings page.

**Architecture:** Add a Core `SettingsSectionPresenter` with static, testable copy. WinUI reads the presenter once while settings load and assigns text blocks near existing controls.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Settings Section Presenter

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/SettingsSectionPresenter.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Settings/SettingsSectionPresenterTests.cs`

- [x] **Step 1: Write failing tests**

Assert that `SettingsSectionPresenter.Present()` returns Settings hero text and sections for Shortcuts, Recording Feedback, Interface, Clipboard, Cleanup, Privacy, General, Backup, and Diagnostics.

- [x] **Step 2: Run focused tests to verify RED**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter SettingsSectionPresenterTests
```

- [x] **Step 3: Implement the presenter**

Add records and static section copy.

- [x] **Step 4: Run focused tests to verify GREEN**

Run the same focused test command.

### Task 2: WinUI Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add description text blocks under Settings section headings**

Add named text blocks for each Settings group.

- [x] **Step 2: Assign presenter output**

Call `ApplySettingsSectionPresentation(SettingsSectionPresenter.Present())` when applying settings to the UI.

- [x] **Step 3: Build**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

### Task 3: Docs, Review, Verification, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update docs**
- [x] **Step 2: Run full tests, build, and `git diff --check`**
- [ ] **Step 3: Review and commit**
