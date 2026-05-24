# Paste Last Primitives Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add reusable Windows primitives for macOS-style paste-last original/final and paste-last enhanced actions, then expose them in the current shell.

**Architecture:** Keep action selection in `VoiceInk.Windows.Core` so later global utility shortcuts can reuse it. The Core service reads the newest history item and delegates insertion through `ITextInjectionService`; the shell only invokes the service and reports status.

**Tech Stack:** .NET 10, existing `IHistoryStore`, existing `ITextInjectionService`, WinUI 3 buttons.

---

## Source Notes

- macOS source of truth: `VoiceInk/Services/LastTranscriptionService.swift` and `VoiceInk/Shortcuts/RecordingShortcutManager.swift`.
- Windows docs reference: Microsoft Learn `SendInput` and clipboard guidance underpin the existing `ClipboardTextInjectionService`.

## File Structure

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/LastTranscriptionTextKind.cs`: action enum for final/original and enhanced-preferred behavior.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/LastTranscriptionActionService.cs`: get latest history item, choose text, insert through `ITextInjectionService`, return action result.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/LastTranscriptionActionResult.cs`: result record with success/error/status text.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/LastTranscriptionActionServiceTests.cs`: no-history, final/original, enhanced-preferred, fallback, failed/canceled skip behavior.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: add `Paste Last` and `Paste Last Enhanced` buttons in History section.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: compose and invoke the service.
- Modify docs and this plan with verification status.

## Task 1: Core Service

**Files:**

- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/LastTranscriptionTextKind.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/LastTranscriptionActionResult.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/LastTranscriptionActionService.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/LastTranscriptionActionServiceTests.cs`

- [x] **Step 1: Write failing tests**

Add tests with these behaviors:

```csharp
[Fact]
public async Task PasteLastAsync_InsertsNewestCompletedFinalText()
{
    var newest = new TranscriptionHistoryItem(
        Guid.NewGuid(),
        DateTimeOffset.UtcNow,
        "final cleaned",
        "local-whisper",
        TimeSpan.Zero,
        TimeSpan.Zero,
        originalText: "raw original");
    var history = new FakeHistoryStore([newest]);
    var insertion = new FakeTextInjectionService();
    var service = new LastTranscriptionActionService(history, insertion);

    var result = await service.PasteLastAsync(LastTranscriptionTextKind.Final, CancellationToken.None);

    Assert.True(result.Success);
    Assert.Equal("Last transcription pasted", result.Message);
    Assert.Equal("final cleaned", insertion.InsertedText);
}

[Fact]
public async Task PasteLastAsync_InsertsNewestCompletedEnhancedTextWhenAvailable()
{
    var item = new TranscriptionHistoryItem(
        Guid.NewGuid(),
        DateTimeOffset.UtcNow,
        "final cleaned",
        "local-whisper",
        TimeSpan.Zero,
        TimeSpan.Zero,
        enhancedText: "enhanced text");
    var insertion = new FakeTextInjectionService();
    var service = new LastTranscriptionActionService(new FakeHistoryStore([item]), insertion);

    var result = await service.PasteLastAsync(LastTranscriptionTextKind.EnhancedPreferred, CancellationToken.None);

    Assert.True(result.Success);
    Assert.Equal("Last enhanced transcription pasted", result.Message);
    Assert.Equal("enhanced text", insertion.InsertedText);
}

[Fact]
public async Task PasteLastAsync_FallsBackToFinalTextWhenEnhancedIsMissing()
{
    var item = new TranscriptionHistoryItem(
        Guid.NewGuid(),
        DateTimeOffset.UtcNow,
        "final cleaned",
        "local-whisper",
        TimeSpan.Zero,
        TimeSpan.Zero);
    var insertion = new FakeTextInjectionService();
    var service = new LastTranscriptionActionService(new FakeHistoryStore([item]), insertion);

    var result = await service.PasteLastAsync(LastTranscriptionTextKind.EnhancedPreferred, CancellationToken.None);

    Assert.True(result.Success);
    Assert.Equal("final cleaned", insertion.InsertedText);
}

[Fact]
public async Task PasteLastAsync_ReturnsErrorWhenNoCompletedTranscriptionExists()
{
    var service = new LastTranscriptionActionService(new FakeHistoryStore([]), new FakeTextInjectionService());

    var result = await service.PasteLastAsync(LastTranscriptionTextKind.Final, CancellationToken.None);

    Assert.False(result.Success);
    Assert.Equal("No transcription available", result.Message);
}
```

- [x] **Step 2: Run tests to verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter LastTranscriptionActionServiceTests
```

Expected: build fails because the service types do not exist.

- [x] **Step 3: Implement service**

Implementation rules:

- Query `historyStore.GetLatestCompletedAsync(cancellationToken)`.
- Use the newest persisted row with `Status == Completed`.
- `Final` inserts `item.Text`.
- `EnhancedPreferred` inserts non-empty `item.EnhancedText`, otherwise `item.Text`.
- Empty chosen text returns `No transcription available`.
- Let insertion exceptions flow to the shell for now.

- [x] **Step 4: Run tests to verify GREEN**

Run the same filtered Core test command.

Expected: all `LastTranscriptionActionServiceTests` pass.

- [x] **Step 5: Commit**

Commit message:

```text
feat(windows): add paste-last history primitives
```

## Task 2: Shell Buttons

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add buttons**

Add `Paste Last` and `Paste Last Enhanced` buttons in the History section toolbar.

- [x] **Step 2: Wire actions**

Instantiate `LastTranscriptionActionService(historyStore, textInjectionService)` or equivalent reusable insertion service. Button handlers call the service and report returned messages in the status text.

- [x] **Step 3: Build app**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 errors.

- [x] **Step 4: Commit**

Commit message:

```text
feat(windows): expose paste-last history actions
```

## Task 3: Docs, Review, And Verification

**Files:**

- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/plans/2026-05-24-paste-last-primitives.md`

- [x] **Step 1: Update docs**

Document paste-last final and enhanced-preferred primitives and shell buttons. Keep configurable global shortcuts and retry-last as gaps.

- [x] **Step 2: Run full tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
```

Expected: all tests pass.

- [x] **Step 3: Run Debug x64 build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 errors.

- [x] **Step 4: Request review and fix Important findings**

Review the slice from the plan commit through HEAD. Fix Critical and Important findings before proceeding.

Review result: no Critical or Important findings. Minor findings were addressed by documenting
`GetLatestCompletedAsync` and making enhanced fallback status match the inserted final text.

- [x] **Step 5: Commit docs**

Commit message:

```text
docs(windows): note paste-last actions
```

## Plan Self-Review

Spec coverage:

- Covers paste-last final/original and enhanced-preferred behavior as reusable primitives plus shell access.
- Leaves retry-last, audio-file retry, and configurable global utility shortcuts for later slices.

Placeholder scan:

- No placeholder sections remain.

Type consistency:

- Uses existing `IHistoryStore`, `ITextInjectionService`, `TranscriptionHistoryItem`, and `TranscriptionHistoryStatus` names.
