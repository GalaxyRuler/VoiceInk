# Windows WinGet GPL License Metadata Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Replace incorrect MIT WinGet release metadata with GPL metadata.

---

## Task 1: Red Packaging Tests

- [x] Update WinGet packaging asset tests to expect `License: GPL-3.0`.
- [x] Add guards rejecting `License: MIT`.
- [x] Confirm focused tests fail before implementation.

## Task 2: Script Implementation

- [x] Change `write-winget-manifest.ps1` to emit `License: GPL-3.0`.
- [x] Change `test-winget-manifest.ps1` to validate `License: GPL-3.0`.
- [x] Run focused WinGet packaging tests.
- [x] Generate and smoke-validate a sample WinGet manifest.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Online grounding: Microsoft WinGet manifest docs describe `License` and `LicenseUrl` manifest fields for package metadata.
- Red: focused WinGet tests failed because `License: GPL-3.0` was absent from both generator and validator scripts.
- Green: focused WinGet packaging tests passed after replacing the stale MIT metadata.
- Smoke: generated a sample WinGet manifest under `VoiceInk.Windows\artifacts\winget-gpl-smoke` and validated it without running WinGet or installing anything.
- Full test: solution test passed with 771 Core tests and 266 Infrastructure tests.
- Build: Debug x64 solution build passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only line-ending normalization warnings.
