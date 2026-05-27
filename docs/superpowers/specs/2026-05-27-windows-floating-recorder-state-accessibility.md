# Windows Floating Recorder State Accessibility

## Goal

Improve floating recorder parity polish by exposing the recorder's current state as one coherent UI Automation name.

## Source Of Truth

- The macOS mini/notch recorder is the primary always-visible recording surface and communicates state, elapsed time, processing, live text, and stop/cancel intent compactly.
- Windows already renders the same state through `FloatingRecorderPresenter`, but the recorder chrome needs a presenter-backed programmatic name for assistive technology.
- Microsoft WinUI accessibility guidance treats AutomationProperties names as the core programmatic label for controls and interactive surfaces.

## Requirements

- Add a Core `AccessibleName` for `FloatingRecorderViewState`.
- Include title, detail, elapsed time, footer hint, and live-preview presence when available.
- Bind the accessible name onto the WinUI recorder chrome when state is applied.
- Keep recorder behavior, no-activate window plumbing, and prompt/Power controls unchanged.
