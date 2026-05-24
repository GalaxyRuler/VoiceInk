# Dictionary Persistence And Shell UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the Windows Dictionary usable by persisting vocabulary words and word replacements, then exposing add/delete controls in the current shell.

**Architecture:** Keep dictionary domain behavior in `VoiceInk.Windows.Core` and put JSON persistence in `VoiceInk.Windows.Infrastructure`. The WinUI shell composes the persistent store once and refreshes simple list controls after each mutation; the dictation pipeline continues to consume the same `IDictionaryStore` abstraction.

**Tech Stack:** .NET 10, WinUI 3, `System.Text.Json`, existing Core dictionary models and `DictationController`.

---

## Source Notes

- macOS source of truth: `VoiceInk/Views/Dictionary/VocabularyView.swift`, `VoiceInk/Views/Dictionary/WordReplacementView.swift`, `VoiceInk/Services/DictionaryService.swift`, and `VoiceInk/Transcription/Processing/WordReplacementService.swift`.
- Windows docs reference: Microsoft Learn `ListView`/`TextBox`/`Button` patterns for simple WinUI list editing.
- JSON docs reference: Microsoft Learn `System.Text.Json` immutable type support for serializing record-based dictionary data.

## File Structure

- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IDictionaryStore.cs`: add write/delete methods needed by UI and future quick-add.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Dictionary/JsonDictionaryStore.cs`: JSON-backed dictionary store with atomic saves and duplicate validation through `DictionaryService`.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Dictionary/JsonDictionaryStoreTests.cs`: persistence, duplicate, and delete behavior tests.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: wrap current shell in a scroll view and add dictionary controls.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: compose the persistent store, load lists, handle add/delete actions, and keep dictation wired to the same store.
- Modify `README.md`: document persistent dictionary editing status and remaining dictionary gaps.
- Modify `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`: update implemented status after the slice.

## Task 1: Writable JSON Dictionary Store

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IDictionaryStore.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Dictionary/JsonDictionaryStore.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Dictionary/JsonDictionaryStoreTests.cs`

- [x] **Step 1: Write failing tests**

Add tests that describe the store API before implementation:

```csharp
[Fact]
public async Task AddVocabularyWordsAsync_PersistsCommaSeparatedWordsAndDeduplicatesWithinInput()
{
    using var temp = new TempDirectory();
    var store = new JsonDictionaryStore(Path.Combine(temp.Path, "dictionary.json"));

    var error = await store.AddVocabularyWordsAsync("VoiceInk, Whisper, voiceink", CancellationToken.None);

    Assert.Null(error);
    var words = await store.ListVocabularyAsync(CancellationToken.None);
    Assert.Equal(["VoiceInk", "Whisper"], words.Select(word => word.Word).ToArray());
}

[Fact]
public async Task AddVocabularyWordsAsync_ReturnsDuplicateErrorForSingleExistingWord()
{
    using var temp = new TempDirectory();
    var store = new JsonDictionaryStore(Path.Combine(temp.Path, "dictionary.json"));

    await store.AddVocabularyWordsAsync("VoiceInk", CancellationToken.None);
    var error = await store.AddVocabularyWordsAsync("voiceink", CancellationToken.None);

    Assert.Equal("'voiceink' is already in the vocabulary", error);
    var words = await store.ListVocabularyAsync(CancellationToken.None);
    Assert.Single(words);
}

[Fact]
public async Task AddWordReplacementAsync_PersistsReplacementAndRejectsDuplicateVariant()
{
    using var temp = new TempDirectory();
    var store = new JsonDictionaryStore(Path.Combine(temp.Path, "dictionary.json"));

    var firstError = await store.AddWordReplacementAsync("Voice ink, Voicing", "VoiceInk", CancellationToken.None);
    var duplicateError = await store.AddWordReplacementAsync("voicing", "VoiceInk", CancellationToken.None);

    Assert.Null(firstError);
    Assert.Equal("'voicing' already exists in word replacements", duplicateError);
    var replacements = await store.ListReplacementsAsync(CancellationToken.None);
    var replacement = Assert.Single(replacements);
    Assert.Equal("Voice ink, Voicing", replacement.OriginalText);
    Assert.Equal("VoiceInk", replacement.ReplacementText);
}

[Fact]
public async Task DeleteVocabularyWordAsync_RemovesMatchingWord()
{
    using var temp = new TempDirectory();
    var store = new JsonDictionaryStore(Path.Combine(temp.Path, "dictionary.json"));
    await store.AddVocabularyWordsAsync("VoiceInk, Whisper", CancellationToken.None);
    var word = (await store.ListVocabularyAsync(CancellationToken.None)).Single(item => item.Word == "VoiceInk");

    await store.DeleteVocabularyWordAsync(word.Id, CancellationToken.None);

    var words = await store.ListVocabularyAsync(CancellationToken.None);
    Assert.Equal(["Whisper"], words.Select(item => item.Word).ToArray());
}

[Fact]
public async Task DeleteReplacementAsync_RemovesMatchingReplacement()
{
    using var temp = new TempDirectory();
    var store = new JsonDictionaryStore(Path.Combine(temp.Path, "dictionary.json"));
    await store.AddWordReplacementAsync("Voice ink", "VoiceInk", CancellationToken.None);
    var replacement = Assert.Single(await store.ListReplacementsAsync(CancellationToken.None));

    await store.DeleteReplacementAsync(replacement.Id, CancellationToken.None);

    Assert.Empty(await store.ListReplacementsAsync(CancellationToken.None));
}
```

