# Windows Metrics Filters And Export Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add macOS-style Metrics time filters and a non-destructive CSV export to the Windows Metrics section.

**Architecture:** Keep time range calculation and CSV formatting in Core. Extend the metrics store contract with filtered summary reads, implement the filter in SQLite using existing UTC ticks, then wire a WinUI ComboBox and save picker without adding any reset/delete behavior.

**Tech Stack:** .NET 10, WinUI 3, Microsoft.Data.Sqlite, xUnit.

---

## File Structure

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/SessionMetricsTimeFilter.cs` for Last 7 Days, Last 30 Days, This Year, and All Time.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/MetricsCsvExporter.cs` for local-only summary/performance CSV export.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/ISessionMetricStore.cs` to add filtered summary reads.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Metrics/SqliteSessionMetricStore.cs` to filter summaries by `timestamp_utc_ticks`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml(.cs)` to add the filter ComboBox and Metrics CSV export button.
- Add focused Core and Infrastructure tests.
- Update README and the parity spec.

### Task 1: Core Time Filters And CSV Export

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/SessionMetricsTimeFilter.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/MetricsCsvExporter.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Metrics/SessionMetricsTimeFilterTests.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Metrics/MetricsCsvExporterTests.cs`

- [ ] **Step 1: Write failing tests**

Cover:
- Last 7 Days subtracts seven days from `now`.
- Last 30 Days subtracts thirty days from `now`.
- This Year returns January 1 of the current year in the same offset.
- All Time returns `null`.
- CSV export includes summary, transcription model rows, enhancement model rows, filter label, and escapes commas/quotes/newlines.

- [ ] **Step 2: Run RED**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~SessionMetricsTimeFilter|FullyQualifiedName~MetricsCsvExporter"
```

Expected: compile failure for missing types.

- [ ] **Step 3: Implement Core helpers**

Implement enum-like filter choices with source IDs and display labels matching macOS: `Last 7 Days`, `Last 30 Days`, `This Year`, `All Time`. Implement CSV export as plain UTF-8-ready text returned to the app.

- [ ] **Step 4: Run GREEN**

Run the same focused command and expect pass.

### Task 2: Filtered SQLite Summary

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/ISessionMetricStore.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/DisabledSessionMetricStore.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Metrics/SqliteSessionMetricStore.cs`
- Modify test fakes that implement `ISessionMetricStore`.
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Metrics/SqliteSessionMetricStoreTests.cs`

- [ ] **Step 1: Write failing store test**

Add a test where an old metric and a new metric exist, then `GetSummaryAsync(since, token)` returns only the new metric.

- [ ] **Step 2: Run RED**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~SqliteSessionMetricStore"
```

Expected: compile failure until the filtered summary API exists.

- [ ] **Step 3: Implement filtered summary**

Keep existing `GetSummaryAsync(CancellationToken)` as all-time convenience and add `GetSummaryAsync(DateTimeOffset? since, CancellationToken)`.

- [ ] **Step 4: Run GREEN**

Run the Infrastructure metrics filter and expect pass.

### Task 3: WinUI Metrics Filter And Export

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [ ] **Step 1: Wire controls**

Add `MetricsTimeFilterComboBox` and `ExportMetricsButton` to the Metrics section. Default to Last 7 Days, matching macOS `ModelPerformancePanel`.

- [ ] **Step 2: Implement refresh/export behavior**

Use the selected filter for summary and performance queries. Export the current filtered metrics CSV through `FileSavePicker`, following the existing History export pattern.

- [ ] **Step 3: Verify app build**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj -c Debug -p:Platform=x64
```

Expected: build succeeds.

### Task 4: Review, Verification, Commit

- [ ] **Step 1: Request review**

Ask a reviewer to focus on time range correctness, exported CSV content, non-destructive behavior, and whether refresh/export failures can break other workflows.

- [ ] **Step 2: Fix Critical/Important findings**

Run focused tests after each fix.

- [ ] **Step 3: Run full verification**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

- [ ] **Step 4: Commit**

```powershell
git add README.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md docs/superpowers/plans/2026-05-25-windows-metrics-filters-export.md VoiceInk.Windows
git commit -m "feat(windows): add metrics filters and export"
```

