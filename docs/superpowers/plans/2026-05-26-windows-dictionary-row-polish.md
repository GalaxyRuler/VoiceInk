# Windows Dictionary Row Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add presenter-backed detail/status rows for Dictionary vocabulary and replacements.

**Architecture:** Extend existing `DictionaryPagePresenter` row records; keep IDs and source ordering so WinUI selection still maps to the backing `vocabularyItems` and `replacementItems` collections.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Presenter Rows

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionaryPagePresenter.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictionary/DictionaryPagePresenterTests.cs`

- [x] **Step 1: Write failing tests**

Assert vocabulary detail/status and replacement detail/status fields.

- [x] **Step 2: Run focused tests to verify RED**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter DictionaryPagePresenterTests
```

Expected: fail because new row fields do not exist.

- [x] **Step 3: Implement row fields**

Add detail/status properties to vocabulary and replacement rows.

- [x] **Step 4: Run focused tests to verify GREEN**

Run the same focused command.

Expected: pass.

### Task 2: WinUI Row Templates

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`

- [x] **Step 1: Add vocabulary template**

Show word, detail, and status badge.

- [x] **Step 2: Add replacement template**

Show original/replacement, detail, and enabled/disabled badge.

- [x] **Step 3: Run focused tests and build**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter DictionaryPagePresenterTests
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: pass.

### Task 3: Docs, Review, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update docs**

Record Dictionary row polish progress.

- [x] **Step 2: Run final verification**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: pass.

- [x] **Step 3: Review and commit**

Fix Critical/Important findings, then commit.
