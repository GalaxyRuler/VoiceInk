# Windows Metrics Time Saved Card Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a presenter-backed `Time Saved` dashboard card to Windows Metrics.

**Architecture:** Extend `SessionMetricsDashboardPresenter` only. The WinUI Metrics page already renders dashboard cards from the presenter collection, so adding the card there preserves UI-independent Core behavior and keeps the UI binding unchanged.

**Tech Stack:** .NET 10, xUnit, VoiceInk.Windows.Core.

---

### Task 1: RED Metrics Card Test

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Metrics/SessionMetricsDashboardPresenterTests.cs`

- [x] **Step 1: Add card expectation**

Update `Present_BuildsMacStyleHeroAndMetricCards` so `presentation.Cards` includes a `Time Saved` card before `Keystrokes Saved`, with value `25m 0s` and detail `estimated typing time saved`.

- [x] **Step 2: Add accessible-name expectation**

Update `Present_RowsAndCardsExposeAccessibleNames` so the new card exposes `Time Saved, 25m 0s, estimated typing time saved`.

- [x] **Step 3: Run focused RED**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~SessionMetricsDashboardPresenterTests.Present_BuildsMacStyleHeroAndMetricCards -nr:false -p:UseSharedCompilation=false
```

Expected RED: the card collection skips `Time Saved` and jumps from `Words Per Minute` to `Keystrokes Saved`.

### Task 2: Presenter Card

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/SessionMetricsDashboardPresenter.cs`

- [x] **Step 1: Add `Time Saved` card**

Insert a card before `Keystrokes Saved`:

```csharp
new(
    "\uE823",
    "Time Saved",
    FormatDuration(summary.TimeSaved, culture),
    "estimated typing time saved",
    "Green")
```

- [x] **Step 2: Run focused GREEN**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~SessionMetricsDashboardPresenterTests -nr:false -p:UseSharedCompilation=false
```

Expected GREEN: all Metrics dashboard presenter tests pass.

### Task 3: Docs, Verification, Commit

**Files:**
- Create: `docs/superpowers/specs/2026-05-27-windows-metrics-time-saved-card.md`
- Create: `docs/superpowers/plans/2026-05-27-windows-metrics-time-saved-card.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update parity docs**

Record that Metrics now includes a presenter-backed Time Saved dashboard card.

- [x] **Step 2: Run full verification**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln -nr:false -p:UseSharedCompilation=false
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64 -nr:false -p:UseSharedCompilation=false
git diff --check
```

- [x] **Step 3: Commit**

Commit with:

```powershell
git add VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/SessionMetricsDashboardPresenter.cs VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Metrics/SessionMetricsDashboardPresenterTests.cs docs/superpowers/specs/2026-05-27-windows-metrics-time-saved-card.md docs/superpowers/plans/2026-05-27-windows-metrics-time-saved-card.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md docs/superpowers/project-completion.md
git commit -m "feat(windows): add metrics time saved card"
```
