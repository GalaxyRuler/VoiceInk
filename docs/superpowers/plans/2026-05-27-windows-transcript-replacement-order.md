# Windows Transcript Replacement Order Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Align Windows transcript formatting and dictionary replacement order with macOS VoiceInk.

**Architecture:** Keep the behavior in UI-independent Core post-processing. Move only the existing dictionary replacement call so formatting can establish paragraph boundaries before replacements run, while cleanup preferences remain last.

**Tech Stack:** .NET 10, xUnit, VoiceInk.Windows.Core.

---

### Task 1: Regression Test

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Text/TextPostProcessorTests.cs`

- [x] **Step 1: Write the failing test**

Add `Process_AppliesTextFormattingBeforeWordReplacements`, using a five-sentence transcript where formatting inserts a paragraph break after the first four sentences. Configure a replacement that matches exactly the formatted first paragraph.

- [x] **Step 2: Run test to verify it fails**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~TextPostProcessorTests.Process_AppliesTextFormattingBeforeWordReplacements -nr:false -p:UseSharedCompilation=false
```

Expected RED: assertion differs because the output is `First paragraph replaced. Fifth sentence...` without the paragraph break.

### Task 2: Core Ordering

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/TextPostProcessor.cs`

- [x] **Step 1: Move replacement application**

Move the existing `DictionaryService.ApplyReplacements` block after `ApplyTextFormatting(processed)` and before `ApplyPunctuationCleanup(...)`.

- [x] **Step 2: Run focused test to verify it passes**

Run the same focused test command.

Expected GREEN: one test passes.

### Task 3: Docs, Verification, Commit

**Files:**
- Create: `docs/superpowers/specs/2026-05-27-windows-transcript-replacement-order.md`
- Create: `docs/superpowers/plans/2026-05-27-windows-transcript-replacement-order.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update parity docs**

Record the macOS order and Windows completion note. Move the Core dictation pipeline tracker from 97% to 98%.

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
git add VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/TextPostProcessor.cs VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Text/TextPostProcessorTests.cs docs/superpowers/specs/2026-05-27-windows-transcript-replacement-order.md docs/superpowers/plans/2026-05-27-windows-transcript-replacement-order.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md docs/superpowers/project-completion.md
git commit -m "fix(windows): align transcript replacement order"
```
