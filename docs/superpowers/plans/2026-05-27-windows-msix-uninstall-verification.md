# Windows MSIX Uninstall Verification Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Tighten the maintainer-gated MSIX smoke helper so uninstall cleanup is explicitly verified.

---

## Task 1: Red Packaging Test

- [x] Add a packaging asset test that expects post-uninstall `Get-AppxPackage` verification.
- [x] Run the focused packaging test and confirm it fails because the helper does not verify cleanup yet.

## Task 2: Smoke Helper Implementation

- [x] Update `smoke-msix-install.ps1` to query after `Remove-AppxPackage`.
- [x] Throw if the installed package name still resolves after uninstall.
- [x] Keep the default path plan-only and certificate-safe.

## Task 3: Verification And Commit

- [x] Run focused packaging tests, full solution tests/build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [x] Update the project completion bar and commit the slice.

## Verification Notes

- Red focused packaging test failed because `smoke-msix-install.ps1` did not verify uninstall cleanup.
- Focused single packaging test passed after implementation.
- Focused packaging asset suite passed: 14 tests.
- Full solution tests passed: 923 tests.
- Full Debug x64 solution build passed: 0 warnings, 0 errors.
- `git diff --check` passed with only existing LF-to-CRLF working-copy warnings.
