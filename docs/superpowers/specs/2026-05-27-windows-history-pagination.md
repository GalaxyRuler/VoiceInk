# Windows History Pagination

## Goal

Remove the silent 100-row cap in the Windows History window by adding macOS-style page loading with an explicit Load More affordance.

## Source Of Truth

- macOS history view: `VoiceInk/Views/History/TranscriptionHistoryView.swift`
- Windows history window: `VoiceInk.Windows/src/VoiceInk.Windows.App/HistoryWindow.xaml`
- Windows history persistence: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/History/SqliteHistoryStore.cs`
- WinUI guidance: Microsoft documents ListView/GridView for selectable item collections and notes that incremental loading remains available in Windows App SDK.

## Requirements

- Keep the existing Windows History window layout and controls.
- Load the first 100 newest rows initially, matching the existing page size.
- Add an explicit Load More button below the history list when more rows are available.
- Support pagination for both normal history and search results.
- Preserve existing selection when appending a page.
- Reset pagination when the user searches, clears search, refreshes, deletes, retries, or re-enhances.
- Use stable newest-first ordering with `(created_at_utc_ticks, id)` so tied timestamps do not skip or duplicate rows.
- Keep export/delete/retry/re-enhance behavior scoped to loaded and selected rows.
- Do not add dependencies or cloud services.

## Non-Goals

- Do not redesign the History window.
- Do not add automatic scroll-triggered loading in this slice.
- Do not change CSV format or history record schema beyond query behavior.
