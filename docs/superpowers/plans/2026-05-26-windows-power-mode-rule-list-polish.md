# Windows Power Mode Rule List Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace flat Power Mode rule list strings with richer presenter-backed rows.

**Architecture:** Extend `PowerModePagePresenter` to return row presentation records alongside existing page copy. Bind the WinUI `ListView` to row objects while keeping selected-index mapping against `powerModeRules`.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Presenter Rows

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/PowerMode/PowerModePagePresenter.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/PowerMode/PowerModePagePresenterTests.cs`

- [x] **Step 1: Write failing tests**

Assert manual switching guidance and a rule row with target, override, shortcut, and status summaries.

- [x] **Step 2: Run focused tests to verify RED**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter PowerModePagePresenterTests
```

Expected: fail because the new row/guidance properties do not exist.

- [x] **Step 3: Implement presenter rows**

Add `PowerModeRuleRowPresentation` and populate rows in rule order.

- [x] **Step 4: Run focused tests to verify GREEN**

Run the same focused command.

Expected: pass.

### Task 2: WinUI Binding

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add row template**

Render title, target, overrides, shortcut, and status badge.

- [x] **Step 2: Bind rows from presenter**

Use `PowerModePagePresentation.RuleRows` as the list source.

- [x] **Step 3: Run focused tests and build**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter PowerModePagePresenterTests
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: pass.

### Task 3: Docs, Review, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update docs**

Record Power Mode rule list polish progress.

- [x] **Step 2: Run final verification**

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: pass.

- [x] **Step 3: Review and commit**

Fix Critical/Important findings, then commit.
