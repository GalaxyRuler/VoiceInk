# Windows History Pagination Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add explicit paginated loading to the Windows History window.

**Architecture:** Add Core page/cursor records to `VoiceInk.Windows.Core.History`, extend `IHistoryStore` with a page API, implement stable SQLite cursor pagination, and wire the History WinUI window to append pages through a Load More button. Keep existing list/search methods for older callers.

**Tech Stack:** .NET 10, WinUI 3, SQLite via `Microsoft.Data.Sqlite`, xUnit.

---

### Task 1: SQLite Page API

**Files:**

- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryPage.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryPageCursor.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IHistoryStore.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/History/SqliteHistoryStore.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/History/SqliteHistoryStoreTests.cs`

- [x] **Step 1: Write failing page tests**

Add tests for first page newest-first, next page after cursor, search-aware next page, stable ordering for tied ticks, zero page size, and negative page size.

- [x] **Step 2: Run focused tests red**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~SqliteHistoryStoreTests.ListPageAsync"
```

- [x] **Step 3: Implement page records and interface method**

Add:

```csharp
public sealed record HistoryPageCursor(long CreatedAtUtcTicks, string Id);
public sealed record HistoryPage(
    IReadOnlyList<TranscriptionHistoryItem> Items,
    HistoryPageCursor? NextCursor,
    bool HasMore);
```

and:

```csharp
Task<HistoryPage> ListPageAsync(
    string? query,
    HistoryPageCursor? cursor,
    int pageSize,
    CancellationToken cancellationToken);
```

- [x] **Step 4: Implement SQLite pagination**

Order by `created_at_utc_ticks DESC, id DESC`, fetch `pageSize + 1`, use cursor predicate `(created_at_utc_ticks < $cursor_ticks OR (created_at_utc_ticks = $cursor_ticks AND id < $cursor_id))`, and return a cursor from the last returned item when another page exists.

- [x] **Step 5: Run focused tests green**

Run the same focused command.

### Task 2: History Window Load More UI

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/HistoryWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/HistoryWindow.xaml.cs`
- Modify fake `IHistoryStore` implementations in tests so the app and tests compile.

- [x] **Step 1: Add Load More UI**

Add a `LoadMoreHistoryButton` below `HistoryListView`, with status text handled by existing `StatusTextBlock`.

- [x] **Step 2: Add page state**

Track current query, next cursor, `hasMoreHistory`, and append pages while preserving selected ids.

- [x] **Step 3: Replace fixed 100-row refresh**

Use `ListPageAsync(query, cursor: null, pageSize: 100)` for refresh/search and `ListPageAsync(query, nextCursor, pageSize: 100)` for Load More.

- [x] **Step 4: Build app project**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj -c Debug -p:Platform=x64
```

### Task 3: Docs, Verification, Commit

**Files:**

- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-27-windows-history-pagination.md`
- Modify: `docs/superpowers/plans/2026-05-27-windows-history-pagination.md`

- [x] **Step 1: Update tracker**

Mark History pagination as the current completed slice and increase the History area.

- [x] **Step 2: Run verification**

Run focused SQLite page tests, app project build, full solution tests, Debug x64 build, and `git diff --check`.

- [x] **Step 3: Commit**

Commit with:

```text
feat(windows): add paginated history loading
```
