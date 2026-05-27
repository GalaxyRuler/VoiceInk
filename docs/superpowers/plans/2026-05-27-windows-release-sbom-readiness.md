# Windows Release SBOM Readiness Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add non-mutating SBOM readiness guidance to release packaging checks.

---

## Task 1: Red Packaging Test

- [x] Add release-readiness assertions for SBOM readiness text.
- [x] Confirm focused packaging test fails before implementation.

## Task 2: Readiness And Docs

- [x] Add SBOM readiness section to `test-release-readiness.ps1`.
- [x] Update README packaging notes.
- [x] Update project completion tracker.
- [x] Run focused packaging test and readiness script.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red: `dotnet test VoiceInk.Windows.Core.Tests.csproj --filter "WindowsPackagingAssetsTests.ReleaseReadinessScript_PrintsNonMutatingPackagingChecklist"` failed before the SBOM readiness reference existed.
- Green: same focused packaging test passed, 1 test.
- Readiness: `VoiceInk.Windows\scripts\test-release-readiness.ps1` passed and printed the SBOM readiness section.
- Full: `dotnet test VoiceInk.Windows.sln` passed, 770 Core tests and 266 Infrastructure tests.
- Build: `dotnet build VoiceInk.Windows.sln -c Debug -p:Platform=x64` passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only expected LF-to-CRLF working-copy warnings.
