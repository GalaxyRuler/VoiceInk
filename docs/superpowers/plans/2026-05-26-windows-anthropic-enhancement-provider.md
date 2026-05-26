# Windows Anthropic Enhancement Provider Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Anthropic Messages API support to AI Enhancement.

**Architecture:** Keep `ITextEnhancementService` unchanged. Add Anthropic to Core provider configuration and branch inside the existing infrastructure enhancement service for provider-specific request/response shape.

**Tech Stack:** .NET 10, HttpClient, System.Text.Json, Windows Credential Manager abstraction, xUnit.

---

### Task 1: Core Provider Preset

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementProviderPresetCatalog.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementConfiguration.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementProviderPresetCatalogTests.cs`

- [x] Write failing tests for Anthropic preset, secret name, and provider metadata.
- [x] Add Anthropic preset/configuration.
- [x] Run focused Core tests.

### Task 2: Anthropic Messages Request

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Enhancement/OpenAICompatibleTextEnhancementService.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Enhancement/OpenAICompatibleTextEnhancementServiceTests.cs`

- [x] Write failing Infrastructure tests for Anthropic headers/body, response parsing, missing key, and sanitized HTTP failure.
- [x] Implement Anthropic Messages request/response handling.
- [x] Run focused Infrastructure tests.

### Task 3: Docs And Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] Update docs and completion tracker.
- [x] Run full tests, Debug x64 build, `git diff --check`, review, and commit.
