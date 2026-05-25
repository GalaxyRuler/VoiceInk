# Windows History Copy Actions Design

## Context

The macOS VoiceInk history views expose copy buttons on transcript text and AI request details. Windows currently shows Original, Final, and Enhanced text in read-only boxes, but users must manually select text. The Windows fork should match the original app's quick copy workflow while staying local-only.

## Goals

- Add one-click History copy actions for Original, Final, Enhanced, and AI Request.
- Copy only local text already visible or persisted in History.
- Disable unavailable actions when the selected row has no corresponding text.
- Keep the behavior UI-light and test the selection rules in Core.

## Non-goals

- Do not add paste-at-cursor behavior for selected history rows in this slice.
- Do not add batch copy.
- Do not add commercial telemetry, accounts, or cloud sync.
- Do not build the dedicated separate History window in this slice.

## Architecture

Add a small Core selector, `HistoryCopyTextSelector`, plus a `HistoryCopyTextKind` enum. The selector returns a success flag, text, and message for each copy kind. `MainWindow` calls the selector and uses the existing clipboard copy service to place text on the Windows clipboard.

## Verification

- Unit tests cover original, final, enhanced, full AI request, and unavailable text.
- App project builds.
- Full solution tests/build pass before commit.
