# Dictionary Edit And Sorting Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Close the visible Dictionary edit/sorting parity gap by adding macOS-style sort controls and replacement editing/enabled-state management to the Windows shell.

**Architecture:** Core owns sort constants/order logic and replacement update validation. The JSON dictionary store exposes a single update method that replaces an existing `WordReplacement` by id while preserving duplicate-variant rules. WinUI exposes sort combo boxes plus Edit and Enable/Disable actions for the selected replacement, refreshing the existing list views after every mutation.

**Tech Stack:** .NET 10, WinUI 3 `ComboBox`/`ContentDialog`/`CheckBox`, JSON dictionary persistence, xUnit.

---

## Files

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionarySortModes.cs` for macOS raw sort constants.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionarySortService.cs` for vocabulary/replacement ordering.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionaryService.cs` to validate replacement updates.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IWritableDictionaryStore.cs` for `UpdateWordReplacementAsync`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Dictionary/JsonDictionaryStore.cs`.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictionary/DictionarySortServiceTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictionary/DictionaryServiceTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Dictionary/JsonDictionaryStoreTests.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml` and `MainWindow.xaml.cs`.
- Modify `README.md` and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [ ] **Step 1: Mark dictionary edit/sorting active**

Update the parity spec with an active slice note: Windows will add visible sort controls and replacement edit/enabled state management, while the dedicated navigation/sidebar Dictionary page remains a later shell gap.

- [ ] **Step 2: Commit the plan**

Run:

```powershell
git add docs\superpowers\plans\2026-05-24-dictionary-edit-sorting.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "docs(windows): plan dictionary edit sorting"
```

Expected: docs-only commit.

## Task 2: Red Tests

- [ ] **Step 1: Add sort service tests**

Create tests for:

- Vocabulary `wordDesc` returns words sorted Z-A case-insensitively.
- Replacement `replacementDesc` sorts by replacement text Z-A case-insensitively.
- Unknown sort mode falls back to original A-Z for replacements.

- [ ] **Step 2: Add replacement update validation tests**

Add core `DictionaryService.UpdateWordReplacement` tests proving:

- Updating trims original/replacement and preserves id/date.
- Duplicate original variants in other replacement entries return the macOS duplicate message.

- [ ] **Step 3: Add store update tests**

Add infrastructure tests proving:

- `UpdateWordReplacementAsync` edits original/replacement text and disabled state.
- Duplicate variants are rejected while excluding the row being edited.

- [ ] **Step 4: Run red tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~DictionarySortServiceTests|FullyQualifiedName~DictionaryServiceTests"
```

Expected: compile failure because the sort service and update method do not exist.

## Task 3: Core And Store Implementation

- [ ] **Step 1: Add sort constants and service**

Implement:

- `DictionarySortModes.VocabularyWordAscending = "wordAsc"`
- `DictionarySortModes.VocabularyWordDescending = "wordDesc"`
- `DictionarySortModes.ReplacementOriginalAscending = "originalAsc"`
- `DictionarySortModes.ReplacementOriginalDescending = "originalDesc"`
- `DictionarySortModes.ReplacementTextAscending = "replacementAsc"`
- `DictionarySortModes.ReplacementTextDescending = "replacementDesc"`
- `DictionarySortService.SortVocabulary(IEnumerable<VocabularyWord>, string)`
- `DictionarySortService.SortReplacements(IEnumerable<WordReplacement>, string)`

- [ ] **Step 2: Add replacement update validation**

Implement `DictionaryService.UpdateWordReplacement(WordReplacement current, string original, string replacement, bool isEnabled, IEnumerable<WordReplacement> existing, out string? error)` with the same duplicate-token rules as add, excluding `current.Id`. Return `null` with an error for blank original/replacement; otherwise return a replacement record with preserved `Id` and `DateAdded`.

- [ ] **Step 3: Add writable store update method**

Add `UpdateWordReplacementAsync(Guid id, string original, string replacement, bool isEnabled, CancellationToken)` to `IWritableDictionaryStore` and `JsonDictionaryStore`. Return `Word replacement not found.` when the id is missing; return duplicate/validation errors unchanged; save only on successful update.

- [ ] **Step 4: Verify focused tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~DictionarySortServiceTests|FullyQualifiedName~DictionaryServiceTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter FullyQualifiedName~JsonDictionaryStoreTests
```

Expected: selected tests pass.

## Task 4: WinUI Wiring

- [ ] **Step 1: Add sort controls and replacement actions**

Add vocabulary and replacement sort combo boxes. Add `Edit Replacement` and `Enable/Disable` buttons near the replacement list. Add `SelectionChanged` for replacement selection so button labels and enabled states refresh.

- [ ] **Step 2: Sort list rendering**

Update `RefreshDictionaryAsync` to keep selected ids when possible, apply `DictionarySortService`, and render disabled replacements with a clear `[Disabled]` prefix.

- [ ] **Step 3: Implement edit/toggle actions**

`EditSelectedReplacementAsync` opens a `ContentDialog` with original text, replacement text, and enabled checkbox. It uses `UpdateWordReplacementAsync`, keeps the dialog open on validation/duplicate errors, refreshes on success, and preserves selection. `ToggleSelectedReplacementAsync` flips `IsEnabled` through the same store method.

- [ ] **Step 4: Verify app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build passes with 0 warnings/errors.

## Task 5: Review, Docs, Commit

- [ ] **Step 1: Update docs**

Update README/spec implemented dictionary lists. Leave the dedicated macOS-style Dictionary navigation page/richer layout as a remaining shell gap.

- [ ] **Step 2: Full verification**

Run full solution tests and Debug x64 build.

- [ ] **Step 3: Request review**

Request review, fix Critical and Important issues.

- [ ] **Step 4: Commit**

Run:

```powershell
git add README.md VoiceInk.Windows docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "feat(windows): add dictionary edit sorting"
```
