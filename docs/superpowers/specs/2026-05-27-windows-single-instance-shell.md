# Windows Single Instance Shell

## Goal

Close a shell lifecycle parity gap by ensuring VoiceInk for Windows behaves as a single tray/dictation app instance.

## Source Of Truth

- The macOS menu-bar app model naturally avoids duplicate menu-bar/hotkey/recorder ownership for normal launches.
- Windows App SDK / WinUI apps are multi-instance by default, so the Windows fork needs explicit single-instance activation handling.

## Online Grounding

- Microsoft documents `AppInstance.FindOrRegisterForKey` and activation redirection for single-instanced WinUI apps: https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/applifecycle/applifecycle-single-instance

## Requirements

- Register a stable VoiceInk app instance key before creating the main window.
- Redirect secondary activations to the first instance and exit the duplicate process.
- Restore and activate the existing main window on redirected activation, including when the first instance was launched hidden to tray.
- Add static tests for the lifecycle wiring.
- Keep login-startup hidden-to-tray behavior unchanged for the first instance.

## Acceptance

- Focused static shell lifecycle tests fail before implementation.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
