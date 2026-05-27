# Windows History Detail Accessibility

## Goal

Improve the embedded History view polish by making the search, transcript detail, playback-rate, and audio-player controls expose stable UI Automation names.

## Behavior

- The embedded History search box exposes `History search query`.
- Original, final, and enhanced transcript boxes expose distinct automation names.
- The History audio playback rate picker and audio player expose distinct automation names.
- Visible labels, layout, copy/retry/delete/export behavior, and dedicated History window behavior remain unchanged.

## Rationale

Windows accessibility and automated GUI smoke tooling both depend on stable UI Automation names. Explicit names make the embedded History workflow easier to inspect, test, and use with assistive technologies without changing the visual design.
