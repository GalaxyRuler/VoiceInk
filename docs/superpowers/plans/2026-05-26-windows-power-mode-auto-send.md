# Windows Power Mode Auto-Send Implementation Plan

**Goal:** Add mac-compatible Power Mode auto-send keys to Windows rules and fire the selected key after successful dictation insertion.

**Tech Stack:** .NET 10, WinUI 3, user32 `SendInput`, xUnit.

## Task 1: Add Core Rule And Dictation Tests

Add failing tests for `PowerModeRule.AutoSendKey`, `PowerModeResolution.AutoSendKey`, and dictation completion invoking an auto-send service only after insertion succeeds.

Status: completed.

## Task 2: Implement Core Contract

Add `PowerModeAutoSendKey`, JSON conversion, `IPowerModeAutoSendService`, resolution exposure, and `DictationController` wiring.

Status: completed.

## Task 3: Add Persistence Coverage

Add settings and backup tests requiring mac-compatible raw strings such as `commandEnter` and `shiftEnter`.

Status: completed.

## Task 4: Wire Native And UI

Add a Win32 `SendInput` auto-send service and expose the selector in the Power Mode editor with tested display choices.

Status: completed.

## Task 5: Document And Verify

Update the completion tracker, run focused tests, run app build, run full solution tests/build, inspect diff, and commit.

Status: completed.
