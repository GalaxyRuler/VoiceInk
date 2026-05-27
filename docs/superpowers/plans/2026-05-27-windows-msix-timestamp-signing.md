# Windows MSIX Timestamp Signing Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add safe timestamped signing support to the maintainer-gated MSIX packaging path.

---

## Task 1: Red Packaging Tests

- [x] Add a focused packaging asset test that expects `TimestampServerUrl`, `AppxPackageSigningTimestampServerUrl`, `AppxPackageSigningTimestampDigestAlgorithm`, the default timestamp URL, SHA-256 digest wording, and no certificate-store mutation.
- [x] Confirm the test fails before implementation.
- [x] Add release-readiness coverage for the timestamp parameter and default URL.
- [x] Confirm that release-readiness test fails before implementation.

## Task 2: Script And Docs

- [x] Add `TimestampServerUrl` to `package-msix.ps1` with the Microsoft timestamp URL default.
- [x] Add `TimestampDigestAlgorithm` to `package-msix.ps1` with the SHA-256 default.
- [x] Validate the timestamp as an absolute URI.
- [x] Pass the value to MSIX signing through `AppxPackageSigningTimestampServerUrl`.
- [x] Pass the digest algorithm to MSIX signing through `AppxPackageSigningTimestampDigestAlgorithm`.
- [x] Print the value in preflight and release-readiness output.
- [x] Update README, spec, plan, and project completion tracking.

## Task 3: Verification And Commit

- [x] Run focused packaging tests.
- [x] Run `package-msix.ps1 -Help`.
- [x] Run `package-msix.ps1 -Preflight`.
- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red packaging script test failed because `package-msix.ps1` did not mention `TimestampServerUrl`.
- Red release-readiness test failed because `test-release-readiness.ps1` did not mention `TimestampServerUrl`.
- Focused packaging tests passed: 15 tests.
- `package-msix.ps1 -Help`, `package-msix.ps1 -Preflight`, and `test-release-readiness.ps1` passed without signing or installing.
- Full solution tests passed: Core 733, Infrastructure 266.
- Debug x64 solution build passed with 0 warnings and 0 errors.
- `git diff --check` passed with only existing LF-to-CRLF normalization warnings.
