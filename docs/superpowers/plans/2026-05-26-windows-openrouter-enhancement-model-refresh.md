# Windows OpenRouter Enhancement Model Refresh Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add dynamic OpenRouter model refresh for AI Enhancement.

**Architecture:** Infrastructure owns the OpenRouter `/api/v1/models` HTTP client and parsing. WinUI reuses the Enhancement refresh action for both OpenRouter and Ollama, with provider-specific catalog clients behind the window.

**Tech Stack:** .NET 10, HttpClient, System.Text.Json, WinUI 3, xUnit.

---

### Task 1: OpenRouter Model Client

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Enhancement/OpenRouterModelCatalogClient.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Enhancement/OpenRouterModelCatalogClientTests.cs`

- [x] Write failing tests for model parsing, endpoint origin derivation, preferred model ordering input, empty model lists, and HTTP failure.
- [x] Implement `/api/v1/models` client.
- [x] Run focused tests.

### Task 2: WinUI Refresh Action

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] Rename the refresh action to provider-neutral `Refresh Models`.
- [x] Enable it for OpenRouter and Ollama while idle.
- [x] Populate model choices and selected text from refreshed OpenRouter models.
- [x] Preserve the existing Ollama behavior.
- [x] Build Debug x64.

### Task 3: Docs And Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] Update docs and completion tracker.
- [x] Run full tests, Debug x64 build, `git diff --check`, and commit.
