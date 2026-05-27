# Windows History Enhancement Terminology Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace Windows-only `AI cleanup` History analysis wording with VoiceInk's original `AI enhancement` terminology.

**Architecture:** Change only `HistoryAnalysisPresenter` and focused Core tests. The existing History UI binds these presenter rows and accessible names.

**Tech Stack:** .NET 10, xUnit, VoiceInk.Windows.Core.

---

### Task 1: RED Terminology Test

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryAnalysisPresenterTests.cs`

- [x] **Step 1: Update expected History analysis detail**

Expect `AI enhancement completed in 2s` for an enhanced completed item.

- [x] **Step 2: Run focused RED**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~HistoryAnalysisPresenterTests.Present_CompletedEnhancedItem_ReturnsLocalAnalysisRows -nr:false -p:UseSharedCompilation=false
```

Expected RED: presenter still says `AI cleanup completed in 2s`.

### Task 2: Presenter Copy

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryAnalysisPresenter.cs`

- [x] **Step 1: Replace cleanup wording**

Return `AI enhancement completed` and `AI enhancement completed in <duration>` for enhanced history items.

- [x] **Step 2: Run focused GREEN**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~HistoryAnalysisPresenterTests -nr:false -p:UseSharedCompilation=false
```

Expected GREEN: all History analysis presenter tests pass.

### Task 3: Docs, Verification, Commit

**Files:**
- Create: `docs/superpowers/specs/2026-05-27-windows-history-enhancement-terminology.md`
- Create: `docs/superpowers/plans/2026-05-27-windows-history-enhancement-terminology.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update parity docs**

Record that History analysis now uses macOS-aligned enhancement terminology.

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
git add VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryAnalysisPresenter.cs VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryAnalysisPresenterTests.cs docs/superpowers/specs/2026-05-27-windows-history-enhancement-terminology.md docs/superpowers/plans/2026-05-27-windows-history-enhancement-terminology.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md docs/superpowers/project-completion.md
git commit -m "fix(windows): align history enhancement wording"
```
