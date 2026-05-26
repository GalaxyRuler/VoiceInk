# Windows Transcribe Audio Per File Enhancement Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an explicit selected-item enhancement action for completed Transcribe Audio queue items that can run independently of the global Enhancement toggle.

**Architecture:** Extend the existing History re-enhancement service with a force option so Core owns the global-toggle bypass semantics. WinUI wires the selected queue item to that service and updates the queue item with the returned History row.

**Tech Stack:** .NET 10, WinUI 3, xUnit.

---

### Task 1: Forced Re-Enhancement Contract

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryReenhancementService.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryReenhancementServiceTests.cs`

- [x] **Step 1: Write failing tests**

Add one test showing normal re-enhancement returns `AI enhancement is disabled` when global enhancement is off, and one test showing `forceEnhancement: true` runs with the same settings without persisting the global toggle.

- [x] **Step 2: Run focused tests and confirm failure**

Run: `& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter HistoryReenhancementServiceTests`

Expected: the forced overload/parameter does not exist.

- [x] **Step 3: Implement force option**

Add an optional `bool forceEnhancement = false` parameter. If force is true, call the pipeline with a settings copy where `IsEnhancementEnabled = true`; otherwise preserve current behavior.

- [x] **Step 4: Run focused tests and confirm pass**

Run: `& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter HistoryReenhancementServiceTests`

Expected: all re-enhancement tests pass.

### Task 2: WinUI Selected Queue Enhance Action

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add selected-item Enhance button**

Place an `Enhance` button with Copy/Save in the Transcribe Audio detail pane.

- [x] **Step 2: Wire action**

For the selected completed item, call `historyReenhancementService.ReenhanceAsync(item.HistoryItem, forceEnhancement: true, windowLifetime.Token)`.

- [x] **Step 3: Update queue row**

When enhancement succeeds, replace the selected queue item history with the returned History row, refresh History, refresh queue details, and persist the queue snapshot.

- [x] **Step 4: Verify build**

Run: `& "..\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`

Expected: build succeeds with 0 errors.

### Task 3: Docs And Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update docs**

Document explicit per-file Transcribe Audio enhancement and remove the final Transcribe Audio gap.

- [x] **Step 2: Run full verification**

Run:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "..\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: tests pass, build succeeds with 0 errors, diff check exits 0.

- [ ] **Step 3: Commit**

Commit message: `feat(windows): add transcribe audio per-file enhancement`
