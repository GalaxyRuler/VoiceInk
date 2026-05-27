# Windows MSIX License File Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Include and validate GPL `LICENSE.txt` in MSIX package output.

---

## Task 1: Red Packaging Tests

- [x] Add app project assertions for the root `LICENSE` content item.
- [x] Add MSIX smoke script assertions for `LICENSE.txt` and GPL text.
- [x] Confirm focused tests fail before implementation.

## Task 2: Project And Validator Implementation

- [x] Include root `LICENSE` as `LICENSE.txt` in the Windows app output/package.
- [x] Validate `LICENSE.txt` and GPL text in `test-msix-package.ps1`.
- [x] Run focused packaging tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Online grounding: GNU GPL binary distribution guidance calls for a license copy with distributed binaries; MSBuild content items can copy external files into project output/package assets.
- Red: focused packaging tests failed because the app project lacked the root `LICENSE` content item and the MSIX smoke validator did not check `LICENSE.txt`.
- Green: focused MSIX packaging tests passed after adding the content item and smoke validator checks.
- Full test: solution test passed with 774 Core tests and 266 Infrastructure tests.
- Build: Debug x64 solution build passed with 0 warnings and 0 errors.
- Output smoke: Debug app output contains `LICENSE.txt` with GPL text.
- Whitespace: `git diff --check` passed with only line-ending normalization warnings.
