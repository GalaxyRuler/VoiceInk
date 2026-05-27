# Windows Transcript Formatting Commands Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make local transcript text formatting respect spoken `new line` and `new paragraph` commands.

**Architecture:** Extend `TextPostProcessor.ApplyTextFormatting` with a deterministic command replacement pass before sentence chunking. Keep the behavior gated behind the existing `ApplyTextFormatting` option so literal transcripts are unchanged when formatting is off.

**Tech Stack:** .NET 10, xUnit, VoiceInk.Windows.Core.

---

### Task 1: RED Text Formatting Command Tests

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Text/TextPostProcessorTests.cs`

- [x] **Step 1: Add enabled formatting-command test**

Add `Process_AppliesDictationLineAndParagraphCommandsWhenFormattingIsEnabled`, expecting `new line` to become `\n` and `new paragraph` to become `\n\n`.

- [x] **Step 2: Add disabled formatting-command test**

Add `Process_KeepsDictationLineAndParagraphCommandsWhenFormattingIsDisabled`, expecting the literal words to remain when `ApplyTextFormatting` is false.

- [x] **Step 3: Run focused RED**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~TextPostProcessorTests.Process_AppliesDictationLineAndParagraphCommandsWhenFormattingIsEnabled -nr:false -p:UseSharedCompilation=false
```

Expected RED: output still contains literal `new line` and `new paragraph`.

### Task 2: Local Formatting Command Pass

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/TextPostProcessor.cs`

- [x] **Step 1: Add command regexes**

Add compiled, culture-invariant, case-insensitive regexes for word-boundary `new paragraph` and `new line` commands.

- [x] **Step 2: Apply commands before sentence chunking**

Call a small `ApplyDictationFormattingCommands` helper at the start of `ApplyTextFormatting`, replacing `new paragraph` first, then `new line`.

- [x] **Step 3: Run focused GREEN**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~TextPostProcessorTests -nr:false -p:UseSharedCompilation=false
```

Expected GREEN: all text post-processor tests pass.

### Task 3: Docs, Verification, Commit

**Files:**
- Create: `docs/superpowers/specs/2026-05-27-windows-transcript-formatting-commands.md`
- Create: `docs/superpowers/plans/2026-05-27-windows-transcript-formatting-commands.md`
- Modify: `docs/superpowers/specs/2026-05-27-windows-transcript-text-formatting.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update parity docs**

Record local formatting-command support and correct the transcript text-formatting spec to the current formatting-before-replacement order.

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
git add VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/TextPostProcessor.cs VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Text/TextPostProcessorTests.cs docs/superpowers/specs/2026-05-27-windows-transcript-formatting-commands.md docs/superpowers/plans/2026-05-27-windows-transcript-formatting-commands.md docs/superpowers/specs/2026-05-27-windows-transcript-text-formatting.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md docs/superpowers/project-completion.md
git commit -m "feat(windows): support transcript formatting commands"
```
