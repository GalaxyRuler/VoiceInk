# Windows Dictionary Overview Guidance Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a Core-backed Dictionary overview for vocabulary, replacements, disabled rules, and local import/export guidance.

**Architecture:** Extend `DictionaryPagePresenter` with overview strings. The WinUI Dictionary section reads those strings from its existing refresh path.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Presenter Tests And Core Copy

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionaryPagePresenter.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictionary/DictionaryPagePresenterTests.cs`

- [x] **Step 1: Add failing tests**

Assert overview text for empty, active, and disabled replacement states.

- [x] **Step 2: Run focused tests to verify RED**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter DictionaryPagePresenterTests
```

Expected: fail because the new overview properties do not exist.

- [x] **Step 3: Implement presenter properties**

Add overview summary and local backup guidance to `DictionaryPagePresentation`.

- [x] **Step 4: Run focused tests to verify GREEN**

Run the same filtered command.

Expected: pass.

### Task 2: WinUI Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add overview text blocks**

Place them under the Dictionary hero description.

- [x] **Step 2: Populate from existing refresh path**

Set the overview strings wherever dictionary page presentation is applied.

- [x] **Step 3: Run focused test and build**

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

Record dictionary overview guidance.

- [x] **Step 2: Run final verification**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: pass.

- [x] **Step 3: Review and commit**

Fix Critical/Important findings, then commit.
