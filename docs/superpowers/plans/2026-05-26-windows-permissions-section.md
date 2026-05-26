# Windows Permissions Section Implementation Plan

## Goal

Port the macOS Permissions sidebar surface to the Windows fork as a safe, open-source readiness page.

## Task 1: Core Presenter

- Add Core models for permission readiness items and summary status.
- Build readiness from persisted settings plus available audio-input state.
- Cover shortcut, microphone, text insertion, and screen context.
- Verify with focused Core tests.

## Task 2: Shell Navigation

- Insert `Permissions` in the shell navigation presenter between `Power Mode` and `Audio Input`.
- Update the macOS-order navigation test.

## Task 3: WinUI Surface

- Add a hidden `PermissionsSectionPanel` to `MainWindow.xaml`.
- Add summary `InfoBar`, refresh/open settings buttons, and a list of permission cards.
- Route card actions to Windows microphone settings or the relevant VoiceInk section.
- Refresh the page when selected.

## Task 4: Docs And Verification

- Update README and the completion bar.
- Run focused tests, app build, full solution tests, full Debug x64 build, and whitespace check.
- Commit the completed slice.

## Result

Completed on 2026-05-26 with the `Permissions` route source-runnable in the WinUI shell.
