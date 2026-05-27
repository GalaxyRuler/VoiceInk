# Windows MSIX Trust Store Readiness Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Make maintainer-gated MSIX trust prerequisites unambiguous in release readiness output without mutating the machine.

---

## Task 1: Red Packaging Test

- [x] Add a packaging asset assertion that expects `Cert:\LocalMachine\TrustedPeople` and `0x800B0109` in `test-release-readiness.ps1`.
- [x] Run the focused packaging test and confirm it fails against the current readiness output.

## Task 2: Read-Only Readiness Guidance

- [x] Update `test-release-readiness.ps1` checklist wording for the machine Trusted People store.
- [x] Preserve the no-create/no-import/no-trust-store-mutation boundary.

## Task 3: Verification And Commit

- [x] Run the focused packaging asset tests.
- [x] Run release readiness script.
- [x] Run full solution tests/build and whitespace check.
- [x] Update the project completion bar and commit the slice.

## Verification Notes

- Red focused packaging test failed because `test-release-readiness.ps1` did not mention `Cert:\LocalMachine\TrustedPeople`.
- Focused release readiness packaging assertion passed after adding the machine Trusted People store and `0x800B0109` guidance.
- Focused packaging asset suite passed: 14 tests.
- Release readiness script passed and printed the exact Local Machine Trusted People store plus `0x800B0109` guidance.
- Full solution tests passed: 929 tests.
- Full Debug x64 solution build passed: 0 warnings, 0 errors.
- `git diff --check` passed with only LF-to-CRLF working-copy warnings.
