# Windows VAD Threshold Parity Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development` and `superpowers:executing-plans` for this task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Align Windows VAD default speech duration with macOS whisper.cpp VAD.

---

## Task 1: Red Boundary Tests

- [x] Add a failing 200 ms default VAD test.
- [x] Add 250 ms default VAD pass coverage.
- [x] Add explicit override coverage.
- [x] Run focused VAD tests and confirm the 200 ms default test fails.

## Task 2: Implementation

- [x] Change `WavVoiceActivityDetector` default minimum speech duration to 250 ms.
- [x] Run focused VAD tests and confirm they pass.

## Task 3: Verification And Commit

- [x] Update the project completion tracker.
- [x] Run focused VAD tests, dictation controller tests, full solution tests, full Debug x64 build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [ ] Commit the slice.
