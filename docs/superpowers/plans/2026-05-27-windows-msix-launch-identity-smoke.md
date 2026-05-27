# Windows MSIX Launch Identity Smoke Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add installed app identity verification to the signed MSIX smoke helper without running install/uninstall locally.

---

## Task 1: Red Packaging Test

- [x] Add packaging asset assertions for `Get-AppxPackageManifest`, `VoiceInk.Windows.App`, `PackageFamilyName`, and `shell:AppsFolder`.
- [x] Run the focused packaging test and confirm it fails against the current smoke helper.

## Task 2: Smoke Helper Implementation

- [x] Update `smoke-msix-install.ps1` to print the launch identity reference in the non-mutating plan.
- [x] Update the `-Execute` path to inspect the installed manifest and verify the application id before uninstall.
- [x] Update release readiness text to mention installed manifest identity verification.

## Task 3: Verification And Commit

- [x] Run focused packaging tests, release readiness script, full solution tests, Debug x64 build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [x] Commit the slice.
