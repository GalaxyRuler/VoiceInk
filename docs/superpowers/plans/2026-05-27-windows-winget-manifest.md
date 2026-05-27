# Windows WinGet Manifest Packaging Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add non-mutating WinGet manifest generation and validation for signed release metadata.

---

## Task 1: Red Packaging Tests

- [x] Add static packaging coverage for `write-winget-manifest.ps1` and `test-winget-manifest.ps1`.
- [x] Add release-readiness coverage for WinGet manifest commands and checklist text.
- [x] Confirm the focused packaging tests fail before implementation.

## Task 2: Scripts And Docs

- [x] Add the WinGet manifest writer under `VoiceInk.Windows\scripts`.
- [x] Add the WinGet manifest validator under `VoiceInk.Windows\scripts`.
- [x] Update release readiness output and README packaging notes.
- [x] Run focused packaging tests and script help/generation/validation smoke.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red focused packaging tests failed before implementation on missing WinGet scripts and release readiness wiring.
- Focused packaging tests passed after implementation: 17 tests.
- `write-winget-manifest.ps1 -Help` and `test-winget-manifest.ps1 -Help` passed.
- Generated a non-mutating fake WinGet manifest under `VoiceInk.Windows\artifacts\winget-smoke` and validated it with `test-winget-manifest.ps1`.
- `test-release-readiness.ps1` passed and printed the WinGet manifest readiness reference.
- Full solution tests passed: Core 746, Infrastructure 266.
- Debug x64 solution build passed with 0 warnings and 0 errors.
- `git diff --check` passed with only LF-to-CRLF normalization warnings.
