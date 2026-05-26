# Windows Metrics Model Performance Guidance Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a Core presenter for Metrics model performance rows and wire it into the Windows Metrics page.

**Architecture:** Create `ModelPerformancePresenter` in Core Metrics. WinUI receives display rows from the presenter and keeps storage/export unchanged.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Core Presenter

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/ModelPerformancePresenter.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Metrics/ModelPerformancePresenterTests.cs`

- [x] **Step 1: Write failing tests**

Cover transcription performance, enhancement performance, and empty state rows.

- [x] **Step 2: Run focused tests to verify RED**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter ModelPerformancePresenterTests
```

Expected: fail because `ModelPerformancePresenter` does not exist.

- [x] **Step 3: Implement presenter**

Return rows with display text and detail text using `SessionMetricsDashboardPresenter.FormatDuration`.

- [x] **Step 4: Run focused tests to verify GREEN**

Run the same focused command.

Expected: pass.

### Task 2: Metrics Page Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Replace ad hoc row formatters**

Use `ModelPerformancePresenter.PresentTranscription` and `PresentEnhancement` in `RefreshMetricsAsync`.

- [x] **Step 2: Run focused test and build**

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

Record model performance guidance.

- [x] **Step 2: Run final verification**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: pass.

- [x] **Step 3: Review and commit**

Fix Critical/Important findings, then commit.
