# Windows Open-Source Update Guidance

## Goal

Adapt the macOS Sparkle update surface to the free/open-source Windows fork without adding a private updater, paid channel, background telemetry, account flow, or machine-mutating install action.

## Source Of Truth

- The macOS settings surface exposes update controls through Sparkle: auto-check updates and check for updates.
- Windows release distribution already has open-source packaging paths: signed MSIX/App Installer, WinGet manifest metadata, and source-built ZIP artifacts.
- Microsoft App Installer supports update checks through `.appinstaller` metadata, and WinGet supports explicit package upgrades through `winget upgrade --id`.

## Requirements

- The About / Open Source settings section shows Windows update guidance rows for:
  - App Installer optional on-launch checks when maintainers publish signed `.appinstaller` releases.
  - WinGet manual upgrade using `winget upgrade --id VoiceInk.VoiceInkWindows`.
  - Source builds and ZIP users downloading/rebuilding public release artifacts manually.
- The UI exposes a user-clicked `Check for Updates` action that opens the public GitHub releases page.
- The implementation must not add a background update checker, private release feed, telemetry, account dependency, paid updater channel, or automatic installer execution.
- Guidance rows include UI Automation names through the shared settings row accessibility model.

## Acceptance

- Focused tests fail before implementation because the update guidance presentation and About UI are missing.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
