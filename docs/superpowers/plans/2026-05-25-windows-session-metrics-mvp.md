# Windows Session Metrics MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add macOS-aligned session metrics persistence, dashboard summary cards, and model/enhancement performance aggregation to the Windows fork.

**Architecture:** Keep metrics in Core as UI-independent records, aggregation helpers, and an `ISessionMetricStore` contract. Implement SQLite persistence in Infrastructure, then wire dictation, file transcription, and history retry to record one metric per completed history item while the WinUI shell reads summaries from the store.

**Tech Stack:** .NET 10, WinUI 3, Microsoft.Data.Sqlite, xUnit.

---

## File Structure

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/SessionMetric.cs` for the persisted metrics record that mirrors macOS `SessionMetric`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/SessionMetricsSummary.cs` for dashboard totals and derived values.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/ModelPerformanceStat.cs` for transcription/enhancement performance rows.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/SessionMetricsAggregator.cs` for pure aggregation logic.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/SessionMetricRecorder.cs` for converting completed history rows into metrics.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/ISessionMetricStore.cs` for persistence.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Metrics/SqliteSessionMetricStore.cs` for SQLite storage and summary queries.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs` to optionally record metrics after completed history save.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/AudioFiles/AudioFileTranscriptionService.cs` to optionally record metrics after completed file transcription save.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryRetryService.cs` to optionally record metrics after completed retry save.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/ShellNavigationPresenter.cs` and `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml(.cs)` to add a Metrics section.
- Add focused tests under `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Metrics` and `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Metrics`.
- Update `README.md` and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## References

- macOS source: `VoiceInk/Models/SessionMetric.swift`, `VoiceInk/Services/SessionMetricRecorder.swift`, `VoiceInk/Views/Metrics/MetricsContent.swift`, `VoiceInk/Views/Metrics/ModelPerformancePanel.swift`.
- Online grounding: Microsoft.Data.Sqlite parameter docs, SQLite aggregate function docs, and Microsoft WinUI `NavigationView` docs.

### Task 1: Core Metrics Records And Aggregation

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/SessionMetric.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/SessionMetricsSummary.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/ModelPerformanceStat.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/SessionMetricsAggregator.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Metrics/SessionMetricsAggregatorTests.cs`

- [ ] **Step 1: Write failing aggregation tests**

Test that two metrics produce total sessions, words, duration, words per minute, keystrokes saved, time saved based on 35 WPM, and transcription/enhancement model performance averages. Use one metric with transcription model `base.en` and one with `large-v3`, with enhancement model names on both.

- [ ] **Step 2: Run tests to verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~Metrics"
```

Expected: compile failure because metrics types do not exist.

- [ ] **Step 3: Implement records and pure aggregation**

Implement:

```csharp
public sealed record SessionMetric(
    Guid Id,
    Guid TranscriptionId,
    DateTimeOffset Timestamp,
    string Source,
    int WordCount,
    TimeSpan AudioDuration,
    string? TranscriptionModelName,
    TimeSpan? TranscriptionDuration,
    double? SpeedFactor,
    string? PowerModeName,
    string? AiEnhancementModelName,
    TimeSpan? EnhancementDuration);
```

Aggregation rules:
- Ignore negative word counts and negative durations by clamping to zero.
- `WordsPerMinute = TotalWords / (TotalAudioDuration.TotalMinutes)` when duration is positive.
- `KeystrokesSaved = TotalWords * 5`.
- `TimeSaved = max((TotalWords / 35 WPM) - TotalAudioDuration, 0)`.
- Transcription model stats ignore rows without a model or non-positive transcription duration.
- Enhancement stats ignore rows without an enhancement model or non-positive enhancement duration.

- [ ] **Step 4: Run tests to verify GREEN**

Run the same Metrics filter and expect the new tests to pass.

### Task 2: Metric Recorder From History

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/SessionMetricRecorder.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/ISessionMetricStore.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Metrics/SessionMetricRecorderTests.cs`

- [ ] **Step 1: Write failing recorder tests**

Cover:
- completed history rows record exactly one metric.
- canceled/failed rows are ignored.
- enhanced text is counted when `EnhancementDuration` is present and `EnhancedText` is non-empty.
- source defaults to `recorder` but supports `audio-file` and `retry`.
- duplicate metrics are skipped through `HasTranscriptionAsync`.
- store failures return a warning instead of throwing into the transcription pipeline.

