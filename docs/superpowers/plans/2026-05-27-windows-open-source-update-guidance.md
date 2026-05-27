# Windows Open-Source Update Guidance Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add macOS-inspired update affordances as open-source Windows release guidance.

---

## Task 1: Red Tests

- [x] Add settings presenter expectations for App Installer, WinGet, and source-build update guidance rows.
- [x] Add static XAML/code coverage for the About update guidance list, accessible row binding, and public releases button.
- [x] Confirm focused tests fail before implementation.

## Task 2: Presenter And UI Implementation

- [x] Add `SettingsUpdateGuidanceRow` to the Core settings presentation model.
- [x] Bind update guidance rows in the About / Open Source settings section.
- [x] Add a user-clicked `Check for Updates` button that opens the public GitHub releases page.
- [x] Run focused tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Online grounding: Microsoft documents App Installer update checks through `.appinstaller` update settings and WinGet upgrades through `winget upgrade --id`.
- Red: focused Core tests failed because `SettingsSectionPresentation` had no `UpdateGuidanceRows` property.
- Green: focused Core tests passed after adding presenter rows and the About UI wiring.
- Full test: solution test passed with 775 Core tests and 266 Infrastructure tests.
- Build: Debug x64 solution build passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only line-ending normalization warnings.
