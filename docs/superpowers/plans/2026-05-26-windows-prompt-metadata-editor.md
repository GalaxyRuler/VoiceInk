# Windows Prompt Metadata Editor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Expose icon and description editing for custom AI Enhancement prompts.

**Architecture:** Use the existing `EnhancementPrompt` Core model and persistence path. Keep predefined prompt metadata read-only and only allow trigger-word overrides for predefined prompts.

**Tech Stack:** WinUI 3, existing Core prompt library, xUnit verification.

---

### Task 1: Prompt Metadata Fields

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] Add prompt icon and description fields.
- [x] Load selected prompt icon/description into the editor.
- [x] Save custom prompt icon/description through `EnhancementPromptLibrary.CreateCustomPrompt`.
- [x] Keep predefined prompt metadata read-only.
- [x] Build Debug x64.

### Task 2: Docs And Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] Update docs and completion tracker.
- [x] Run full tests, Debug x64 build, `git diff --check`, review, and commit.
