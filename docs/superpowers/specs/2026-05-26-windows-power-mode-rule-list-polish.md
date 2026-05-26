# Windows Power Mode Rule List Polish Spec

Date: 2026-05-26

## Goal

Make the Windows Power Mode page easier to scan by replacing flat rule strings with presenter-backed rows that summarize match targets, overrides, shortcuts, and enabled state.

## Source Of Truth

VoiceInk Power Mode docs describe app/site-specific configurations and quick manual switching from the recorder and shortcuts. The Windows implementation already supports rule matching, shortcuts, explicit selection, and settings overlays; this slice improves page clarity without changing behavior.

## Windows Behavior

- Extend the Core `PowerModePagePresenter` with manual switching guidance and rule rows.
- Each row shows:
  - title from icon/name
  - target summary for Default, process, title, and browser URL patterns
  - override summary count and key override types
  - shortcut summary
  - enabled/disabled badge
- Keep selection order identical to the stored rule order.
- Do not change Power Mode matching, shortcut registration, or persistence.
- Keep the slice open-source and local-only.

