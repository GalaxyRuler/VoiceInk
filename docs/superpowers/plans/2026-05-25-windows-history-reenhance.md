# Windows History Re-enhance Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a History "Re-enhance Selected" workflow that reruns AI enhancement from saved transcript text without retranscribing audio.

**Architecture:** Implement the behavior in Core as a `HistoryReenhancementService` using existing settings, dictionary, and `TextEnhancementPipeline` abstractions. The WinUI shell adds one button that calls the service, refreshes history, and selects the newly created derived item.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit, existing VoiceInk.Windows Core/App projects.

---

## File Structure

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryReenhancementService.cs`: service that validates the source history item, runs enhancement, and saves a new item.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryReenhancementResult.cs`: simple result DTO mirroring the retry service shape.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryReenhancementServiceTests.cs`: focused TDD coverage for success and failures.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: add `Re-enhance Selected` button beside `Retry Selected`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: instantiate service, add click handler, operation guard, UI enablement, and status messages.
- Modify `docs/superpowers/project-completion.md`: mark the History gap progress.

## Tasks

### Task 1: Core Service Tests

**Files:**
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryReenhancementServiceTests.cs`

- [ ] **Step 1: Write failing tests**

Add tests that prove:
- completed history items are re-enhanced from `OriginalText`, not already-enhanced `Text`;
- disabled enhancement returns "AI enhancement is disabled" and saves nothing;
- missing provider configuration returns the pipeline warning and saves nothing;
- failed/canceled history items return "Only completed transcriptions can be re-enhanced".

- [ ] **Step 2: Run test to verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter HistoryReenhancementServiceTests
```

Expected: compile failure because `HistoryReenhancementService` does not exist.

### Task 2: Core Service Implementation

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryReenhancementResult.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryReenhancementService.cs`

- [ ] **Step 1: Implement result DTO**

Create a record with `Success`, `Message`, and optional `TranscriptionHistoryItem`.

- [ ] **Step 2: Implement service**

Service behavior:
- reject non-completed source rows;
- choose `source.OriginalText` when nonblank, otherwise `source.Text`;
- reject blank text;
- load settings and require `IsEnhancementEnabled`;
- load dictionary vocabulary;
- call `TextEnhancementPipeline.EnhanceAsync`;
- require `AttemptedEnhancement`, no warning, and nonblank `EnhancedText`;
- save a new completed `TranscriptionHistoryItem` with source transcription metadata and new enhancement metadata;
- return success message "Re-enhanced transcription saved".

- [ ] **Step 3: Run focused tests to verify GREEN**

Run the same filtered test command and expect PASS.

### Task 3: WinUI Shell Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [ ] **Step 1: Add button**

Add `ReEnhanceHistoryButton` after `RetryHistoryButton` in the History action row.

- [ ] **Step 2: Instantiate and guard service**

Add a `HistoryReenhancementService` field and `isReenhancingHistory` operation flag. Instantiate it with existing `historyStore`, `settingsStore`, `dictionaryStore`, and `textEnhancementPipeline`.

- [ ] **Step 3: Add click handler**

Save settings, run `ReenhanceAsync`, refresh history, select the new item, refresh metrics best effort, and show the result message.

- [ ] **Step 4: Update enablement**

Enable the new button only when settings are loaded, no operation is active, controller is not recording, and a completed history item is selected.

- [ ] **Step 5: Build app project**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj -c Debug -p:Platform=x64
```

Expected: build succeeds.

### Task 4: Docs, Review, Verification, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [ ] **Step 1: Update completion tracker**

Increase History progress and set current slice to History re-enhance.

- [ ] **Step 2: Run verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: tests/build pass; diff check has no errors.

- [ ] **Step 3: Review**

Review the diff for accidental commercial surfaces, unrelated edits, nullability issues, and operation guards.

- [ ] **Step 4: Commit**

Commit only intended source/docs changes:

```powershell
git add VoiceInk.Windows docs/superpowers
git commit -m "feat(windows): add history re-enhance action"
```
