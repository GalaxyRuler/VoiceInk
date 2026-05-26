# Windows Deepgram Query Options Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Support user-supplied advanced Deepgram endpoint query options without conflicting duplicate defaults.

**Architecture:** Keep the behavior inside `DeepgramCloudTranscriptionService.BuildRequestUri`. Do not add new settings fields; use the existing endpoint field as the advanced local configuration surface.

**Tech Stack:** .NET 10, C#, HttpClient, xUnit.

---

### Task 1: Deepgram Query Option Merge

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/DeepgramCloudTranscriptionService.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Transcription/DeepgramCloudTranscriptionServiceTests.cs`

- [x] Add a failing request URI test for existing `smart_format=false`, `language=es`, `diarize_model=latest`, `paragraphs=true`, and `utterances=true`.
- [x] Run focused Deepgram Infrastructure tests and confirm the URI fails because duplicate defaults are appended.
- [x] Add query-parameter detection and skip default `smart_format`/`language` when already supplied.
- [x] Rerun focused Deepgram Infrastructure tests.

### Task 2: Docs, Verification, Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] Document advanced Deepgram query options through the endpoint field.
- [x] Run full solution tests.
- [x] Run Debug x64 build.
- [x] Run `git diff --check`.
- [x] Commit with `feat(windows): preserve deepgram query options`.
