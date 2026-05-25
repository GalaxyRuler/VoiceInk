# Windows Audio Input Status UI Design

## Goal

Make microphone selection state more legible by showing persistent, inline Audio Input page notices for active, rebound, unavailable, and missing-device states.

## External Grounding

Microsoft's WinUI `InfoBar` guidance describes it as a visible but non-intrusive inline control for informing users about changed app state. Audio input availability changes are exactly that kind of state: important enough to stay visible on the page, but not disruptive enough for a modal.

## Behavior

- Keep the existing `System Default` and custom microphone selector.
- Add a Core notice model so UI wording is testable without WinUI automation.
- Show a success notice when a custom saved microphone is active.
- Show an informational notice when using System Default with physical input devices available.
- Show a warning notice when the saved microphone was rebound by name because its device number changed.
- Show a warning notice when the saved microphone is unavailable and VoiceInk falls back to System Default.
- Show an error notice when no physical microphone choices are available.
- Preserve existing warnings returned to the shell status line.

## Non-Goals

- No new audio capture implementation.
- No device permission wizard in this slice.
- No automatic switch to another custom microphone when a saved microphone is unavailable.
