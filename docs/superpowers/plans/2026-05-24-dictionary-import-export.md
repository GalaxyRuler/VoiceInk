# Dictionary Import Export Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let Windows users export and import VoiceInk vocabulary and word replacements as JSON.

**Architecture:** Keep JSON backup formatting and merge validation in Core so import behavior is testable without WinUI. Keep file picker UI in the WinUI app, initialized with the owner HWND. Extend the existing JSON dictionary store with export/import methods that merge imported entries through the same duplicate rules used by manual add.

**Tech Stack:** .NET 10, WinUI 3, `System.Text.Json`, existing `JsonDictionaryStore`, Windows `FileOpenPicker`/`FileSavePicker` with `InitializeWithWindow`.

---

## Source Notes

- macOS source of truth: `VoiceInk/Services/ImportExportService.swift` exports `vocabularyWords` as an array of `{ word }` objects and `wordReplacements` as a string dictionary mapping original text to replacement text.
- macOS dictionary services: `VoiceInk/Services/DictionaryService.swift`, `VoiceInk/Views/Dictionary/VocabularyView.swift`, and `VoiceInk/Views/Dictionary/WordReplacementView.swift` keep duplicate validation consistent with manual add.
- Online grounding: Microsoft Learn documents that desktop WinUI file pickers must be associated with an owner HWND before displaying UI; the existing Windows history export already follows that pattern.
- Windows adaptation: this slice imports/exports dictionary data only. It intentionally excludes macOS commercial/account/provider settings and API keys.

## File Structure

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionaryBackup.cs`: backup records and serializer for `version`, `vocabularyWords`, and `wordReplacements`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IWritableDictionaryStore.cs`: add export/import methods.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Dictionary/JsonDictionaryStore.cs`: implement JSON backup export and merge import with existing validation.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictionary/DictionaryBackupTests.cs`: verify macOS-style JSON shape and tolerant import parsing.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Dictionary/JsonDictionaryStoreTests.cs`: verify export/import merge and duplicate handling.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: add dictionary import/export buttons.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: use file pickers, read/write dictionary backups, refresh Dictionary UI, and report statuses.
- Modify `README.md`: document dictionary import/export in Windows MVP scope.
- Modify `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`: mark dictionary import/export as implemented.
- Update this plan with verification and review status.

## Task 1: Core Backup Format

**Files:**

- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionaryBackup.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictionary/DictionaryBackupTests.cs`

- [x] **Step 1: Write failing backup format tests**

Create tests for:

```csharp
var json = DictionaryBackup.Export(
    [
        new VocabularyWord(Guid.NewGuid(), "VoiceInk", DateTimeOffset.UnixEpoch),
        new VocabularyWord(Guid.NewGuid(), "Whisper", DateTimeOffset.UnixEpoch)
    ],
    [
        new WordReplacement(Guid.NewGuid(), "Voice ink, Voicing", "VoiceInk", DateTimeOffset.UnixEpoch),
        new WordReplacement(Guid.NewGuid(), "disabled", "ignored", DateTimeOffset.UnixEpoch, IsEnabled: false)
    ]);

Assert.Contains("\"vocabularyWords\"", json);
Assert.Contains("\"word\": \"VoiceInk\"", json);
Assert.Contains("\"wordReplacements\"", json);
Assert.Contains("\"Voice ink, Voicing\": \"VoiceInk\"", json);
Assert.DoesNotContain("disabled", json);

var parsed = DictionaryBackup.Parse("""
{
  "version": "1.0",
  "vocabularyWords": [{ "word": "VoiceInk" }, { "word": "Whisper" }],
  "wordReplacements": {
    "Voice ink, Voicing": "VoiceInk"
  }
}
""");

Assert.Equal(["VoiceInk", "Whisper"], parsed.VocabularyWords);
Assert.Equal(("Voice ink, Voicing", "VoiceInk"), parsed.WordReplacements.Single());
```

- [x] **Step 2: Verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter DictionaryBackupTests
```

Expected: build fails because `DictionaryBackup` does not exist.

- [x] **Step 3: Implement backup serializer**

Implement:

```csharp
public sealed record DictionaryBackupData(
    IReadOnlyList<string> VocabularyWords,
    IReadOnlyList<(string OriginalText, string ReplacementText)> WordReplacements);
```

`DictionaryBackup.Export(...)` writes indented JSON with:

- `version: "1.0"`
- `vocabularyWords: [{ "word": "<word>" }]`, sorted by word, empty omitted by writing `[]`
- `wordReplacements: { "<original>": "<replacement>" }`, sorted by original, only enabled replacements

`DictionaryBackup.Parse(string json)` reads the same shape, trims blank values, ignores blank vocabulary words, and ignores replacements with blank original or replacement text.

- [x] **Step 4: Verify GREEN**

Run the same filtered Core test command.

Expected: all `DictionaryBackupTests` pass.

- [ ] **Step 5: Commit**

Commit message:

```text
feat(windows): add dictionary backup format
```

## Task 2: Store Export And Merge Import

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IWritableDictionaryStore.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Dictionary/JsonDictionaryStore.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Dictionary/JsonDictionaryStoreTests.cs`

- [x] **Step 1: Write failing store tests**

Add tests:

