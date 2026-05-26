# Windows OCR Display Picker Spec

Date: 2026-05-26

## Goal

Refine the Windows screen OCR region picker for multi-monitor setups by letting users choose which display should receive the fullscreen selection overlay.

## Grounding

- Microsoft documents Windows multi-monitor desktops as a virtual screen with display-specific coordinates.
- Windows App SDK exposes display/window placement primitives for targeting a window to a display area.
- VoiceInk already stores OCR regions as absolute screen coordinates, so the Windows refinement should preserve those coordinates and only improve the display-targeting workflow.

## Windows Behavior

- Show an OCR display selector next to the existing `Select Region` action.
- Populate the selector from Windows display enumeration, sorted with the primary display first.
- Display each monitor with a scan-friendly label containing primary status, dimensions, and virtual-screen origin.
- Open the OCR region picker on the selected display where practical.
- Keep selected OCR region persistence unchanged: left, top, width, and height remain absolute virtual-screen coordinates.
- If display enumeration is unavailable, keep the existing picker path available.

## Open-Source Boundary

No telemetry, cloud calls, paid permissions, account flows, or commercial capture integrations are added. Screen OCR remains opt-in and local.
