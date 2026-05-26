# Windows Mini Recorder Numeric Shortcuts Implementation Plan

## Goal

Port macOS mini-recorder numeric prompt and Power Mode shortcuts to Windows.

## Task 1: Core Mapping

- Add a Core presenter that maps Ctrl+digit to prompt slots and Alt+digit to Power Mode slots.
- Use `1..9,0` slot ordering.
- Add focused Core tests.

## Task 2: Native Detection

- Extend the existing low-level keyboard hook to detect exact Ctrl-only and Alt-only digit chords.
- Suppress repeat keydown events until key-up.
- Emit a contextual mini-recorder shortcut event.

## Task 3: Shell Routing

- Subscribe in `MainWindow`.
- Ignore events when floating recorder controls are unavailable.
- Route prompt slots to `SelectFloatingRecorderPromptAsync`.
- Route Power Mode slots to enabled rule choices, skipping Auto.

## Task 4: Docs And Verification

- Update README and project completion.
- Run focused tests, full tests, full Debug x64 build, and whitespace check.
- Commit the completed slice.

## Result

Completed on 2026-05-26 with source-runnable Ctrl/Alt digit mini-recorder shortcuts.
