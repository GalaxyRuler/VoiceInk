# Windows Model Removal Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add safe local model removal parity for Windows AI Models.

---

## Task 1: Red Core Removal Tests

- [x] Add `LocalWhisperModelService.RemoveModel` tests for app-local downloaded files, external imported files, and default-model clearing.
- [x] Run focused model service tests and confirm the removal planner is missing.

## Task 2: Core And UI Implementation

- [x] Add a Core removal result that includes updated models, updated default model path, and optional app-owned file path to delete.
- [x] Wire catalog delete and selected-model remove buttons in WinUI.
- [x] Confirm destructive deletion through `ContentDialog`.
- [x] Delete only the planned app-owned file and never delete external imported files.

## Task 3: Verification And Commit

- [x] Run focused model tests, app build, full solution tests, Debug x64 build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [x] Commit the slice.
