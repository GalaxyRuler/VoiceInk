# Dictionary Text Cleanup Parity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Windows core parity for macOS dictionary word replacements, vocabulary prompt rendering, transcription cleanup preferences, and richer history text/status fields.

**Architecture:** Keep all behavior testable in `VoiceInk.Windows.Core`. Add persistence only through interfaces in Core and SQLite/JSON implementations in Infrastructure. Keep WinUI unchanged except for using the richer settings and pipeline behavior through existing composition.

**Tech Stack:** .NET 10, xUnit, Microsoft.Data.Sqlite, existing WinUI 3 solution.

---

## File Structure

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/VocabularyWord.cs`: immutable vocabulary record.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/WordReplacement.cs`: immutable replacement record with enabled flag.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionaryService.cs`: add/validate vocabulary and replacements, render vocabulary prompt text, apply replacements.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IDictionaryStore.cs`: load enabled replacements and vocabulary for the dictation pipeline.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/PunctuationCleanupMode.cs`: cleanup modes matching macOS.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/TextPostProcessingOptions.cs`: include cleanup flags and replacements.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/TextPostProcessor.cs`: implement macOS cleanup order.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`: add cleanup defaults and filler-word defaults.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/TranscriptionHistoryItem.cs`: add original text, enhanced text, status, language, model, prompt, and timing fields with constructor-compatible defaults.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IHistoryStore.cs`: keep existing method names, accept richer history item.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs`: load dictionary data, run richer cleanup, save original/final/status history.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/History/SqliteHistoryStore.cs`: add schema migration for richer columns and map old rows safely.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: compose an empty dictionary store until UI persistence lands.
- Test `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictionary/DictionaryServiceTests.cs`.
- Test `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Text/TextPostProcessorTests.cs`.
- Test `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictation/DictationControllerTests.cs`.
- Test `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/History/SqliteHistoryStoreTests.cs`.
- Modify `README.md`: document cleanup/dictionary core behavior and current UI gap.

## Task 1: Add Core Dictionary Behavior

**Files:**

- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/VocabularyWord.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/WordReplacement.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionaryService.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IDictionaryStore.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictionary/DictionaryServiceTests.cs`

- [ ] **Step 1: Write failing dictionary tests**

Create tests that prove:

- comma-separated vocabulary input is trimmed, deduplicated case-insensitively, and reports duplicate single-word input with the macOS-style message.
- replacement input rejects duplicate variants across existing replacement entries.
- replacement application sorts longest originals first.
- spaced-language replacements use case-insensitive word boundaries.
- CJK/Thai/Hangul/Japanese replacements use substring matching.
- vocabulary prompt renders `Important Vocabulary: word, word`.

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter DictionaryServiceTests
```

Expected: build fails because the dictionary namespace does not exist.

- [ ] **Step 2: Implement dictionary records and service**

Add immutable records with `Id`, text fields, `DateAdded`, and `IsEnabled` for replacements. Implement pure service methods:

```csharp
public static IReadOnlyList<VocabularyWord> AddVocabularyWords(string input, IEnumerable<VocabularyWord> existing, DateTimeOffset now, out string? error)
public static WordReplacement? AddWordReplacement(string original, string replacement, IEnumerable<WordReplacement> existing, DateTimeOffset now, out string? error)
public static string RenderVocabularyPrompt(IEnumerable<VocabularyWord> words)
public static string ApplyReplacements(string text, IEnumerable<WordReplacement> replacements)
```

Use the macOS behavior from `DictionaryService.swift`, `CustomVocabularyService.swift`, and `WordReplacementService.swift`.

- [ ] **Step 3: Add dictionary store contract**

Add:

```csharp
public interface IDictionaryStore
{
    Task<IReadOnlyList<VocabularyWord>> ListVocabularyAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<WordReplacement>> ListReplacementsAsync(CancellationToken cancellationToken);
}
```

The first slice only needs reads in the dictation pipeline.

- [ ] **Step 4: Verify dictionary tests**

Run the same filtered test command. Expected: all `DictionaryServiceTests` pass.

- [ ] **Step 5: Commit dictionary core**

Commit message:

```text
feat(windows): add dictionary core parity
```

## Task 2: Add Cleanup Options To Text Post-Processing

**Files:**

- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/PunctuationCleanupMode.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/TextPostProcessingOptions.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/TextPostProcessor.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Text/TextPostProcessorTests.cs`

- [ ] **Step 1: Write failing cleanup tests**

Add tests for:

- removing XML-like tag blocks and bracketed hallucinations.
- removing default filler words with optional comma/period.
- keeping apostrophe-like characters out of punctuation cleanup as macOS does.
- removing trailing single period but preserving ellipsis.
- lowercasing after punctuation cleanup.
- applying word replacements before cleanup preferences.
- preserving existing trim and trailing-space behavior.

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter TextPostProcessorTests
```

