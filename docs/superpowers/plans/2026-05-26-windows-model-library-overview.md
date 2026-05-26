# Windows Model Library Overview Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a testable local model library presenter and show its summary on the Windows AI Models page.

**Architecture:** Add a Core presenter over existing `WhisperModelCatalogItem` values. The WinUI page binds presenter strings to simple text blocks.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Core Presenter

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/ModelLibraryOverviewPresenter.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Models/ModelLibraryOverviewPresenterTests.cs`

- [x] **Step 1: Write failing tests**

Cover empty library, populated default model, and unavailable imported model counts.

- [x] **Step 2: Run focused tests to verify RED**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter ModelLibraryOverviewPresenterTests
```

Expected: fail because `ModelLibraryOverviewPresenter` does not exist.

- [x] **Step 3: Implement presenter**

Return title, summary, default model label, and cleanup hint.

- [x] **Step 4: Run focused tests to verify GREEN**

Run the same filtered test command.

Expected: pass.

### Task 2: AI Models Page Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add overview text blocks**

Place overview text above the local model catalog.

- [x] **Step 2: Populate overview from refresh path**

Call the presenter from `RefreshModelCatalogItems`.

- [x] **Step 3: Run focused test and app build**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter ModelLibraryOverviewPresenterTests
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: pass.

### Task 3: Docs And Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update tracker and parity spec**

Record model library overview progress.

- [x] **Step 2: Run final verification**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: tests, build, and diff check pass.

- [x] **Step 3: Review and commit**

Fix any Critical/Important findings, then commit.
