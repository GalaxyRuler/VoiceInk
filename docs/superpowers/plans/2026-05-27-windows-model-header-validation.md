# Windows Model Header Validation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add optional GGML magic-header validation to local model health checks.

---

## Task 1: Red Model Tests

- [x] Add a model health test for a large `.bin` file with a non-GGML header.
- [x] Add presenter coverage for invalid-header repair guidance.
- [x] Run focused model tests and confirm they fail before implementation.

## Task 2: Header Validation Implementation

- [x] Add an invalid-header health status and optional header-reader parameter.
- [x] Validate the little-endian GGML magic header when bytes are supplied.
- [x] Add repair guidance copy that tells the user to import a whisper.cpp GGML model.

## Task 3: Verification And Commit

- [x] Run focused model tests, full solution tests, Debug x64 build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [x] Update the project completion bar and commit the slice.
