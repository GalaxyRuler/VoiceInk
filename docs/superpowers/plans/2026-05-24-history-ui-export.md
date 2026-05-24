# History UI And CSV Export Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Expose the Windows transcription history metadata that is already stored in SQLite and add a source-runnable CSV export path.

**Architecture:** Keep CSV formatting in `VoiceInk.Windows.Core` so it is independently testable and reusable by future full History pages. Keep the first WinUI integration inside the current shell: load recent rows from the existing `SqliteHistoryStore`, show list/detail metadata, and export a CSV file to a local app-data export directory.

**Tech Stack:** .NET 10, WinUI 3, existing SQLite history store, `System.Text`.

---

## Source Notes

- macOS source of truth: `VoiceInk/Views/History/TranscriptionHistoryView.swift`, `VoiceInk/Views/History/TranscriptionDetailView.swift`, `VoiceInk/Views/Common/TranscriptionInfoPanel.swift`, and `VoiceInk/Services/VoiceInkCSVExportService.swift`.
- Windows docs reference: Microsoft Learn WinUI ListView selection/data display guidance.
- Export docs reference: Microsoft Learn Windows App SDK FileSavePicker docs; this slice intentionally writes to a deterministic local export path and leaves picker-based user-chosen save location for the full History page.

## File Structure

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryCsvExporter.cs`: CSV header, row generation, value escaping, and stable duration/timestamp formatting.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryCsvExporterTests.cs`: tests for rich metadata rows, failed/canceled rows, and CSV escaping.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: add recent history list/detail/export controls.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: keep a shared `SqliteHistoryStore`, refresh recent history, display selected metadata, and export CSV.
- Modify `README.md`: document visible history metadata and local CSV export.
- Modify `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`: update history parity status.

## Task 1: Core CSV Formatter

**Files:**

- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryCsvExporter.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryCsvExporterTests.cs`

- [x] **Step 1: Write failing tests**

Add tests with these behaviors:

```csharp
[Fact]
public void Export_ReturnsMacStyleHeaderAndRichMetadataRows()
{
    var item = new TranscriptionHistoryItem(
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        new DateTimeOffset(2026, 5, 24, 12, 0, 0, TimeSpan.Zero),
        "final text",
        "local-whisper",
        TimeSpan.FromSeconds(4),
        TimeSpan.FromMilliseconds(700),
        originalText: "original text",
        enhancedText: "enhanced text",
        status: TranscriptionHistoryStatus.Completed,
        language: "en",
        modelPath: "C:\\Models\\ggml-base.en.bin",
        promptName: "Default",
        enhancementDuration: TimeSpan.FromMilliseconds(250));

    var csv = HistoryCsvExporter.Export([item]);

    Assert.StartsWith(
        "Original Transcript,Enhanced Transcript,Prompt Name,Transcription Model,Provider,Status,Language,Transcription Time,Enhancement Time,Timestamp,Duration,Error Message",
        csv);
    Assert.Contains("original text,enhanced text,Default,C:\\Models\\ggml-base.en.bin,local-whisper,Completed,en,0.700,0.250,2026-05-24T12:00:00.0000000+00:00,4.000,", csv);
}

[Fact]
public void Export_EscapesCommasQuotesAndNewlines()
{
    var item = new TranscriptionHistoryItem(
        Guid.NewGuid(),
        DateTimeOffset.UnixEpoch,
        "final",
        "local-whisper",
        TimeSpan.Zero,
        TimeSpan.Zero,
        originalText: "hello, \"VoiceInk\"\nworld",
        enhancedText: null);

    var csv = HistoryCsvExporter.Export([item]);

    Assert.Contains("\"hello, \"\"VoiceInk\"\"\nworld\",", csv);
}

[Fact]
public void Export_IncludesFailedAndCanceledMetadata()
{
    var failed = new TranscriptionHistoryItem(
        Guid.NewGuid(),
        DateTimeOffset.UnixEpoch,
        "Transcription Failed: model failed",
        "local-whisper",
        TimeSpan.FromSeconds(2),
        TimeSpan.Zero,
        status: TranscriptionHistoryStatus.Failed,
        errorMessage: "model failed");

    var csv = HistoryCsvExporter.Export([failed]);

    Assert.Contains(",local-whisper,Failed,auto,0.000,,1970-01-01T00:00:00.0000000+00:00,2.000,model failed", csv);
}
```

- [x] **Step 2: Run tests to verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter HistoryCsvExporterTests
```

Expected: build fails because `HistoryCsvExporter` does not exist.

- [x] **Step 3: Implement formatter**

Create a static exporter that:

- Emits one header row.
- Uses `OriginalText`, `EnhancedText`, `PromptName`, `ModelPath`, `ProviderName`, `Status`, `Language`, duration seconds with `0.000`, `CreatedAt` in round-trip `O` format, and `ErrorMessage`.
- Quotes values containing comma, double quote, CR, or LF.
- Doubles embedded double quotes.

- [x] **Step 4: Run tests to verify GREEN**

Run the same filtered Core test command.

Expected: all `HistoryCsvExporterTests` pass.

- [x] **Step 5: Commit**

Commit message:

```text
feat(windows): add history csv exporter
```

## Task 2: Recent History Shell Section

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add history controls**

Add a `History` shell section with:

- `Refresh` button.
- `Export CSV` button.
- Recent transcription `ListView`.
- Read-only detail fields for original, final, enhanced, and metadata.

- [x] **Step 2: Wire store and selection**

Keep one `SqliteHistoryStore` instance on the window, pass it to `DictationController`, refresh the history list after initialization and after recording stop, and update detail text on selection.

- [x] **Step 3: Wire CSV export**

Export the current loaded history rows to:

```text
%LOCALAPPDATA%\VoiceInk.Windows\Exports\VoiceInk-history-YYYYMMdd-HHmmss.csv
```

Set the status text to the exported path.

- [x] **Step 4: Build app**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 errors.

- [x] **Step 5: Commit**

Commit message:

```text
feat(windows): show recent transcription history
```

## Task 3: Docs, Review, And Verification

**Files:**

- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/plans/2026-05-24-history-ui-export.md`

- [x] **Step 1: Update docs**

Document recent history list/detail and local CSV export. Keep search, delete, retry, audio playback, selected batch export, and polished split-view History page as gaps.

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

Review the slice from the docs plan commit through HEAD. Fix Critical and Important findings before proceeding.

- [ ] **Step 5: Commit docs**

Commit message:

```text
docs(windows): note history metadata view
```

## Plan Self-Review

Spec coverage:

- Covers user-visible history list/detail and CSV export over the metadata already persisted in SQLite.
- Leaves destructive delete, retry, paste-last, search pagination, audio playback, and full split-view polish for later slices.

Placeholder scan:

- No placeholder sections remain.

Type consistency:

- Uses existing `TranscriptionHistoryItem`, `TranscriptionHistoryStatus`, `IHistoryStore`, and `SqliteHistoryStore` names.
