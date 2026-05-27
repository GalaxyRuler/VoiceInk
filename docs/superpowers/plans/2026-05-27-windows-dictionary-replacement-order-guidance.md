# Windows Dictionary Replacement Order Guidance Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Update Dictionary page guidance so users see the same replacement order the transcript pipeline actually uses.

**Architecture:** Change only Core presenter text and tests. The existing WinUI Dictionary page already renders the presenter-backed rows and accessible names.

**Tech Stack:** .NET 10, xUnit, VoiceInk.Windows.Core.

---

### Task 1: RED Presenter Assertions

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictionary/DictionaryPagePresenterTests.cs`

- [x] **Step 1: Update replacement-order expectations**

Assert that active replacement summary, rule guidance, replacement row detail, provider boundary detail, and accessible names describe replacements running after text formatting.

- [x] **Step 2: Run focused RED**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~DictionaryPagePresenterTests.Present_BuildsMacStyleSectionLabelsAndRows -nr:false -p:UseSharedCompilation=false
```

Expected RED: stale presenter copy says replacements run after transcription/before insertion.

### Task 2: Presenter Copy

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionaryPagePresenter.cs`

- [x] **Step 1: Update scan-friendly guidance**

Change active replacement details to:

- `Runs after text formatting, before final cleanup and insertion.`
- `Enabled replacements run after text formatting and before final cleanup/insertion.`
- Provider boundary: replacements are applied locally after text formatting.

- [x] **Step 2: Run focused GREEN**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~DictionaryPagePresenterTests -nr:false -p:UseSharedCompilation=false
```

Expected GREEN: all Dictionary presenter tests pass.

### Task 3: Docs, Verification, Commit

**Files:**
- Create: `docs/superpowers/specs/2026-05-27-windows-dictionary-replacement-order-guidance.md`
- Create: `docs/superpowers/plans/2026-05-27-windows-dictionary-replacement-order-guidance.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update parity docs**

Record the Dictionary replacement-order guidance slice and update the current slice tracker.

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
git add VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionaryPagePresenter.cs VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictionary/DictionaryPagePresenterTests.cs docs/superpowers/specs/2026-05-27-windows-dictionary-replacement-order-guidance.md docs/superpowers/plans/2026-05-27-windows-dictionary-replacement-order-guidance.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md docs/superpowers/project-completion.md
git commit -m "fix(windows): clarify dictionary replacement order"
```
