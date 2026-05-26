# Windows Transcribe Audio Copy Save Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add selected-row copy and save actions for completed Transcribe Audio queue items.

**Architecture:** Keep action text and export formatting in Core so it is testable without UI automation. WinUI only wires buttons to the existing clipboard abstraction and Windows App SDK save picker.

**Tech Stack:** .NET 10, WinUI 3, Windows App SDK `FileSavePicker`, `FileIO`, xUnit.

---

### Task 1: Core Action Text And Export Formatting

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/AudioFiles/AudioFileQueueTextActions.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/AudioFiles/AudioFileQueueTextActionsTests.cs`

- [x] **Step 1: Write failing tests**

Add tests for completed-item action text, disabled states, sanitized file names, and Markdown export formatting.

- [x] **Step 2: Run focused tests and confirm failure**

Run: `& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter AudioFileQueueTextActionsTests`

Expected: fails because `AudioFileQueueTextActions` does not exist.

- [x] **Step 3: Implement Core helper**

Implement static methods:

- `TryGetActionText(AudioFileQueueItem? item, out string text, out string message)`
- `SuggestFileName(string text)`
- `FormatMarkdown(string text, DateTimeOffset createdAt)`

- [x] **Step 4: Run focused tests and confirm pass**

Run: `& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter AudioFileQueueTextActionsTests`

Expected: all new tests pass.

### Task 2: WinUI Copy And Save Controls

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add controls**

Add `Copy` and `Save` buttons below the selected queue transcript detail.

- [x] **Step 2: Wire copy**

Use `AudioFileQueueTextActions.TryGetActionText` and `textInjectionService.CopyAsync`.

- [x] **Step 3: Wire save**

Use `FileSavePicker` with `.txt` and `.md` choices, then write plain text or Markdown with `FileIO.WriteTextAsync`.

- [x] **Step 4: Refresh enablement**

Enable both buttons only when the selected item has action text and the app is not busy.

- [x] **Step 5: Verify build**

Run: `& "..\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`

Expected: build succeeds with 0 errors.

### Task 3: Docs And Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update docs**

Document selected-row Transcribe Audio copy/save behavior and remove per-file copy/save from the current gap list.

- [x] **Step 2: Run full verification**

Run:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "..\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: tests pass, build succeeds with 0 errors, diff check exits 0.

- [ ] **Step 3: Commit**

Commit message: `feat(windows): add transcribe audio copy save actions`
