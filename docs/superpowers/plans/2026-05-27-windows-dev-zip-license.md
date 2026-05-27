# Windows Dev ZIP License File Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Include and validate GPL license text in the Windows dev ZIP package.

---

## Task 1: Red Packaging Tests

- [x] Add packaging asset assertions for `LICENSE.txt` in `package-dev-zip.ps1`.
- [x] Add smoke validation assertions for `LICENSE.txt` in `test-dev-zip.ps1`.
- [x] Confirm focused packaging tests fail before implementation.

## Task 2: Script Implementation

- [x] Copy repository root `LICENSE` to `LICENSE.txt` in the dev ZIP package directory.
- [x] Validate `LICENSE.txt` exists and contains GPL text in the dev ZIP smoke script.
- [x] Run focused packaging tests and relevant script help/smoke path.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red: focused packaging tests failed because `package-dev-zip.ps1` did not mention `LICENSE` and `test-dev-zip.ps1` did not validate `LICENSE.txt`.
- Green: focused packaging tests passed after adding license packaging and validation.
- Smoke: `package-dev-zip.ps1` produced a fresh dev ZIP and `test-dev-zip.ps1` validated the extracted package including `LICENSE.txt`.
- Full test: solution test passed with 771 Core tests and 266 Infrastructure tests.
- Build: Debug x64 solution build passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only existing line-ending normalization warnings.
