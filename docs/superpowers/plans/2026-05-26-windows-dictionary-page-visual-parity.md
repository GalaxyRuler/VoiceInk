# Windows Dictionary Page Visual Parity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add macOS-style dictionary page state text, counts, empty states, and replacement row presentation to the Windows Dictionary page.

**Architecture:** Keep storage/sorting/actions unchanged and add a Core `DictionaryPagePresenter` for UI-independent section labels and row records. WinUI assigns presenter output during `RefreshDictionaryAsync`.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Core Dictionary Page Presenter

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionaryPagePresenter.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictionary/DictionaryPagePresenterTests.cs`

- [x] **Step 1: Write failing tests**

Create tests that verify:

- non-empty vocabulary/replacements produce `Vocabulary Words (2)`, `Word Replacements (2)`, no empty text, vocabulary rows, and replacement row display strings with disabled status;
- empty lists produce `Vocabulary Words (0)`, `Word Replacements (0)`, and macOS-style guidance text.

- [x] **Step 2: Run focused tests to verify RED**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter DictionaryPagePresenterTests
```

Expected: fail because `DictionaryPagePresenter` does not exist.

- [x] **Step 3: Implement presenter records and formatting**

Add `DictionaryPagePresentation`, `DictionaryVocabularyRow`, `DictionaryReplacementRow`, and `DictionaryPagePresenter.Present(...)`.

- [x] **Step 4: Run focused tests to verify GREEN**

Run the same focused test command. Expected: pass.

### Task 2: WinUI Dictionary Page Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add hero/description/count/empty controls**

Add dictionary hero description text, section descriptions, count text blocks, and empty-state text blocks.

- [x] **Step 2: Bind presenter output**

In `RefreshDictionaryAsync`, call `DictionaryPagePresenter.Present(...)` and bind row records to the existing list views while keeping `vocabularyItems` and `replacementItems` for command selection.

- [x] **Step 3: Build**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds.

### Task 3: Docs, Review, Verification, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update docs**

Record the Dictionary page visual parity slice and update completion status.

- [ ] **Step 2: Run verification**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: test/build pass; diff check has no content errors.

- [ ] **Step 3: Review and commit**

Fix Critical/Important review findings, then commit:

```powershell
git add VoiceInk.Windows docs
git commit -m "feat(windows): polish dictionary page presentation"
```
