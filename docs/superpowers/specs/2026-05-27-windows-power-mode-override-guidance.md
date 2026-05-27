# Windows Power Mode Override Guidance

## Goal

Make Power Mode override precedence visible on the Power Mode page: rule override fields temporarily layer over Settings while a mode is active, and blank override fields preserve the current Settings value.

## Source Of Truth

The macOS app presents Power Mode as context-aware workflow configuration. The Windows implementation already applies rule overrides through the core matcher; the page should explain that behavior so users can confidently mix global Settings with per-app or per-site rule overrides.

## Requirements

- `PowerModePagePresentation` must expose override guidance separately from match guidance.
- The guidance must state that rule overrides temporarily layer over Settings while the selected Power Mode is active.
- The guidance must state that blank override fields keep the current Settings value.
- The WinUI Power Mode page must render the guidance text.

## Non-Goals

- This slice does not change rule matching or override behavior.
- This slice does not add new override types.
