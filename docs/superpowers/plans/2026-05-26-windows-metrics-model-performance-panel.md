# Windows Metrics Model Performance Panel Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add richer presenter-backed model performance rows and render them in Metrics.

**Architecture:** Extend the existing Core `ModelPerformanceRow` record while preserving existing `DisplayText`. Update WinUI list templates to bind to the richer fields.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Presenter Fields

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/ModelPerformancePresenter.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Metrics/ModelPerformancePresenterTests.cs`

- [x] **Step 1: Write failing tests**

Assert value and status badge for transcription, enhancement, and empty rows.

- [x] **Step 2: Run focused tests to verify RED**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter ModelPerformancePresenterTests
```

Expected: fail because new fields do not exist.

- [x] **Step 3: Implement fields**

Add `PrimaryValue` and `StatusBadge` to `ModelPerformanceRow`.

- [x] **Step 4: Run focused tests to verify GREEN**

Run the same focused command.

Expected: pass.

### Task 2: WinUI Templates

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`

- [x] **Step 1: Add transcription template**

Render title, primary value, subtitle/detail, and status badge.

- [x] **Step 2: Add enhancement template**

Render the same fields for enhancement rows.

- [x] **Step 3: Run focused tests and build**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter ModelPerformancePresenterTests
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: pass.

### Task 3: Docs, Review, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update docs**

Record Metrics model performance panel progress.

- [x] **Step 2: Run final verification**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: pass.

- [x] **Step 3: Review and commit**

Fix Critical/Important findings, then commit.
