# Windows Shortcut Recorder Implementation Plan

**Goal:** Add normalized shortcut capture to existing shortcut text boxes.

**Tech Stack:** .NET 10, WinUI 3 `KeyDown`, user32 `GetKeyState`, xUnit.

## Task 1: Add Capture Formatter Tests

Add tests for formatting a captured modifier+key combination and rejecting modifier-only or Windows-key captures.

Status: completed.

## Task 2: Implement Capture Formatter

Add `GlobalShortcut.TryCreateFromKeyCapture` so UI capture and typed parsing share the same normalized `GlobalShortcut` model.

Status: completed.

## Task 3: Wire Shortcut Fields

Attach `KeyDown` to shortcut text boxes, format captured combinations into the focused field, clear with bare Escape, and keep typed values available.

Status: completed.

## Task 4: Document And Verify

Update README and completion tracker, then run focused tests, full solution tests, Debug x64 build, and `git diff --check`.

Status: completed.
