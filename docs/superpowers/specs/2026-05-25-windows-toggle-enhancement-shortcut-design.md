# Windows Toggle Enhancement Shortcut Design

Date: 2026-05-25

## Goal

Add a configurable global shortcut that toggles AI enhancement on or off without opening Settings, matching the macOS shortcut parity area while keeping the Windows implementation local and free/open-source.

## Source Of Truth

- macOS parity requirement: global shortcuts include toggling enhancement.
- Windows current state: optional global shortcut settings already exist for paste last, retry last, cancel recording, open History, and quick add to Dictionary.
- External grounding: Windows global shortcuts are registered with `RegisterHotKey` and delivered through `WM_HOTKEY`; WinUI keyboard accelerators are app-local and do not replace the existing global shortcut service.

## Behavior

- Settings > Shortcuts adds a `Toggle Enhancement` optional shortcut text box.
- Blank means disabled, matching the other optional shortcuts.
- The shortcut participates in existing parsing, normalization, registration, and duplicate detection.
- Pressing the shortcut while onboarding is closed flips `AppSettings.IsEnhancementEnabled`, saves settings, updates the Enhancement checkbox, and refreshes UI state with `Enhancement enabled` or `Enhancement disabled`.
- The action is ignored while another operation is active, matching other settings-changing commands.
- The shortcut does not require provider credentials. If enhancement is enabled but endpoint/model configuration is incomplete, the existing enhancement pipeline/settings validation behavior still handles actual enhancement attempts.

## Persistence And Backup

- Add `ToggleEnhancementHotkey` to `AppSettings`.
- JSON settings persistence stores the field naturally with the existing settings record.
- Settings backup export/import preserves the shortcut like other shortcut fields.

## Tests

- Shortcut registration includes `ToggleEnhancement` when configured.
- Duplicate detection reports `Toggle Enhancement already uses ...`.
- JSON settings round-trip preserves the new field.
- Settings backup round-trip preserves the new field.

## Non-Goals

- No shortcut recorder UI in this slice.
- No Power Mode shortcut family in this slice.
- No commercial gates or telemetry.
