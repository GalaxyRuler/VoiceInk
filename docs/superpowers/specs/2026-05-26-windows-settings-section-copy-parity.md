# Windows Settings Section Copy Parity Spec

Last updated: 2026-05-26

## Goal

Make the Windows Settings page easier to scan and closer to macOS VoiceInk by adding UI-independent section copy for high-use settings groups.

## Source of Truth

- `VoiceInk/Views/Settings/SettingsView.swift`
- `VoiceInk/Views/Settings/DiagnosticsSettingsView.swift`
- VoiceInk public docs for General Settings, Shortcuts, and Audio Input.

## Windows Design

- Add a Core presenter for Settings section headings and concise descriptions.
- Wire descriptions into the existing WinUI Settings page without changing settings behavior.
- Preserve open-source replacements: update checks are not commercialized, diagnostics remain local-only, and backups exclude secrets.

## Acceptance Criteria

- Settings page has macOS-aligned section labels/descriptions for Shortcuts, Recording Feedback, Interface, Clipboard, Cleanup, Privacy, General, Backup, and Diagnostics.
- Presenter behavior is unit tested.
- Existing save/apply/import/export/privacy behavior remains unchanged.
