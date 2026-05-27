# Windows Dictionary Overview Order Guidance Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Align the Dictionary overview summary with the formatting-before-replacement transcript pipeline.

**Architecture:** Update only `DictionaryPagePresenter` overview copy and focused Core presenter tests. The existing WinUI page renders this presenter value without additional UI wiring.

**Tech Stack:** .NET 10, xUnit, VoiceInk.Windows.Core.

---

### Task 1: RED Overview Tests

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictionary/DictionaryPagePresenterTests.cs`

- [x] **Step 1: Update singular overview expectation**

Change the existing macOS-style section test to expect `1 active replacement runs after text formatting`.

- [x] **Step 2: Add plural overview regression**

Add `Present_MultipleActiveReplacements_DescribesOverviewFormattingOrder`, with two enabled replacements and an expected overview of `2 active replacements run after text formatting`.

- [x] **Step 3: Run focused RED**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~DictionaryPagePresenterTests.Present_MultipleActiveReplacements_DescribesOverviewFormattingOrder -nr:false -p:UseSharedCompilation=false
```

Expected RED: the overview says active replacements run after transcription.

### Task 2: Presenter Overview Copy

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionaryPagePresenter.cs`

- [x] **Step 1: Update `ActiveReplacementText`**

Return `after text formatting` for singular and plural active replacement overview strings.

- [x] **Step 2: Run focused GREEN**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~DictionaryPagePresenterTests -nr:false -p:UseSharedCompilation=false
```

Expected GREEN: all Dictionary presenter tests pass.

### Task 3: Docs, Verification, Commit

**Files:**
- Create: `docs/superpowers/specs/2026-05-27-windows-dictionary-overview-order-guidance.md`
- Create: `docs/superpowers/plans/2026-05-27-windows-dictionary-overview-order-guidance.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update parity docs**

Record that Dictionary overview wording now matches the row-level formatting-before-replacement guidance.

- [x] **Step 2: Run full verification**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln -nr:false -p:UseSharedCompilation=false
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64 -nr:false -p:UseSharedCompilation=false
git diff --check
```

- [x] **Step 3: Commit**

Commit with:

```powershell
git add VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionaryPagePresenter.cs VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictionary/DictionaryPagePresenterTests.cs docs/superpowers/specs/2026-05-27-windows-dictionary-overview-order-guidance.md docs/superpowers/plans/2026-05-27-windows-dictionary-overview-order-guidance.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md docs/superpowers/project-completion.md
git commit -m "fix(windows): align dictionary overview order"
```