- [ ] **Step 2: Run tests to verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~SessionMetricRecorder"
```

Expected: compile failure because recorder/store contract does not exist.

- [ ] **Step 3: Implement recorder and store contract**

Implement `ISessionMetricStore` with:

```csharp
Task SaveAsync(SessionMetric metric, CancellationToken cancellationToken);
Task<bool> HasTranscriptionAsync(Guid transcriptionId, CancellationToken cancellationToken);
Task<SessionMetricsSummary> GetSummaryAsync(CancellationToken cancellationToken);
Task<IReadOnlyList<ModelPerformanceStat>> ListTranscriptionModelPerformanceAsync(DateTimeOffset? since, CancellationToken cancellationToken);
Task<IReadOnlyList<ModelPerformanceStat>> ListEnhancementModelPerformanceAsync(DateTimeOffset? since, CancellationToken cancellationToken);
```

Recorder behavior matches macOS `SessionMetricRecorder`: only completed rows record; word count uses enhanced text only when enhancement actually ran; speed factor is audio duration divided by transcription duration.

- [ ] **Step 4: Run tests to verify GREEN**

Run the recorder filter and expect pass.

### Task 3: SQLite Metrics Store

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Metrics/SqliteSessionMetricStore.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Metrics/SqliteSessionMetricStoreTests.cs`

- [ ] **Step 1: Write failing SQLite tests**

Cover:
- save/load summary persists across store recreation.
- duplicate transcription id is idempotently ignored.
- transcription performance returns fastest average processing first.
- enhancement performance filters by `since`.
- existing database schema migrates when optional columns are missing.

- [ ] **Step 2: Run tests to verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~SqliteSessionMetricStore"
```

Expected: compile failure because the store does not exist.

- [ ] **Step 3: Implement SQLite store**

Create `session_metrics` with:
- `id TEXT PRIMARY KEY`
- `transcription_id TEXT NOT NULL UNIQUE`
- `timestamp TEXT NOT NULL`
- `timestamp_utc_ticks INTEGER NOT NULL`
- `source TEXT NOT NULL`
- `word_count INTEGER NOT NULL`
- `audio_duration_ms REAL NOT NULL`
- `transcription_model_name TEXT NULL`
- `transcription_duration_ms REAL NULL`
- `speed_factor REAL NULL`
- `power_mode_name TEXT NULL`
- `ai_enhancement_model_name TEXT NULL`
- `enhancement_duration_ms REAL NULL`

Use parameterized commands for all writes and filters.

- [ ] **Step 4: Run tests to verify GREEN**

Run the Infrastructure metrics filter and expect pass.

### Task 4: Pipeline Wiring And Metrics UI

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/AudioFiles/AudioFileTranscriptionService.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryRetryService.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/ShellNavigationPresenter.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Test: existing dictation/audio/retry/navigation tests plus new recorder assertions.

- [ ] **Step 1: Write failing wiring tests**

Add tests asserting dictation, file transcription, and history retry call the fake metrics store for completed rows and do not call it for canceled rows. Update shell navigation expectation to include `Metrics` after `History`.

- [ ] **Step 2: Run focused tests to verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~DictationControllerTests|FullyQualifiedName~AudioFileTranscriptionServiceTests|FullyQualifiedName~HistoryRetryServiceTests|FullyQualifiedName~ShellNavigationPresenterTests"
```

Expected: failures where metrics are not recorded and navigation lacks Metrics.

- [ ] **Step 3: Implement wiring and WinUI section**

Add an optional `ISessionMetricStore?` parameter to the three services and call `SessionMetricRecorder.RecordAsync(...)` after successful history save. Add `metrics.db` next to `history.db`, instantiate `SqliteSessionMetricStore`, create a `Metrics` sidebar item, show dashboard values, and display transcription/enhancement model performance lists.

- [ ] **Step 4: Run focused tests to verify GREEN**

Run the same focused Core tests and expect pass.

### Task 5: Docs, Review, Verification, Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [ ] **Step 1: Update docs**

Document the Metrics section, `metrics.db`, summary cards, model performance aggregation, and manual smoke path.

- [ ] **Step 2: Request code review**

Dispatch a review subagent focused on metrics persistence idempotency, UI refresh gaps, and whether metrics failures can break transcription.

- [ ] **Step 3: Fix all Critical and Important findings**

Make minimal fixes and rerun focused tests.

- [ ] **Step 4: Run full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

- [ ] **Step 5: Commit slice**

Commit with:

```powershell
git add README.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md docs/superpowers/plans/2026-05-25-windows-session-metrics-mvp.md VoiceInk.Windows
git commit -m "feat(windows): add session metrics dashboard"
```

