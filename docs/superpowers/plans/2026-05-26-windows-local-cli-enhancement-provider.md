# Windows Local CLI Enhancement Provider Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Local CLI support to AI Enhancement.

**Architecture:** Keep Core UI-independent. Add provider-aware validation for Local CLI command templates. Add an Infrastructure process runner abstraction used by the existing enhancement service when the Local CLI provider is selected.

**Tech Stack:** .NET 10, System.Diagnostics.Process, xUnit, WinUI 3.

---

### Task 1: Core Provider And Validation

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementProviderPresetCatalog.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementConfiguration.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/TextEnhancementPipeline.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementProviderPresetCatalogTests.cs`

- [x] Write failing tests for Local CLI preset, keyless secret behavior, and provider-aware command validation.
- [x] Add Local CLI preset/configuration and provider-aware validation.
- [x] Run focused Core tests.

### Task 2: Local CLI Runner

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Enhancement/OpenAICompatibleTextEnhancementService.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Enhancement/OpenAICompatibleTextEnhancementServiceTests.cs`

- [x] Write failing tests for command request construction, stdout result, timeout/nonzero/empty failures, and no API-key read.
- [x] Implement process runner abstraction and Local CLI branch.
- [x] Run focused Infrastructure tests.

### Task 3: Docs And Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] Update docs and completion tracker.
- [x] Run full tests, Debug x64 build, `git diff --check`, review, and commit.
