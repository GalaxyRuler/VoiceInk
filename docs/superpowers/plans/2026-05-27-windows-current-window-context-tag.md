# Windows Current Window Context Tag Parity Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development` for the prompt contract change. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the Windows-only `<SCREEN_OCR_CONTEXT>` prompt section with macOS-compatible `<CURRENT_WINDOW_CONTEXT>` while preserving ordering.

---

## Task 1: Red Prompt Tests

- [x] Update renderer tests to expect `<CURRENT_WINDOW_CONTEXT>` and reject `<SCREEN_OCR_CONTEXT>`.
- [x] Update pipeline OCR-context test to expect `<CURRENT_WINDOW_CONTEXT>`.
- [x] Run focused enhancement tests and confirm they fail on the old tag.

## Task 2: Implementation

- [x] Change `EnhancementPromptRenderer` OCR context section tags to `<CURRENT_WINDOW_CONTEXT>`.
- [x] Run focused enhancement tests and confirm they pass.

## Task 3: Verification And Commit

- [x] Update context docs and project completion tracker.
- [x] Run focused enhancement tests, full solution tests, full Debug x64 build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [ ] Commit the slice.
