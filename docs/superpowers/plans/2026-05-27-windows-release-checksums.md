# Windows Release Checksums Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add non-mutating SHA256 checksum manifest generation for release artifacts.

---

## Task 1: Red Packaging Tests

- [x] Add static packaging coverage for `write-release-checksums.ps1`.
- [x] Add release-readiness coverage for checksum manifest command and checklist text.
- [x] Confirm the focused packaging tests fail before implementation.

## Task 2: Script And Docs

- [x] Add the checksum writer under `VoiceInk.Windows\scripts`.
- [x] Update release readiness output and README packaging notes.
- [x] Run focused packaging tests and script help/smoke checksum generation.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red focused packaging tests failed before implementation on missing checksum helper and release readiness wiring.
- Focused packaging tests passed after implementation: 18 tests.
- `write-release-checksums.ps1 -Help` passed.
- Generated `SHA256SUMS.txt` over a staged artifact under `VoiceInk.Windows\artifacts\checksum-smoke`.
- Fixed strict-mode scalar handling so a single `ArtifactPath` works.
- `test-release-readiness.ps1` passed and printed the release checksum readiness reference.
- Full solution tests passed: Core 747, Infrastructure 266.
- Debug x64 solution build passed with 0 warnings and 0 errors.
- `git diff --check` passed with only LF-to-CRLF normalization warnings.
