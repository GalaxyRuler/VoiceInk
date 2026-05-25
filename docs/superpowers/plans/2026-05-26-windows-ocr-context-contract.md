# Windows OCR Context Contract Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a testable OCR context contract to the Windows enhancement pipeline with graceful native no-op behavior.

**Architecture:** Extend the Core enhancement context/request records and prompt renderer first, then wire Windows native context collection through an injectable `IOcrTextReader`. The default implementation returns empty text until full Windows screen capture and `Windows.Media.Ocr` bitmap recognition are implemented.

**Tech Stack:** .NET 10, C#, xUnit, WinUI 3 solution structure, Windows native integration interfaces.

---

### Task 1: Core OCR Prompt Context

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementContext.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementContextRequest.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementPromptRenderer.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/TextEnhancementPipeline.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementPromptTests.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/TextEnhancementPipelineTests.cs`

- [x] **Step 1: Write failing prompt renderer test**

Add a test that renders an `EnhancementContext(OcrText: "Invoice total $42")`, expects `<SCREEN_OCR_CONTEXT>`, and asserts OCR appears before selected text.

- [x] **Step 2: Write failing pipeline request test**

Update the selected-text pipeline context request expectation to include `IncludeOcr: true`.

- [x] **Step 3: Run focused Core tests and verify failure**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~EnhancementPromptTests|FullyQualifiedName~TextEnhancementPipelineTests"
```

Expected: fail because `OcrText` and `IncludeOcr` are not defined yet.

- [x] **Step 4: Implement minimal Core support**

Add `OcrText` to `EnhancementContext`, `IncludeOcr` to `EnhancementContextRequest`, render OCR context, and request OCR from `TextEnhancementPipeline`.

- [x] **Step 5: Run focused Core tests and verify pass**

Run the same focused Core command. Expected: pass.

### Task 2: Native OCR Reader Hook

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/IOcrTextReader.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/EmptyOcrTextReader.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/WindowsEnhancementContextProvider.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Text/WindowsEnhancementContextProviderTests.cs`

- [x] **Step 1: Write failing native provider tests**

Add tests that verify OCR text is included when requested, skipped when not requested, and failure returns empty OCR text.

- [x] **Step 2: Run focused Infrastructure tests and verify failure**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter WindowsEnhancementContextProviderTests
```

Expected: fail because the OCR reader interface/constructor path does not exist yet.

- [x] **Step 3: Implement native no-op reader and provider wiring**

Add `IOcrTextReader`, `EmptyOcrTextReader`, constructor injection, request gating, graceful exception handling, and pass OCR text into `EnhancementContext`.

- [x] **Step 4: Run focused Infrastructure tests and verify pass**

Run the same focused Infrastructure command. Expected: pass.

### Task 3: Verification, Docs, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Run full tests**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
```

Expected: all tests pass.

- [x] **Step 2: Run full x64 build**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 errors.

- [x] **Step 3: Update completion bar**

Raise Context features to reflect the new OCR contract and set the current slice to complete.

- [ ] **Step 4: Commit**

```powershell
git add docs/superpowers/specs/2026-05-26-windows-ocr-context-contract-design.md docs/superpowers/plans/2026-05-26-windows-ocr-context-contract.md docs/superpowers/project-completion.md VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement VoiceInk.Windows/src/VoiceInk.Windows.Native/Text VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Text
git commit -m "feat(windows): add ocr context contract"
```
