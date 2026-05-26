# Windows Ollama Native Enhancement Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Route the Ollama AI Enhancement provider through Ollama's native `/api/chat` endpoint.

**Architecture:** Keep the existing provider preset and `OpenAICompatibleTextEnhancementService` entry point, but branch request/response construction for the Ollama provider just as Anthropic already uses a provider-specific native API shape.

**Tech Stack:** .NET 10, HttpClient, System.Text.Json, xUnit.

---

### Task 1: Native Ollama Request Path

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementProviderPresetCatalog.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Enhancement/OpenAICompatibleTextEnhancementService.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Enhancement/OpenAICompatibleTextEnhancementServiceTests.cs`

- [x] Add failing test for native `/api/chat` request body and response parsing.
- [x] Update Ollama preset endpoint to `/api/chat`.
- [x] Add Ollama request/response branch with `stream: false`.
- [x] Run focused enhancement tests.

### Task 2: Docs And Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] Update docs and completion tracker.
- [ ] Run full tests, Debug x64 build, `git diff --check`, review, and commit.
