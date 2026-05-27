# Windows Desktop Microphone Guidance Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Clarify the Windows desktop-app microphone privacy toggle in Audio Input health guidance.

---

## Task 1: Red Test

- [x] Update Audio Input device health test expectations for the desktop-app microphone privacy toggle.
- [x] Confirm the focused test fails before implementation.

## Task 2: Presenter Copy

- [x] Update `AudioInputDeviceHealthPresenter` privacy row copy.
- [x] Run the focused audio test.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Online grounding: Microsoft documents the Windows microphone privacy boundary for desktop apps as "Let desktop apps access your microphone".
- Red: focused Audio Input test failed because the existing privacy row used generic desktop microphone wording.
- Green: focused Audio Input test passed after naming the desktop-app microphone toggle.
- Full test: solution test passed with 777 Core tests and 266 Infrastructure tests.
- Build: Debug x64 solution build passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only line-ending normalization warnings.
