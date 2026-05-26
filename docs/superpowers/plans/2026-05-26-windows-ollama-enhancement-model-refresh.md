# Windows Ollama Enhancement Model Refresh Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add dynamic local Ollama model refresh for AI Enhancement.

**Architecture:** Infrastructure owns the Ollama `/api/tags` HTTP client and parsing. WinUI uses the client only for the Ollama preset and updates the existing model combo/text box.

**Tech Stack:** .NET 10, HttpClient, System.Text.Json, WinUI 3, xUnit.

---

### Task 1: Ollama Model Client

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Enhancement/OllamaModelCatalogClient.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Enhancement/OllamaModelCatalogClientTests.cs`

- [x] Write failing tests for model parsing, endpoint origin derivation, empty model lists, and HTTP failure.
- [x] Implement `/api/tags` client.
- [x] Run focused tests.

### Task 2: WinUI Refresh Action

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] Add `Refresh Ollama Models` button.
- [x] Enable it only for the Ollama preset while idle.
- [x] Populate model choices and selected text from refreshed local models.
- [x] Build Debug x64.

### Task 3: Docs And Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] Update docs and completion tracker.
- [x] Run full tests, Debug x64 build, `git diff --check`, and commit.
