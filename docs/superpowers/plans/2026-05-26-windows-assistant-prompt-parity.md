# Windows Assistant Prompt Parity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Align Windows AI Enhancement assistant rendering and predefined prompt templates with the macOS VoiceInk source of truth.

**Architecture:** Keep the work in Core so the WinUI shell continues to consume the existing prompt catalog and renderer. Add assistant-specific context wrapping to `EnhancementPromptRenderer` without changing provider adapters or persistence.

**Tech Stack:** .NET 10, C#, xUnit.

---

### Task 1: Assistant Context Rendering

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementPromptRenderer.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementPromptTests.cs`

- [x] Add a failing test that renders the predefined Assistant prompt with selected text, clipboard text, and vocabulary, then asserts the system message contains one `<CONTEXT_INFORMATION>` section wrapping those context subsections.
- [x] Run `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter EnhancementPromptTests`.
- [x] Implement assistant-only context wrapping while leaving normal transcription-enhancement prompts unchanged.
- [x] Rerun the focused Core enhancement tests.

### Task 2: Predefined Prompt Template Fidelity

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementPromptCatalog.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementPromptTests.cs`

- [x] Add failing assertions that the Default, Chat, Email, and Rewrite prompts include the richer macOS rules for self-corrections, emotive markers, proper email formatting, and rhythmic flow.
- [x] Run the focused Core enhancement tests and confirm the new assertions fail.
- [x] Update predefined prompt text to match the macOS Swift templates closely while keeping existing stable IDs, icons, descriptions, and `UseSystemInstructions` settings.
- [x] Rerun focused Core enhancement tests.

### Task 3: Docs, Verification, Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] Document assistant context wrapping and richer predefined prompt parity.
- [x] Run `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln`.
- [x] Run `& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`.
- [x] Run `git diff --check`.
- [x] Review the diff and commit with `feat(windows): align assistant prompt parity`.
