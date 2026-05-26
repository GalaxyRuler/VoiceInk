# Windows Settings Data Safety Overview Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Settings data-safety overview copy to clarify local backup, diagnostics, and privacy cleanup behavior.

**Architecture:** Extend `SettingsSectionPresenter` with overview fields. WinUI displays the overview under the Settings hero description.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Presenter Copy

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/SettingsSectionPresenter.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Settings/SettingsSectionPresenterTests.cs`

- [x] **Step 1: Write failing tests**

Assert overview summary and data-safety guidance.

- [x] **Step 2: Run focused tests to verify RED**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter SettingsSectionPresenterTests
```

Expected: fail because new fields do not exist.

- [x] **Step 3: Implement presenter fields**

Add overview summary and data-safety guidance strings.

- [x] **Step 4: Run focused tests to verify GREEN**

Run the same focused command.

Expected: pass.

### Task 2: WinUI Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add overview text blocks**

Place them under the Settings hero description.

- [x] **Step 2: Populate from existing presenter application**

Set overview text in `ApplySettingsSectionPresentation`.

- [x] **Step 3: Run focused test and build**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter SettingsSectionPresenterTests
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: pass.

### Task 3: Docs, Review, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update docs**

Record Settings data-safety overview progress.

- [x] **Step 2: Run final verification**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: pass.

- [x] **Step 3: Review and commit**

Fix Critical/Important findings, then commit.
