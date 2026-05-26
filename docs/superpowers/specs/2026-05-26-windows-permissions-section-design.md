# Windows Permissions Section Design

VoiceInk for Windows should expose the same first-class Permissions route as the macOS sidebar while using Windows-native permission affordances and open-source behavior.

## Source Of Truth

The macOS `PermissionsView` shows a sidebar section with readiness cards for keyboard shortcut, microphone access, accessibility paste access, and screen recording context. Each card can refresh its status and route the user to the relevant app settings or OS settings page.

## Windows Adaptation

- Add `Permissions` to the Windows sidebar in the macOS order between `Power Mode` and `Audio Input`.
- Present a checklist-style readiness page for:
  - `Keyboard Shortcut`: ready when the primary shortcut is configured, with an action to the Windows Settings section.
  - `Microphone Access`: ready when at least one physical audio input is available, with an action to `ms-settings:privacy-microphone`.
  - `Text Insertion`: documents the current Windows paste/direct-text insertion mode, with an action to Settings.
  - `Screen Context`: ready when OCR context is off, full-screen OCR context is selected, or a positive OCR region is configured, with an action to Enhancement.
- Do not change Windows privacy settings automatically.
- Keep the readiness logic in Core so behavior is testable without WinUI automation.
- Use local-only diagnostics and status text; no telemetry, accounts, paywalls, or paid updater surfaces.

## Completion

Completed on 2026-05-26:

- Added `PermissionsReadinessPresenter` and readiness item/status models in Core.
- Added focused tests for macOS-order navigation and readiness card behavior.
- Added a WinUI `Permissions` section with summary `InfoBar`, refresh button, microphone privacy settings button, and per-card action routing.
- Updated Windows docs and completion tracking.
