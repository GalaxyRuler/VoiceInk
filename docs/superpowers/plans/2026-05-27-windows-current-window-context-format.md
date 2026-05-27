# Windows Current Window Context Format Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Match macOS current-window context formatting inside enhancement prompts.

---

## Task 1: Red Renderer Test

- [x] Update renderer tests to require `Active Window`, `Application`, and `Window Content` inside `<CURRENT_WINDOW_CONTEXT>`.
- [x] Run the focused enhancement renderer test and confirm it fails with the current raw OCR body.

## Task 2: Renderer Implementation

- [x] Format OCR context body with macOS-style current-window metadata.
- [x] Keep empty OCR contexts omitted.

## Task 3: Verification And Commit

- [x] Run focused enhancement tests, full solution tests, Debug x64 build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [x] Commit the slice.
