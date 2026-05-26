# Windows Metrics Dashboard Visual Parity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the Windows Metrics plain summary blob with a macOS-style hero and four-card dashboard presentation.

**Architecture:** Add a UI-independent Core presenter for dashboard display records, then bind those records from the WinUI Metrics section. The existing metrics store, aggregator, CSV export, filters, reset flow, and model performance lists remain unchanged.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Core Metrics Dashboard Presenter

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/SessionMetricsDashboardPresenter.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Metrics/SessionMetricsDashboardPresenterTests.cs`

- [x] **Step 1: Write the failing tests**

Create tests for a non-empty summary and an empty summary. The non-empty case should assert the hero title includes saved time, subtitle includes words and pluralized sessions, and the four macOS metric card titles appear in order. The empty case should assert `No Recorder Sessions Yet` and `Start your first recording to unlock value insights.`

- [x] **Step 2: Run the focused tests to verify RED**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter SessionMetricsDashboardPresenterTests
```

Expected: fail because `SessionMetricsDashboardPresenter` does not exist.

- [x] **Step 3: Implement the presenter**

Create records for `SessionMetricsDashboardPresentation` and `SessionMetricsDashboardCard`, and a static `SessionMetricsDashboardPresenter.Present(string filterLabel, SessionMetricsSummary summary)` method.

- [x] **Step 4: Run the focused tests to verify GREEN**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter SessionMetricsDashboardPresenterTests
```

Expected: pass.

### Task 2: WinUI Metrics Section Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Replace the summary text area with dashboard controls**

Add hero title/subtitle text blocks and a list control for dashboard cards in the Metrics dashboard column. Keep storage and model performance controls intact.

- [x] **Step 2: Assign presenter output during metrics refresh**

In `RefreshMetricsAsync`, call `SessionMetricsDashboardPresenter.Present(filter.Label, summary)`, assign hero text, empty-state hint text, and dashboard card item source.

- [x] **Step 3: Run a build**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds.

### Task 3: Docs and Verification

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update project completion**

Mark the current slice as `Windows metrics dashboard visual parity` and bump Metrics visual status.

- [ ] **Step 2: Run verification**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: tests and build pass; diff check has no content errors.

- [ ] **Step 3: Commit**

Commit with:

```powershell
git add VoiceInk.Windows docs
git commit -m "feat(windows): polish metrics dashboard presentation"
```
