# Windows Transcribe Audio Format Parity Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Match the macOS Transcribe Audio extension allow-list while keeping Windows decoder fallback behavior.

---

## Task 1: Red Queue Test

- [x] Add Core queue tests that expect macOS-supported `.aiff`, `.caf`, `.amr`, `.ogg`, `.oga`, and `.opus` files to be accepted.
- [x] Run the focused queue test and confirm it fails because the current allow-list skips them.

## Task 2: Allow-List Update

- [x] Update `AudioFileQueueService` supported extensions.
- [x] Preserve existing Windows-specific common media extensions.
- [x] Rely on the existing Media Foundation import error path for codec/decoder failures.

## Task 3: Verification And Commit

- [x] Run focused audio-file queue tests.
- [x] Run full solution tests/build and whitespace check.
- [x] Update the project completion bar and commit the slice.

## Verification Notes

- Red focused queue test failed for all six added macOS-format extensions because the queue allow-list skipped them.
- Focused audio-file queue tests passed: 11 tests.
- First full solution test attempt hit a transient Microsoft Defender file lock while writing the Infrastructure test assembly; Debug x64 build still passed.
- Full solution test rerun passed: 940 tests.
- Full Debug x64 solution build passed: 0 warnings, 0 errors.
- `git diff --check` passed with only LF-to-CRLF working-copy warnings.
