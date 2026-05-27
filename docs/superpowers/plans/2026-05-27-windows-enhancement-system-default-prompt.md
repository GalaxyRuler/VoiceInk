# Windows Enhancement System Default Prompt Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rename the built-in Windows default enhancement prompt to the macOS `System Default` label while preserving stable IDs and behavior.

**Architecture:** Update `EnhancementPromptCatalog` metadata only and cover prompt catalog/rendering behavior in existing Core tests. The app shell and recorder prompt pickers already bind prompt titles from the catalog.

**Tech Stack:** .NET 10, xUnit, VoiceInk.Windows.Core.

---

### Task 1: RED Prompt Metadata Tests

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementPromptTests.cs`

- [x] **Step 1: Update default catalog expectation**

Expect the predefined default prompt to have title `System Default` and description `Default system prompt`.

- [x] **Step 2: Update prompt library metadata expectation**

When persisted predefined prompt overrides are merged, expect the immutable default title/description to return to `System Default` / `Default system prompt`.

- [x] **Step 3: Update renderer prompt-name expectation**

Expect rendering with the default prompt to report prompt name `System Default`.

- [x] **Step 4: Run focused RED**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~EnhancementPromptTests.DefaultCatalog_ContainsStableDefaultAndAssistantPrompts -nr:false -p:UseSharedCompilation=false
```

Expected RED: the catalog still returns title `Default` and the old Windows-specific description.

### Task 2: Catalog Metadata

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementPromptCatalog.cs`

- [x] **Step 1: Rename default prompt metadata**

Set the default prompt title to `System Default` and description to `Default system prompt`. Leave ID, prompt text, icon, trigger words, and system-instruction flag unchanged.

- [x] **Step 2: Run focused GREEN**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~EnhancementPromptTests -nr:false -p:UseSharedCompilation=false
```

Expected GREEN: all Enhancement prompt tests pass.

### Task 3: Docs, Verification, Commit

**Files:**
- Create: `docs/superpowers/specs/2026-05-27-windows-enhancement-system-default-prompt.md`
- Create: `docs/superpowers/plans/2026-05-27-windows-enhancement-system-default-prompt.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update parity docs**

Record that Windows prompt metadata now uses the macOS `System Default` title/description.

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
git add VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementPromptCatalog.cs VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementPromptTests.cs docs/superpowers/specs/2026-05-27-windows-enhancement-system-default-prompt.md docs/superpowers/plans/2026-05-27-windows-enhancement-system-default-prompt.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md docs/superpowers/project-completion.md
git commit -m "fix(windows): align default enhancement prompt"
```
