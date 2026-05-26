# Windows Recording Shortcut Modes Implementation Plan

## Goal

Add macOS-style Toggle, Push to Talk, and Hybrid modes for Windows primary and secondary recording shortcuts.

## Task 1: Core Settings And Registration

- Add mode constants and normalization.
- Persist primary and secondary recording shortcut modes in `AppSettings`.
- Carry each recording mode in `GlobalShortcutRegistration`.
- Add focused Core tests for mode normalization and registration metadata.

## Task 2: Native Key-Up Handling

- Keep non-recording shortcuts on `RegisterHotKey`.
- Register recording shortcuts through a low-level keyboard hook so press and release transitions are available.
- Track active recording shortcut presses to suppress repeat keydown messages.
- Unhook whenever hotkeys are replaced or disposed.

## Task 3: WinUI Wiring

- Add mode selectors below primary and secondary shortcut fields.
- Save/load selector values with settings.
- Route recording shortcut press/release transitions through Toggle, Push to Talk, and Hybrid behavior.

## Task 4: Docs And Verification

- Update README, parity spec/tracker, focused tests, full tests, full Debug x64 build, and whitespace check.
- Commit the completed slice.

## Result

Completed on 2026-05-26 with source-runnable recording shortcut modes.
