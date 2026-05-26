# Windows Settings Current State Summary

## Goal

Add a scan-friendly Settings summary that reflects the user's current paste, clipboard, recording feedback, and privacy cleanup choices.

## Source Of Truth

The macOS app presents settings as everyday behavior controls: shortcuts, sound feedback, clipboard restoration, paste behavior, local privacy cleanup, backups, and diagnostics. The Windows fork already has those controls; this slice adds a state summary so users can quickly verify how VoiceInk will behave before changing individual controls.

## Behavior

- Show summary rows near the Settings hero for:
  - Paste method.
  - Clipboard restoration.
  - Recording feedback.
  - Local cleanup/privacy retention.
- Derive all rows from `AppSettings`.
- Keep the existing action summary rows for shortcuts, data safety, backup, and diagnostics.
- Keep settings persistence unchanged.
- Keep summaries local-only and avoid telemetry, licensing, accounts, or commercial prompts.

## UI

The Settings page will render a compact list below the existing action summary. Each row has a title, value, detail text, and status badge. Values should be short enough to scan while details explain practical behavior.

## Testing

Add presenter tests that prove:

- The default settings produce expected current-state rows.
- Custom settings show direct text paste, disabled clipboard restoration, muted sound feedback, and cleanup retention values.

## Out Of Scope

- No new settings fields.
- No storage migration.
- No visual redesign of the entire Settings page.
- No commercial surfaces.