Expected: new tests fail because options and cleanup behavior are missing.

- [ ] **Step 2: Implement cleanup options and processor**

Add `PunctuationCleanupMode` values `Keep`, `RemoveAll`, and `RemoveTrailingPeriod`.

Extend `TextPostProcessingOptions` with:

```csharp
bool AppendTrailingSpace = false
bool RemoveFillerWords = true
IReadOnlyList<string>? FillerWords = null
IReadOnlyList<WordReplacement>? WordReplacements = null
PunctuationCleanupMode PunctuationCleanupMode = PunctuationCleanupMode.Keep
bool LowercaseTranscription = false
```

Implement cleanup in this order:

1. Remove tag blocks.
2. Remove bracketed hallucinations.
3. Remove filler words when enabled.
4. Normalize whitespace and trim.
5. Apply dictionary replacements.
6. Apply punctuation cleanup.
7. Lowercase.
8. Trim and append trailing space if enabled and non-empty.

Add macOS default filler words to `AppSettings`.

- [ ] **Step 3: Verify cleanup tests**

Run the same filtered test command. Expected: all `TextPostProcessorTests` pass.

- [ ] **Step 4: Commit cleanup core**

Commit message:

```text
feat(windows): add transcription cleanup parity
```

## Task 3: Persist Richer History Metadata

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/TranscriptionHistoryItem.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/History/SqliteHistoryStore.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/History/SqliteHistoryStoreTests.cs`

- [ ] **Step 1: Write failing history migration tests**

Add tests that prove:

- new rows persist original text, final text, status, language, model path, provider name, and timing fields.
- existing databases with the MVP schema still load and map `text` to both final text and original text.
- failed/canceled statuses round-trip.

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter SqliteHistoryStoreTests
```

Expected: new tests fail because the schema does not include the richer columns.

- [ ] **Step 2: Implement history item and schema migration**

Keep the current positional constructor source-compatible by adding optional trailing parameters. Add SQLite columns through idempotent `ALTER TABLE` migration. Store timestamps as UTC ISO-8601 plus ticks for ordering.

- [ ] **Step 3: Verify history tests**

Run the same filtered test command. Expected: all `SqliteHistoryStoreTests` pass.

- [ ] **Step 4: Commit richer history**

Commit message:

```text
feat(windows): expand transcription history metadata
```

## Task 4: Wire Dictionary Cleanup Into Dictation

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictation/DictationControllerTests.cs`

- [ ] **Step 1: Write failing controller tests**

Add tests proving:

- the controller loads dictionary entries and passes enabled replacements into text processing.
- original text from the transcription service is preserved in history while final cleaned text is inserted.
- empty final text saves no completed row and inserts nothing.
- history save for a completed row includes status `completed`, language, model path, and provider metadata.

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter DictationControllerTests
```

Expected: new tests fail because `DictationController` does not load dictionary data or save richer metadata.

- [ ] **Step 2: Implement controller wiring**

Add `IDictionaryStore` to the controller constructor. Load vocabulary and replacements before processing. Use replacements in `TextPostProcessingOptions`. Save richer history values from `TranscriptionResult`, `TranscriptionOptions`, and final text. Preserve existing lifecycle and cancellation behavior.

- [ ] **Step 3: Add empty app dictionary store**

Until Dictionary UI persistence exists, compose a private app-level `EmptyDictionaryStore` in `MainWindow.xaml.cs` that returns empty lists. This keeps the app runnable while Core behavior and tests land.

- [ ] **Step 4: Verify controller tests**

Run the same filtered test command. Expected: all `DictationControllerTests` pass.

- [ ] **Step 5: Commit pipeline wiring**

Commit message:

```text
feat(windows): apply dictionary cleanup in dictation pipeline
```

## Task 5: Documentation And Full Verification

**Files:**

- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [ ] **Step 1: Update docs**

Document:

- Windows now has core dictionary cleanup behavior.
- Dictionary UI/import/export/quick-add are still upcoming parity slices.
- The app remains open-source only and has no commercial gating.

- [ ] **Step 2: Run full tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
```

Expected: all tests pass.

- [ ] **Step 3: Run app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds.

- [ ] **Step 4: Commit docs**

Commit message:

```text
docs(windows): add parity inventory and cleanup plan
```

## Plan Self-Review

Spec coverage:

- Covers the first slice from the parity spec: dictionary models, replacements, vocabulary prompt rendering, cleanup preferences, history metadata, and dictation pipeline wiring.
- Does not include UI, import/export, or quick add because those require navigation and settings surfaces from a separate shell/settings slice.

Placeholder scan:

- No incomplete placeholders remain.

Type consistency:

- `VocabularyWord`, `WordReplacement`, `IDictionaryStore`, `PunctuationCleanupMode`, `TextPostProcessingOptions`, and `TranscriptionHistoryItem` are named consistently across tasks.