```csharp
[Fact]
public async Task ExportBackupAsync_WritesCurrentDictionaryAsBackupJson()
{
    using var temp = new TempDirectory();
    var store = new JsonDictionaryStore(Path.Combine(temp.Path, "dictionary.json"));
    await store.AddVocabularyWordsAsync("VoiceInk, Whisper", CancellationToken.None);
    await store.AddWordReplacementAsync("Voice ink", "VoiceInk", CancellationToken.None);

    var json = await store.ExportBackupAsync(CancellationToken.None);

    Assert.Contains("\"vocabularyWords\"", json);
    Assert.Contains("\"word\": \"VoiceInk\"", json);
    Assert.Contains("\"Voice ink\": \"VoiceInk\"", json);
}

[Fact]
public async Task ImportBackupAsync_MergesNewEntriesAndSkipsDuplicates()
{
    using var temp = new TempDirectory();
    var store = new JsonDictionaryStore(Path.Combine(temp.Path, "dictionary.json"));
    await store.AddVocabularyWordsAsync("VoiceInk", CancellationToken.None);
    await store.AddWordReplacementAsync("Voice ink", "VoiceInk", CancellationToken.None);

    var result = await store.ImportBackupAsync("""
    {
      "version": "1.0",
      "vocabularyWords": [{ "word": "voiceink" }, { "word": "WinUI" }],
      "wordReplacements": {
        "voice ink": "VoiceInk",
        "codex": "Codex"
      }
    }
    """, CancellationToken.None);

    Assert.Equal(1, result.ImportedVocabularyCount);
    Assert.Equal(1, result.ImportedReplacementCount);
    Assert.Equal(2, result.SkippedDuplicateCount);
    Assert.Equal(["VoiceInk", "WinUI"], (await store.ListVocabularyAsync(CancellationToken.None)).Select(w => w.Word).ToArray());
    Assert.Equal(["codex", "Voice ink"], (await store.ListReplacementsAsync(CancellationToken.None)).Select(r => r.OriginalText).Order().ToArray());
}
```

- [x] **Step 2: Verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "ExportBackupAsync|ImportBackupAsync"
```

Expected: build fails because the methods and result type do not exist.

- [x] **Step 3: Extend store contract**

Add to `IWritableDictionaryStore`:

```csharp
Task<string> ExportBackupAsync(CancellationToken cancellationToken);
Task<DictionaryImportResult> ImportBackupAsync(string json, CancellationToken cancellationToken);
```

Create `DictionaryImportResult` in Core:

```csharp
namespace VoiceInk.Windows.Core.Dictionary;

public sealed record DictionaryImportResult(
    int ImportedVocabularyCount,
    int ImportedReplacementCount,
    int SkippedDuplicateCount);
```

- [x] **Step 4: Implement store export/import**

In `JsonDictionaryStore`:

- `ExportBackupAsync` loads data under the semaphore and returns `DictionaryBackup.Export(data.Vocabulary, data.Replacements)`.
- `ImportBackupAsync` parses JSON before mutating state.
- Imported vocabulary uses `DictionaryService.AddVocabularyWords` against current vocabulary.
- Imported replacements use `DictionaryService.AddWordReplacement` against current replacements.
- Duplicate errors are counted as skipped duplicates and do not fail the import.
- If at least one item is imported, save once at the end.

- [x] **Step 5: Verify GREEN**

Run the same filtered Infrastructure test command.

Expected: export/import tests pass.

- [x] **Step 6: Commit**

Commit message:

```text
feat(windows): import and export dictionary backups
```

## Task 3: Shell Import Export Buttons

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add Dictionary buttons**

Add `Export Dictionary` and `Import Dictionary` buttons in the Dictionary section next to existing add/remove controls or below the section heading.

- [x] **Step 2: Implement file picker handlers**

Implement:

- `ExportDictionaryButton_Click` calls `ExportDictionaryAsync`.
- `ImportDictionaryButton_Click` calls `ImportDictionaryAsync`.
- Export uses `FileSavePicker`, suggested file name `VoiceInk-dictionary-yyyyMMdd-HHmmss`, type `.json`, initialized with `WindowNative.GetWindowHandle(this)`, and writes `await dictionaryStore.ExportBackupAsync(...)` with `FileIO.WriteTextAsync`.
- Import uses `FileOpenPicker`, JSON file type filter `.json`, initialized with the same HWND, reads with `FileIO.ReadTextAsync`, calls `dictionaryStore.ImportBackupAsync`, refreshes dictionary lists, and reports `Dictionary imported: X vocabulary, Y replacements, Z skipped`.
- Cancel states report `Dictionary export canceled` and `Dictionary import canceled`.

- [x] **Step 3: Build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 errors.

- [ ] **Step 4: Commit**

Commit message:

```text
feat(windows): expose dictionary import export
```

## Task 4: Docs, Review, And Verification

**Files:**

- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/plans/2026-05-24-dictionary-import-export.md`

- [ ] **Step 1: Update docs**

Document dictionary JSON import/export. Keep edit flow, sorting controls, and quick-add shortcut as gaps.

- [ ] **Step 2: Run full tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
```

Expected: all tests pass.

- [ ] **Step 3: Run Debug x64 build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 errors.

- [ ] **Step 4: Request review and fix Important findings**

Review the slice from this plan commit through HEAD. Fix Critical and Important findings before proceeding.

- [ ] **Step 5: Commit docs**

Commit message:

```text
docs(windows): note dictionary import export
```

## Plan Self-Review

Spec coverage:

- Covers dictionary-only import/export using the macOS backup dictionary field names.
- Keeps commercial/account/provider settings out of scope.
- Leaves edit flow, sorting controls, dedicated page polish, and quick-add shortcut for later slices.

Placeholder scan:

- No placeholder sections remain.

Type consistency:

- Uses `DictionaryBackup`, `DictionaryBackupData`, `DictionaryImportResult`, `ExportBackupAsync`, and `ImportBackupAsync` consistently.