- [x] **Step 2: Run tests to verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter JsonDictionaryStoreTests
```

Expected: build or tests fail because `JsonDictionaryStore` and writable store methods do not exist yet.

- [x] **Step 3: Implement the writable contract and store**

Add these methods to `IDictionaryStore`:

```csharp
Task<string?> AddVocabularyWordsAsync(string input, CancellationToken cancellationToken);
Task<string?> AddWordReplacementAsync(string original, string replacement, CancellationToken cancellationToken);
Task DeleteVocabularyWordAsync(Guid id, CancellationToken cancellationToken);
Task DeleteReplacementAsync(Guid id, CancellationToken cancellationToken);
```

Implement `JsonDictionaryStore` with:

- `SemaphoreSlim` to serialize reads and writes.
- Default empty dictionary data when the file is missing.
- Indented JSON.
- Atomic save through a temporary file and `File.Move(..., overwrite: true)`.
- `DictionaryService.AddVocabularyWords` and `DictionaryService.AddWordReplacement` for macOS-matched validation.
- Stable list ordering by word/original text for UI readability.

- [x] **Step 4: Run tests to verify GREEN**

Run the same filtered infrastructure test command.

Expected: all `JsonDictionaryStoreTests` pass.

- [x] **Step 5: Commit**

Commit message:

```text
feat(windows): persist dictionary entries
```

## Task 2: Dictionary Controls In Current Shell

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add shell controls**

Add a `Dictionary` section below cleanup settings with:

- Vocabulary input, add button, list, and remove button.
- Word replacement original input, replacement input, add button, list, and remove button.
- Text that matches macOS terminology: `Vocabulary`, `Word Replacements`, `Original text`, and `Replacement text`.

- [x] **Step 2: Wire store and refresh behavior**

Compose `JsonDictionaryStore` at `%LOCALAPPDATA%\VoiceInk.Windows\dictionary.json`, pass it to `DictationController`, and use it for shell list mutations. Refresh the lists after initialization and after every add/delete.

- [x] **Step 3: Run app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 errors.

- [x] **Step 4: Commit**

Commit message:

```text
feat(windows): add dictionary controls to shell
```

## Task 3: Docs And Slice Verification

**Files:**

- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update docs**

Document that Windows now supports persistent vocabulary and word replacement add/delete in the shell. Keep import/export, quick add shortcut, edit sheet, and polished full dictionary page listed as gaps.

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

- [ ] **Step 4: Commit**

Commit message:

```text
docs(windows): note dictionary editing controls
```

## Plan Self-Review

Spec coverage:

- Covers persistent dictionary storage, shell add/delete controls, and dictation pipeline consumption through the same store.
- Leaves edit/import/export/quick-add for later because this slice must stay independently verifiable.

Placeholder scan:

- No placeholder sections remain.

Type consistency:

- Uses existing `VocabularyWord`, `WordReplacement`, `DictionaryService`, `IDictionaryStore`, and `DictationController` names.
