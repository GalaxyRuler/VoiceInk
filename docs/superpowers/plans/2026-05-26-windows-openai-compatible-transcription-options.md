# Windows OpenAI-Compatible Transcription Options Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Allow endpoint query `response_format` to control the OpenAI-compatible transcription multipart request.

**Architecture:** Keep this in `OpenAICompatibleCloudTranscriptionService`; parse only the endpoint query option needed for response format and keep the default path unchanged.

**Tech Stack:** .NET 10, C#, HttpClient, xUnit.

---

### Task 1: Response Format Override

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/OpenAICompatibleCloudTranscriptionService.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Transcription/OpenAICompatibleCloudTranscriptionServiceTests.cs`

- [x] Add failing test where endpoint query contains `response_format=verbose_json` and the multipart body uses `verbose_json`.
- [x] Run focused OpenAI-compatible Infrastructure tests and confirm failure.
- [x] Parse the endpoint query and use the configured response format with `json` fallback.
- [x] Rerun focused OpenAI-compatible Infrastructure tests.

### Task 2: Docs, Verification, Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] Document endpoint query `response_format` support.
- [x] Run full solution tests.
- [x] Run Debug x64 build.
- [x] Run `git diff --check`.
- [x] Commit with `feat(windows): support transcription response format option`.
