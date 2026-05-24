# History Search And Delete Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add macOS-style search filtering and confirmed delete for Windows transcription history.

**Architecture:** Extend `IHistoryStore` with structured search and delete operations. Keep SQLite query/delete implementation in Infrastructure with parameters. Keep WinUI shell responsible for search box state, selection refresh, and destructive confirmation through `ContentDialog`.

**Tech Stack:** .NET 10, WinUI 3 `ContentDialog`, Microsoft.Data.Sqlite parameterized commands, existing SQLite history store.

---

## Source Notes

- macOS source of truth: `VoiceInk/Views/History/InlineHistoryView.swift` filters history by transcript/enhanced text and confirms deletion before removing selected transcriptions.
- Windows docs reference: Microsoft Learn WinUI `ContentDialog` confirmation guidance and Microsoft.Data.Sqlite parameterized command examples.

## File Structure

- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IHistoryStore.cs`: add `SearchAsync` and `DeleteAsync`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/History/SqliteHistoryStore.cs`: add parameterized search and delete operations.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/History/SqliteHistoryStoreTests.cs`: add search and delete regression tests.
- Modify fake stores in Core tests to satisfy the expanded interface.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: add history search box and delete button.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: search refresh, delete confirmation, selection cleanup.
- Modify README/spec/plan docs.

## Task 1: Store Search And Delete

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IHistoryStore.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/History/SqliteHistoryStore.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/History/SqliteHistoryStoreTests.cs`
- Modify: Core test fakes implementing `IHistoryStore`.

- [x] **Step 1: Write failing tests**

Add tests:

```csharp
[Fact]
public async Task SearchAsync_MatchesFinalOriginalEnhancedAndMetadata()
{
    using var temp = new TempDirectory();
    var store = new SqliteHistoryStore(Path.Combine(temp.Path, "history.db"));
    var match = new TranscriptionHistoryItem(
        Guid.NewGuid(),
        DateTimeOffset.UtcNow,
        "final VoiceInk text",
        "local-whisper",
        TimeSpan.Zero,
        TimeSpan.Zero,
        originalText: "raw dictated text",
        enhancedText: "polished transcript",
        language: "en",
        modelPath: "C:\\Models\\ggml-base.en.bin");
    var miss = match with { Id = Guid.NewGuid(), Text = "unrelated", OriginalText = "other", EnhancedText = null };

    await store.SaveAsync(miss, CancellationToken.None);
    await store.SaveAsync(match, CancellationToken.None);

    var results = await store.SearchAsync("polished", 10, CancellationToken.None);

    var item = Assert.Single(results);
    Assert.Equal(match, item);
}

[Fact]
public async Task DeleteAsync_RemovesOnlyMatchingItem()
{
    using var temp = new TempDirectory();
    var store = new SqliteHistoryStore(Path.Combine(temp.Path, "history.db"));
    var deleted = new TranscriptionHistoryItem(Guid.NewGuid(), DateTimeOffset.UtcNow, "delete me", "local-whisper", TimeSpan.Zero, TimeSpan.Zero);
    var kept = deleted with { Id = Guid.NewGuid(), Text = "keep me" };

    await store.SaveAsync(deleted, CancellationToken.None);
    await store.SaveAsync(kept, CancellationToken.None);

    Assert.True(await store.DeleteAsync(deleted.Id, CancellationToken.None));
    Assert.False(await store.DeleteAsync(deleted.Id, CancellationToken.None));

    var results = await store.ListRecentAsync(10, CancellationToken.None);
    var item = Assert.Single(results);
    Assert.Equal(kept, item);
}
```

- [x] **Step 2: Verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "SearchAsync|DeleteAsync"
```

Expected: build fails because store methods do not exist.

- [x] **Step 3: Implement store methods**

Rules:

- `SearchAsync("", limit, token)` delegates to `ListRecentAsync(limit, token)`.
- Search uses `LIKE $query ESCAPE '\'` with escaped `%`, `_`, and `\`.
- Search fields: final text, original text, enhanced text, provider name, language, model path, prompt name, error message.
- Preserve newest-first ordering.
- `DeleteAsync(Guid id, token)` returns true when one row is deleted.

- [x] **Step 4: Verify GREEN**

Run the same filtered Infrastructure test command.

Expected: targeted tests pass.

- [x] **Step 5: Commit**

Commit message:

```text
feat(windows): add history search and delete store methods
```

## Task 2: Shell Search And Confirmed Delete

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add controls**

Add a history toolbar `TextBox` named `HistorySearchTextBox` with placeholder `Search transcriptions...`, a `Search` button, a `Clear` button, and a `Delete` button.

- [x] **Step 2: Wire behavior**

Implementation rules:

- `Search` refreshes history using `historyStore.SearchAsync(HistorySearchTextBox.Text, 50, token)`.
- `Clear` clears search text and refreshes recent history.
- `Delete` requires a selected history row, opens a `ContentDialog` with `XamlRoot = Content.XamlRoot`, primary button `Delete`, secondary `Cancel`, and only deletes after primary result.
- After delete, refresh the current search and clear details if the deleted row was selected.

- [x] **Step 3: Build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 errors.

- [x] **Step 4: Commit**

Commit message:

```text
feat(windows): add history search and delete controls
```

## Task 3: Docs, Review, And Verification

**Files:**

- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/plans/2026-05-24-history-search-delete.md`

- [x] **Step 1: Update docs**

Document history search and confirmed delete. Keep retry-last, audio playback, batch actions, and picker-based export as gaps.

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

Review the slice from this plan commit through HEAD. Fix Critical and Important findings before proceeding.

Review result: no Critical or Important findings. Minor findings were addressed by updating
the fresh verification count and adding `_` plus backslash LIKE escape coverage.

- [x] **Step 5: Commit docs**

Commit message:

```text
docs(windows): note history search and delete
```

## Plan Self-Review

Spec coverage:

- Covers search and confirmed single-item delete. Does not cover batch delete, retry, or playback.

Placeholder scan:

- No placeholder sections remain.

Type consistency:

- Uses `SearchAsync`, `DeleteAsync`, `HistorySearchTextBox`, and existing history store naming consistently.
