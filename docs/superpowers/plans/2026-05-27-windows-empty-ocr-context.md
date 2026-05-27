# Windows Empty OCR Context Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Preserve macOS-style current-window context when OCR finds no text.

---

## Task 1: Red OCR Reader Test

- [x] Add an OCR reader test for non-empty capture bytes plus whitespace recognizer output.
- [x] Run the focused OCR reader test and confirm it fails because the reader returns empty text.

## Task 2: OCR Reader Implementation

- [x] Return `No text detected via OCR` only when capture succeeded but recognition is empty.
- [x] Keep no-capture and empty-region behavior unchanged.

## Task 3: Verification And Commit

- [x] Run focused OCR reader tests, full solution tests, Debug x64 build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [x] Commit the slice.
