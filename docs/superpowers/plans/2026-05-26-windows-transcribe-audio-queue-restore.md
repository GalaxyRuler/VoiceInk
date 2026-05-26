# Windows Transcribe Audio Queue Restore Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Persist and restore unfinished Transcribe Audio queue items across app restarts.

**Architecture:** Core owns snapshot creation/restoration rules and path validation. Infrastructure owns atomic JSON persistence. WinUI calls load during startup and save after queue mutations.

**Tech Stack:** .NET 10, System.Text.Json, WinUI 3, xUnit.

---

### Task 1: Core Snapshot Rules

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/AudioFiles/AudioFileQueueService.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/AudioFiles/AudioFileQueueSnapshot.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/AudioFiles/AudioFileQueuePersistence.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/AudioFiles/AudioFileQueuePersistenceTests.cs`

- [x] **Step 1: Write failing tests**

Cover pending/failed snapshot creation, completed exclusion, processing-to-pending restoration, missing/unsupported skip, duplicate skip, and failed-error preservation.

- [x] **Step 2: Run focused tests and confirm failure**

Run: `& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter AudioFileQueuePersistenceTests`

Expected: fails because snapshot types do not exist.

- [x] **Step 3: Implement Core snapshot types and validation helpers**

Expose queue path normalization/support checks through `AudioFileQueueService`, then implement `AudioFileQueuePersistence.CreateSnapshot` and `Restore`.

- [x] **Step 4: Run focused tests and confirm pass**

Run: `& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter AudioFileQueuePersistenceTests`

Expected: all new tests pass.

### Task 2: JSON Snapshot Store

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IAudioFileQueueSnapshotStore.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/AudioFiles/JsonAudioFileQueueSnapshotStore.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/AudioFiles/JsonAudioFileQueueSnapshotStoreTests.cs`

- [x] **Step 1: Write failing store tests**

Cover missing file returns empty snapshot, save/load round trip, empty snapshot deletes the file, and malformed JSON returns empty without throwing.

- [x] **Step 2: Run focused tests and confirm failure**

Run: `& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter JsonAudioFileQueueSnapshotStoreTests`

Expected: fails because the store does not exist.

- [x] **Step 3: Implement store**

Use indented `System.Text.Json`, write to a temp file then move over the target, and delete the snapshot file when no items remain.

- [x] **Step 4: Run focused tests and confirm pass**

Run: `& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter JsonAudioFileQueueSnapshotStoreTests`

Expected: all new tests pass.

### Task 3: WinUI Lifecycle Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add queue snapshot path and store field**

Create `audioFileQueueSnapshotPath` under app data and instantiate `JsonAudioFileQueueSnapshotStore`.

- [x] **Step 2: Load on startup**

After app data paths are initialized and settings load starts, read the snapshot, restore valid items through Core, refresh the queue view, and show a nonfatal restore count/status.

- [x] **Step 3: Save after queue mutations**

Persist after add/drop, remove, retry, clear, queue completion/failure changes, cancel reset, and queue processing finalization.

- [x] **Step 4: Verify build**

Run: `& "..\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`

Expected: build succeeds with 0 errors.

### Task 4: Docs And Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update docs**

Document unfinished Transcribe Audio queue restoration and remove the persistent queue restoration gap.

- [x] **Step 2: Run full verification**

Run:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "..\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: tests pass, build succeeds with 0 errors, diff check exits 0.

- [ ] **Step 3: Commit**

Commit message: `feat(windows): restore transcribe audio queue`
