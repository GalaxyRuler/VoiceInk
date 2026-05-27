# Windows Context Render Order Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make Windows enhancement prompts render context sections in macOS-aligned order before Windows-specific metadata and OCR details.

**Architecture:** Keep changes in UI-independent Core prompt rendering and presenter guidance. No provider, storage, or native capture changes are needed.

**Tech Stack:** .NET 10, xUnit, VoiceInk.Windows.Core.

---

### Task 1: Red Test Context Order

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementPromptTests.cs`

- [x] **Step 1: Write the failing test**

Add `Render_AppendsContextSectionsInMacAlignedOrderBeforeVocabulary`, rendering selected text, clipboard, active-window metadata, browser URL, OCR text, and vocabulary together. Assert selected text renders before clipboard, clipboard before active window, active window before browser URL, browser URL before current-window OCR, and OCR before vocabulary.

- [x] **Step 2: Run test to verify it fails**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~EnhancementPromptTests.Render_AppendsContextSectionsInMacAlignedOrderBeforeVocabulary -nr:false -p:UseSharedCompilation=false
```

Expected RED: failure that clipboard context currently renders after active-window metadata.

### Task 2: Renderer And Guidance

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementPromptRenderer.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementContextReadinessPresenter.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementPromptTests.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementContextReadinessPresenterTests.cs`

- [x] **Step 1: Reorder renderer sections**

Change `ContextSections(...)` to concatenate selected text, clipboard, active-window metadata, browser URL, OCR/current-window context, and vocabulary in that order.

- [x] **Step 2: Update order assertions and visible guidance tests**

Rename/update the older Windows-first order tests and the Context Source Order row expectations.

- [x] **Step 3: Run focused enhancement tests**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~EnhancementPromptTests|FullyQualifiedName~EnhancementContextReadinessPresenterTests" -nr:false -p:UseSharedCompilation=false
```

Expected GREEN: all focused renderer/readiness tests pass.

### Task 3: Docs, Verification, Commit

**Files:**
- Create: `docs/superpowers/specs/2026-05-27-windows-context-render-order.md`
- Create: `docs/superpowers/plans/2026-05-27-windows-context-render-order.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update parity docs**

Document the new context render order and move Context features from 97% to 98%.

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
git add VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementPromptRenderer.cs VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementContextReadinessPresenter.cs VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementPromptTests.cs VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementContextReadinessPresenterTests.cs docs/superpowers/specs/2026-05-27-windows-context-render-order.md docs/superpowers/plans/2026-05-27-windows-context-render-order.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md docs/superpowers/project-completion.md
git commit -m "fix(windows): align context render order"
```
