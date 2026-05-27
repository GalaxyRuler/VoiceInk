# Windows History Text Variants Spec

## Intent

Make the Windows History detail/analysis panel clearer about which text variants are available for copy, export, retry, and re-enhancement.

## Requirements

- Add a Core-presented History analysis row named `Text Variants`.
- Completed enhanced items should show original, enhanced, and final text availability.
- Completed non-enhanced items should show original/final availability.
- Failed or canceled items should conservatively show fallback-only availability.
- Preserve history storage, copy selection, retry, re-enhance, CSV export, audio playback, and privacy cleanup behavior.

## Open-Source Boundary

This is local history presentation. It adds no telemetry, account flow, licensing, paid gate, or commercial surface.

## Verification

- Focused Core History analysis presenter tests.
